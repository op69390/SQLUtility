using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Diagnostics;
using System.Globalization;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Core.Execution;
using SQLUtility.Data.Securite;

namespace SQLUtility.Data.Providers
{
    /// <summary>
    /// Implémentation IBM DB2 for i (AS/400) via ODBC (pilote « IBM i Access ODBC Driver »).
    /// ODBC est utilisé par défaut pour compiler sans la DLL IBM ; le basculement vers
    /// IBM.Data.DB2.iSeries se fera ultérieurement, en isolant l'accès dans cette classe.
    /// </summary>
    public sealed class Db2ForIConnexionProvider : IConnexionProvider
    {
        /// <summary>Nom du pilote ODBC IBM i Access par défaut.</summary>
        public const string PiloteOdbcParDefaut = "IBM i Access ODBC Driver";

        public TypeSgbd Type { get { return TypeSgbd.Db2ForI; } }

        public ResultatTestConnexion TesterConnexion(ConnexionProfil profil)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));

            Stopwatch chrono = Stopwatch.StartNew();
            try
            {
                using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
                {
                    connexion.Open();
                    chrono.Stop();
                    return ResultatTestConnexion.Reussi(chrono.Elapsed);
                }
            }
            catch (Exception ex)
            {
                chrono.Stop();
                return ResultatTestConnexion.Echec(ex.Message);
            }
        }

        public IReadOnlyList<string> ListerSchemas(ConnexionProfil profil)
        {
            // EF-07 : schémas (bibliothèques) accessibles.
            if (profil == null) throw new ArgumentNullException(nameof(profil));

            const string sql = "SELECT SCHEMA_NAME FROM QSYS2.SYSSCHEMAS ORDER BY SCHEMA_NAME";
            List<string> schemas = new List<string>();
            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                connexion.Open();
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        string nom = LireTexte(lecteur, 0);
                        if (!string.IsNullOrEmpty(nom)) schemas.Add(nom);
                    }
                }
            }
            return schemas;
        }

        public IReadOnlyList<TableInfo> ListerTables(ConnexionProfil profil, string schema)
        {
            // EF-07 : tables et vues d'un schéma.
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrEmpty(schema)) throw new ArgumentNullException(nameof(schema));

            const string sql =
                "SELECT TABLE_NAME, SYSTEM_TABLE_NAME, TABLE_TYPE, TABLE_TEXT " +
                "FROM QSYS2.SYSTABLES WHERE TABLE_SCHEMA = ? ORDER BY TABLE_NAME";
            string schemaMaj = schema.ToUpperInvariant();

            List<TableInfo> tables = new List<TableInfo>();
            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                commande.Parameters.Add("schema", OdbcType.VarChar, 128).Value = schemaMaj;
                connexion.Open();
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        TableInfo info = new TableInfo();
                        info.Schema = schemaMaj;
                        info.Nom = LireTexte(lecteur, 0);
                        info.NomSysteme = LireTexte(lecteur, 1);
                        info.Type = LibelleTypeTable(LireTexte(lecteur, 2));
                        info.Libelle = LireTexte(lecteur, 3);
                        tables.Add(info);
                    }
                }
            }
            return tables;
        }

        public IReadOnlyList<ColonneInfo> ListerColonnes(ConnexionProfil profil, string schema, string table)
        {
            // EF-08 : colonnes d'une table (libellé, type, longueur, échelle).
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrEmpty(schema)) throw new ArgumentNullException(nameof(schema));
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException(nameof(table));

            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
            {
                connexion.Open();
                return LireColonnes(connexion, schema.ToUpperInvariant(), table.ToUpperInvariant());
            }
        }

        public StructureTable DecrireTable(ConnexionProfil profil, TableInfo table)
        {
            // EF-08 à EF-10 : colonnes, clés et relations, sur une seule connexion.
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrEmpty(table.Schema)) throw new ArgumentException("Schéma absent.", nameof(table));
            if (string.IsNullOrEmpty(table.Nom)) throw new ArgumentException("Nom de table absent.", nameof(table));

            string schema = table.Schema.ToUpperInvariant();
            string nom = table.Nom.ToUpperInvariant();
            ConstructeurStructureTable constructeur = new ConstructeurStructureTable();

            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
            {
                connexion.Open();

                // Les colonnes sont indispensables : une erreur ici remonte à l'appelant.
                List<ColonneInfo> colonnes = LireColonnes(connexion, schema, nom);

                // Clés et relations : un échec (droit insuffisant sur une vue du catalogue)
                // rend la description partielle sans l'empêcher.
                try
                {
                    LireCheminsAcces(connexion, schema, nom, constructeur);
                }
                catch (OdbcException ex)
                {
                    Trace.WriteLine("DecrireTable : clés illisibles pour " + schema + "." + nom + " : " + ex.Message);
                    constructeur.AjouterAvertissement("Clés et index non lus (QSYS2.SYSPARTITIONINDEXSTAT) : " + ex.Message);
                }

                try
                {
                    LireRelations(connexion, schema, nom, constructeur);
                }
                catch (OdbcException ex)
                {
                    Trace.WriteLine("DecrireTable : relations illisibles pour " + schema + "." + nom + " : " + ex.Message);
                    constructeur.AjouterAvertissement("Clés étrangères non lues (QSYS2.SYSREFCST) : " + ex.Message);
                }

                return constructeur.Construire(table, colonnes);
            }
        }

        private static List<ColonneInfo> LireColonnes(OdbcConnection connexion, string schema, string table)
        {
            const string sql =
                "SELECT COLUMN_NAME, SYSTEM_COLUMN_NAME, COLUMN_TEXT, DATA_TYPE, " +
                "LENGTH, COALESCE(NUMERIC_SCALE, 0), IS_NULLABLE, ORDINAL_POSITION " +
                "FROM QSYS2.SYSCOLUMNS WHERE TABLE_SCHEMA = ? AND TABLE_NAME = ? " +
                "ORDER BY ORDINAL_POSITION";

            List<ColonneInfo> colonnes = new List<ColonneInfo>();
            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                commande.Parameters.Add("schema", OdbcType.VarChar, 128).Value = schema;
                commande.Parameters.Add("table", OdbcType.VarChar, 128).Value = table;
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        ColonneInfo colonne = new ColonneInfo();
                        colonne.Nom = LireTexte(lecteur, 0);
                        colonne.NomSysteme = LireTexte(lecteur, 1);
                        colonne.Libelle = LireTexte(lecteur, 2);
                        colonne.TypeDonnee = LireTexte(lecteur, 3);
                        colonne.Longueur = LireEntier(lecteur, 4);
                        colonne.Echelle = LireEntier(lecteur, 5);
                        colonne.AccepteNull = string.Equals(LireTexte(lecteur, 6), "Y", StringComparison.OrdinalIgnoreCase);
                        colonne.Position = LireEntier(lecteur, 7);
                        colonnes.Add(colonne);
                    }
                }
            }
            return colonnes;
        }

        private static void LireCheminsAcces(OdbcConnection connexion, string schema, string table, ConstructeurStructureTable constructeur)
        {
            // EF-09 : tous les chemins d'accès à clé de la table, quelle que soit leur origine :
            // contraintes PRIMARY KEY / UNIQUE, clé du physique DDS, logiques et index SQL.
            // Une ligne par membre ou partition : DISTINCT, puis dédoublonnage dans le constructeur.
            // "UNIQUE" est un mot réservé : la colonne doit être délimitée.
            const string sql =
                "SELECT DISTINCT INDEX_SCHEMA, INDEX_NAME, INDEX_TYPE, \"UNIQUE\", SPARSE, DERIVED_KEY, COLUMN_NAMES " +
                "FROM QSYS2.SYSPARTITIONINDEXSTAT WHERE TABLE_SCHEMA = ? AND TABLE_NAME = ?";

            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                commande.Parameters.Add("schema", OdbcType.VarChar, 128).Value = schema;
                commande.Parameters.Add("table", OdbcType.VarChar, 128).Value = table;
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        constructeur.AjouterCheminAcces(
                            LireTexte(lecteur, 0), LireTexte(lecteur, 1), LireTexte(lecteur, 2), LireTexte(lecteur, 3),
                            LireTexte(lecteur, 4) == "1", LireTexte(lecteur, 5) == "1", LireTexte(lecteur, 6));
                    }
                }
            }
        }

        private static void LireRelations(OdbcConnection connexion, string schema, string table, ConstructeurStructureTable constructeur)
        {
            // EF-10 : clés étrangères déclarées où la table est enfant OU parente.
            // SYSKEYCST.ORDINAL_POSITION = position de la colonne dans la clé : elle apparie
            // chaque colonne enfant à la colonne de la clé unique parente référencée.
            const string sql =
                "SELECT R.CONSTRAINT_SCHEMA, R.CONSTRAINT_NAME, " +
                "E.TABLE_SCHEMA, E.TABLE_NAME, KE.COLUMN_NAME, " +
                "P.TABLE_SCHEMA, P.TABLE_NAME, KP.COLUMN_NAME, " +
                "R.UPDATE_RULE, R.DELETE_RULE " +
                "FROM QSYS2.SYSREFCST R " +
                "JOIN QSYS2.SYSCST E ON E.CONSTRAINT_SCHEMA = R.CONSTRAINT_SCHEMA AND E.CONSTRAINT_NAME = R.CONSTRAINT_NAME " +
                "JOIN QSYS2.SYSCST P ON P.CONSTRAINT_SCHEMA = R.UNIQUE_CONSTRAINT_SCHEMA AND P.CONSTRAINT_NAME = R.UNIQUE_CONSTRAINT_NAME " +
                "JOIN QSYS2.SYSKEYCST KE ON KE.CONSTRAINT_SCHEMA = R.CONSTRAINT_SCHEMA AND KE.CONSTRAINT_NAME = R.CONSTRAINT_NAME " +
                "JOIN QSYS2.SYSKEYCST KP ON KP.CONSTRAINT_SCHEMA = R.UNIQUE_CONSTRAINT_SCHEMA AND KP.CONSTRAINT_NAME = R.UNIQUE_CONSTRAINT_NAME " +
                "AND KP.ORDINAL_POSITION = KE.ORDINAL_POSITION " +
                "WHERE (E.TABLE_SCHEMA = ? AND E.TABLE_NAME = ?) OR (P.TABLE_SCHEMA = ? AND P.TABLE_NAME = ?) " +
                "ORDER BY R.CONSTRAINT_SCHEMA, R.CONSTRAINT_NAME, KE.ORDINAL_POSITION";

            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                // Paramètres ODBC positionnels : le couple schéma/table sert deux fois.
                commande.Parameters.Add("schemaEnfant", OdbcType.VarChar, 128).Value = schema;
                commande.Parameters.Add("tableEnfant", OdbcType.VarChar, 128).Value = table;
                commande.Parameters.Add("schemaParent", OdbcType.VarChar, 128).Value = schema;
                commande.Parameters.Add("tableParent", OdbcType.VarChar, 128).Value = table;
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        constructeur.AjouterColonneRelation(
                            LireTexte(lecteur, 0), LireTexte(lecteur, 1),
                            LireTexte(lecteur, 2), LireTexte(lecteur, 3), LireTexte(lecteur, 4),
                            LireTexte(lecteur, 5), LireTexte(lecteur, 6), LireTexte(lecteur, 7),
                            LireTexte(lecteur, 8), LireTexte(lecteur, 9));
                    }
                }
            }
        }

        public IReadOnlyList<string> ListerNomsColonnes(ConnexionProfil profil, string schema)
        {
            // EF-16 : noms de colonnes distincts d'un schéma (pour l'auto-complétion de l'éditeur).
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrEmpty(schema)) throw new ArgumentNullException(nameof(schema));

            const string sql =
                "SELECT DISTINCT COLUMN_NAME FROM QSYS2.SYSCOLUMNS " +
                "WHERE TABLE_SCHEMA = ? ORDER BY COLUMN_NAME";

            List<string> noms = new List<string>();
            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                commande.Parameters.Add("schema", OdbcType.VarChar, 128).Value = schema.ToUpperInvariant();
                connexion.Open();
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    while (lecteur.Read())
                    {
                        string nom = LireTexte(lecteur, 0);
                        if (!string.IsNullOrEmpty(nom)) noms.Add(nom);
                    }
                }
            }
            return noms;
        }

        // ---- Lecture défensive des valeurs du DataReader ----

        private static string LireTexte(OdbcDataReader lecteur, int index)
        {
            if (lecteur.IsDBNull(index)) return null;
            string valeur = Convert.ToString(lecteur.GetValue(index), CultureInfo.InvariantCulture);
            // Les colonnes CHAR du catalogue (noms système...) sont complétées d'espaces.
            return valeur == null ? null : valeur.TrimEnd();
        }

        private static int LireEntier(OdbcDataReader lecteur, int index)
        {
            if (lecteur.IsDBNull(index)) return 0;
            return Convert.ToInt32(lecteur.GetValue(index), CultureInfo.InvariantCulture);
        }

        private static string LibelleTypeTable(string code)
        {
            if (string.IsNullOrEmpty(code)) return code;
            switch (code.Trim().ToUpperInvariant())
            {
                case "P": return "Fichier physique";
                case "L": return "Fichier logique";
                case "T": return "Table";
                case "V": return "Vue";
                case "A": return "Alias";
                case "M": return "MQT";
                default: return code;
            }
        }

        public bool EstJournalisee(ConnexionProfil profil, string schema, string table)
        {
            // EF-30 : détection de l'état de journalisation via les services SQL IBM i.
            // Source : QSYS2.SYSPARTITIONSTAT. La table est considérée journalisée
            // uniquement si toutes ses partitions le sont (MIN(JOURNALED) = 'Y').
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrEmpty(schema)) throw new ArgumentNullException(nameof(schema));
            if (string.IsNullOrEmpty(table)) throw new ArgumentNullException(nameof(table));

            const string sql =
                "SELECT CASE WHEN COUNT(*) > 0 AND MIN(JOURNALED) = 'Y' THEN 1 ELSE 0 END " +
                "FROM QSYS2.SYSPARTITIONSTAT WHERE TABLE_SCHEMA = ? AND TABLE_NAME = ?";

            try
            {
                using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil)))
                using (OdbcCommand commande = new OdbcCommand(sql, connexion))
                {
                    // Paramètres ODBC positionnels, dans l'ordre des '?'. Noms en majuscules
                    // (catalogue IBM i ; les fichiers Atlas / MAPICS sont en majuscules).
                    commande.Parameters.Add("schema", OdbcType.VarChar, 128).Value = schema.ToUpperInvariant();
                    commande.Parameters.Add("table", OdbcType.VarChar, 128).Value = table.ToUpperInvariant();

                    connexion.Open();
                    object resultat = commande.ExecuteScalar();
                    return resultat != null && resultat != DBNull.Value && Convert.ToInt32(resultat) == 1;
                }
            }
            catch (OdbcException ex)
            {
                // Choix conservateur (EF-30) : si l'état est indéterminable (table absente,
                // droit insuffisant sur le catalogue, connexion impossible), on retourne false
                // pour déclencher l'avertissement d'irréversibilité. Ne distingue pas ces cas.
                Trace.WriteLine("EstJournalisee : état indéterminé pour " + schema + "." + table + " : " + ex.Message);
                return false;
            }
        }

        // ---- Exécution (EF-22 à EF-32) ----

        /// <summary>Délai d'exécution d'une instruction, en secondes (fichiers Atlas volumineux).</summary>
        public const int DelaiExecutionSecondes = 300;

        public ResultatRequete ExecuterSelect(ConnexionProfil profil, string sql, int limiteLignes)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            if (limiteLignes < 1) throw new ArgumentOutOfRangeException(nameof(limiteLignes));

            List<string> colonnes = new List<string>();
            List<object[]> lignes = new List<object[]>();
            bool tronque = false;

            // Lecture seule forcée : une consultation ne doit jamais pouvoir modifier.
            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil, true, null)))
            using (OdbcCommand commande = new OdbcCommand(sql, connexion))
            {
                commande.CommandTimeout = DelaiExecutionSecondes;
                connexion.Open();
                using (OdbcDataReader lecteur = commande.ExecuteReader())
                {
                    for (int i = 0; i < lecteur.FieldCount; i++)
                        colonnes.Add(lecteur.GetName(i));

                    while (lecteur.Read())
                    {
                        if (lignes.Count >= limiteLignes)
                        {
                            tronque = true;
                            break;
                        }
                        object[] valeurs = new object[lecteur.FieldCount];
                        for (int i = 0; i < valeurs.Length; i++)
                            valeurs[i] = LireValeur(lecteur, i);
                        lignes.Add(valeurs);
                    }
                }
            }
            return new ResultatRequete(colonnes, lignes, tronque);
        }

        public long CompterLignes(ConnexionProfil profil, string sqlComptage)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrWhiteSpace(sqlComptage)) throw new ArgumentNullException(nameof(sqlComptage));

            using (OdbcConnection connexion = new OdbcConnection(ConstruireChaineConnexion(profil, true, null)))
            using (OdbcCommand commande = new OdbcCommand(sqlComptage, connexion))
            {
                commande.CommandTimeout = DelaiExecutionSecondes;
                connexion.Open();
                object resultat = commande.ExecuteScalar();
                return resultat == null || resultat == DBNull.Value ? 0 : Convert.ToInt64(resultat, CultureInfo.InvariantCulture);
            }
        }

        public ITransactionExecution ExecuterModification(ConnexionProfil profil, string sql, bool sousControleValidation)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            // EF-06 : défense en profondeur, l'interface bloque déjà ce cas.
            if (profil.LectureSeule)
                throw new InvalidOperationException("Le profil « " + profil.Nom + " » est en lecture seule : modification refusée.");

            // Sans contrôle de validation : CommitMode 0 (*NONE). Sinon le mode par défaut du
            // pilote (*CHG) provoquerait SQL7008 sur un fichier non journalisé.
            OdbcConnection connexion = new OdbcConnection(
                ConstruireChaineConnexion(profil, false, sousControleValidation ? null : "0"));
            OdbcTransaction transaction = null;
            try
            {
                connexion.Open();
                // ReadCommitted = *CS : lit les seules données validées, verrouille les lignes modifiées.
                if (sousControleValidation)
                    transaction = connexion.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                int lignes;
                using (OdbcCommand commande = new OdbcCommand(sql, connexion, transaction))
                {
                    commande.CommandTimeout = DelaiExecutionSecondes;
                    lignes = commande.ExecuteNonQuery();
                }

                if (transaction != null)
                    return new TransactionExecutionOdbc(connexion, transaction, lignes);

                connexion.Dispose();
                return new TransactionExecutionOdbc(lignes);
            }
            catch
            {
                if (transaction != null)
                {
                    try { transaction.Rollback(); }
                    catch (Exception ex) { Trace.WriteLine("ExecuterModification : ROLLBACK après erreur en échec : " + ex.Message); }
                    transaction.Dispose();
                }
                connexion.Dispose();
                throw;
            }
        }

        private static object LireValeur(OdbcDataReader lecteur, int index)
        {
            if (lecteur.IsDBNull(index)) return null;
            try
            {
                return lecteur.GetValue(index);
            }
            catch (Exception ex)
            {
                // Type que le pilote ne sait pas convertir (DECFLOAT, binaire...) : la ligne reste lisible.
                return "(illisible : " + ex.Message + ")";
            }
        }

        private static string ConstruireChaineConnexion(ConnexionProfil profil)
        {
            return ConstruireChaineConnexion(profil, false, null);
        }

        /// <summary>
        /// Construit la chaîne de connexion ODBC IBM i Access à partir du profil.
        /// La lecture seule (EF-06) est appliquée via l'attribut ConnectionType ;
        /// <paramref name="forcerLectureSeule"/> l'impose quel que soit le profil.
        /// </summary>
        /// <param name="modeValidation">Attribut CommitMode du pilote (0 = *NONE), ou null pour le défaut.</param>
        private static string ConstruireChaineConnexion(ConnexionProfil profil, bool forcerLectureSeule, string modeValidation)
        {
            OdbcConnectionStringBuilder builder = new OdbcConnectionStringBuilder();
            builder.Driver = PiloteOdbcParDefaut;
            builder["System"] = profil.Serveur ?? string.Empty;

            if (!string.IsNullOrEmpty(profil.Utilisateur))
                builder["Uid"] = profil.Utilisateur;

            string secret = string.IsNullOrEmpty(profil.SecretChiffre)
                ? string.Empty
                : ProtecteurSecret.Dechiffrer(profil.SecretChiffre);
            if (!string.IsNullOrEmpty(secret))
                builder["Pwd"] = secret;

            if (!string.IsNullOrEmpty(profil.BibliothequeParDefaut))
                builder["DefaultLibraries"] = profil.BibliothequeParDefaut;

            // ConnectionType : 0 = lecture/écriture, 1 = lecture/appel, 2 = lecture seule.
            builder["ConnectionType"] = (profil.LectureSeule || forcerLectureSeule) ? "2" : "0";

            if (!string.IsNullOrEmpty(modeValidation))
                builder["CommitMode"] = modeValidation;

            return builder.ConnectionString;
        }
    }
}

using System;
using System.Collections.Generic;

namespace SQLUtility.Core.Catalogue
{
    /// <summary>
    /// Assemble une <see cref="StructureTable"/> à partir des lignes brutes du catalogue
    /// (QSYS2.SYSPARTITIONINDEXSTAT pour les clés, SYSREFCST / SYSCST / SYSKEYCST pour
    /// les relations). Sans dépendance technique : testable sans AS/400. (EF-09 / EF-10)
    /// </summary>
    public sealed class ConstructeurStructureTable
    {
        private readonly List<CleInfo> pCles = new List<CleInfo>();
        private readonly HashSet<string> pClesVues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<RelationInfo> pRelations = new List<RelationInfo>();
        private readonly Dictionary<string, RelationInfo> pRelationsParNom = new Dictionary<string, RelationInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> pAvertissements = new List<string>();

        /// <summary>Signale une lecture du catalogue en échec : la description sera partielle.</summary>
        public void AjouterAvertissement(string message)
        {
            if (!string.IsNullOrEmpty(message)) pAvertissements.Add(message);
        }

        /// <summary>
        /// Ajoute une ligne de QSYS2.SYSPARTITIONINDEXSTAT. Les doublons (une ligne par
        /// membre ou partition) et les contraintes référentielles sont ignorés.
        /// </summary>
        /// <param name="typeIndex">INDEX_TYPE : INDEX, LOGICAL, PHYSICAL, PRIMARY KEY, UNIQUE, REFERENTIAL.</param>
        /// <param name="codeUnique">UNIQUE : 0 et 1 = unique, 2 à 4 = non unique.</param>
        /// <param name="listeColonnes">COLUMN_NAMES : noms séparés par des virgules, terminés par « ... » si tronqués.</param>
        public void AjouterCheminAcces(string schemaIndex, string nomIndex, string typeIndex, string codeUnique,
            bool partiel, bool cleDerivee, string listeColonnes)
        {
            string type = (typeIndex ?? string.Empty).Trim().ToUpperInvariant();
            if (type == "REFERENTIAL") return; // Présentée dans les relations.

            string identifiant = schemaIndex + "." + nomIndex;
            if (!pClesVues.Add(identifiant)) return;

            OrigineCle origine = OrigineDepuisType(type);
            string code = (codeUnique ?? string.Empty).Trim();
            bool unique = origine == OrigineCle.ClePrimaire || origine == OrigineCle.ContrainteUnique
                || code == "0" || code == "1";

            bool complete;
            List<string> colonnes = DecouperColonnes(listeColonnes, out complete);
            pCles.Add(new CleInfo(schemaIndex, nomIndex, origine, unique, partiel || cleDerivee, complete, colonnes));
        }

        /// <summary>
        /// Ajoute une colonne d'une clé étrangère. Les lignes d'une même contrainte doivent
        /// arriver dans l'ordre de la clé (ORDINAL_POSITION de SYSKEYCST).
        /// </summary>
        public void AjouterColonneRelation(string schemaContrainte, string nomContrainte,
            string schemaEnfant, string tableEnfant, string colonneEnfant,
            string schemaParent, string tableParent, string colonneParent,
            string regleMiseAJour, string regleSuppression)
        {
            string identifiant = schemaContrainte + "." + nomContrainte;
            RelationInfo relation;
            if (!pRelationsParNom.TryGetValue(identifiant, out relation))
            {
                relation = new RelationInfo();
                relation.Schema = schemaContrainte;
                relation.Nom = nomContrainte;
                relation.SchemaEnfant = schemaEnfant;
                relation.TableEnfant = tableEnfant;
                relation.SchemaParent = schemaParent;
                relation.TableParent = tableParent;
                relation.RegleMiseAJour = regleMiseAJour;
                relation.RegleSuppression = regleSuppression;
                relation.EstDeclaree = true;
                pRelationsParNom.Add(identifiant, relation);
                pRelations.Add(relation);
            }
            relation.ColonnesEnfant.Add(colonneEnfant);
            relation.ColonnesParent.Add(colonneParent);
        }

        /// <summary>
        /// Construit la structure : normalise les noms de colonnes des clés, choisit la clé
        /// d'identification et annote les colonnes (rang de clé, référence étrangère).
        /// </summary>
        public StructureTable Construire(TableInfo table, IList<ColonneInfo> colonnes)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            List<ColonneInfo> listeColonnes = new List<ColonneInfo>(colonnes ?? new List<ColonneInfo>());

            // Les clés DDS peuvent citer le nom système : on les ramène au nom SQL de la table.
            List<KeyValuePair<int, CleInfo>> indexees = new List<KeyValuePair<int, CleInfo>>();
            for (int i = 0; i < pCles.Count; i++)
                indexees.Add(new KeyValuePair<int, CleInfo>(i, NormaliserColonnes(pCles[i], listeColonnes)));

            // Tri stable : clés identifiantes d'abord, puis origine préférée, puis clé la plus courte.
            indexees.Sort(delegate (KeyValuePair<int, CleInfo> a, KeyValuePair<int, CleInfo> b)
            {
                int comparaison = ComparerPertinence(a.Value, b.Value);
                return comparaison != 0 ? comparaison : a.Key.CompareTo(b.Key);
            });
            List<CleInfo> cles = new List<CleInfo>();
            foreach (KeyValuePair<int, CleInfo> paire in indexees) cles.Add(paire.Value);

            CleInfo identification = null;
            foreach (CleInfo cle in cles)
            {
                if (cle.PeutIdentifierLigne && ToutesPresentes(cle.Colonnes, listeColonnes))
                {
                    identification = cle;
                    break;
                }
            }

            List<RelationInfo> versParents = new List<RelationInfo>();
            List<RelationInfo> depuisEnfants = new List<RelationInfo>();
            foreach (RelationInfo relation in pRelations)
            {
                // Une relation réflexive (table parente d'elle-même) figure dans les deux listes.
                if (MemeTable(relation.SchemaEnfant, relation.TableEnfant, table)) versParents.Add(relation);
                if (MemeTable(relation.SchemaParent, relation.TableParent, table)) depuisEnfants.Add(relation);
            }

            AnnoterColonnes(listeColonnes, identification, versParents);
            return new StructureTable(table, listeColonnes, cles, identification, versParents, depuisEnfants,
                new List<string>(pAvertissements));
        }

        // ------------------------------------------------------------------ Outils

        /// <summary>
        /// Découpe COLUMN_NAMES (« A, B DESC, "c" ») en noms de colonnes. Retire les sens
        /// de tri et les guillemets ; <paramref name="complete"/> est faux si la liste a été tronquée.
        /// </summary>
        public static List<string> DecouperColonnes(string listeColonnes, out bool complete)
        {
            complete = true;
            List<string> noms = new List<string>();
            if (string.IsNullOrWhiteSpace(listeColonnes)) return noms;

            string texte = listeColonnes.Trim();
            bool tronquee = texte.EndsWith("...", StringComparison.Ordinal);
            if (tronquee)
            {
                complete = false;
                texte = texte.Substring(0, texte.Length - 3);
            }

            string[] morceaux = texte.Split(',');
            for (int i = 0; i < morceaux.Length; i++)
            {
                // Le dernier nom d'une liste tronquée peut être coupé : on l'écarte.
                if (tronquee && i == morceaux.Length - 1) continue;

                string nom = morceaux[i].Trim();
                if (nom.EndsWith(" DESC", StringComparison.OrdinalIgnoreCase)) nom = nom.Substring(0, nom.Length - 5).TrimEnd();
                else if (nom.EndsWith(" ASC", StringComparison.OrdinalIgnoreCase)) nom = nom.Substring(0, nom.Length - 4).TrimEnd();
                if (nom.Length >= 2 && nom[0] == '"' && nom[nom.Length - 1] == '"') nom = nom.Substring(1, nom.Length - 2);

                if (nom.Length == 0)
                {
                    complete = false;
                    continue;
                }
                noms.Add(nom);
            }
            return noms;
        }

        private static OrigineCle OrigineDepuisType(string type)
        {
            switch (type)
            {
                case "PRIMARY KEY": return OrigineCle.ClePrimaire;
                case "UNIQUE": return OrigineCle.ContrainteUnique;
                case "PHYSICAL": return OrigineCle.FichierPhysique;
                case "INDEX": return OrigineCle.Index;
                case "LOGICAL": return OrigineCle.FichierLogique;
                default: return OrigineCle.Autre;
            }
        }

        private static int ComparerPertinence(CleInfo a, CleInfo b)
        {
            int identifiant = (b.PeutIdentifierLigne ? 1 : 0).CompareTo(a.PeutIdentifierLigne ? 1 : 0);
            if (identifiant != 0) return identifiant;
            int origine = ((int)a.Origine).CompareTo((int)b.Origine);
            if (origine != 0) return origine;
            return a.Colonnes.Count.CompareTo(b.Colonnes.Count);
        }

        private static CleInfo NormaliserColonnes(CleInfo cle, List<ColonneInfo> colonnes)
        {
            List<string> noms = new List<string>();
            foreach (string nom in cle.Colonnes)
            {
                ColonneInfo colonne = Trouver(nom, colonnes);
                noms.Add(colonne != null ? colonne.Nom : nom);
            }
            return new CleInfo(cle.Schema, cle.Nom, cle.Origine, cle.EstUnique, cle.EstPartielle, cle.EstComplete, noms);
        }

        private static ColonneInfo Trouver(string nom, List<ColonneInfo> colonnes)
        {
            foreach (ColonneInfo colonne in colonnes)
            {
                if (string.Equals(colonne.Nom, nom, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(colonne.NomSysteme, nom, StringComparison.OrdinalIgnoreCase))
                    return colonne;
            }
            return null;
        }

        private static bool ToutesPresentes(IReadOnlyList<string> noms, List<ColonneInfo> colonnes)
        {
            // Sans colonnes chargées, on ne peut pas vérifier : on fait confiance au catalogue.
            if (colonnes.Count == 0) return true;
            foreach (string nom in noms)
                if (Trouver(nom, colonnes) == null) return false;
            return true;
        }

        private static bool MemeTable(string schema, string nom, TableInfo table)
        {
            return string.Equals(schema, table.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(nom, table.Nom, StringComparison.OrdinalIgnoreCase);
        }

        private static void AnnoterColonnes(List<ColonneInfo> colonnes, CleInfo identification, List<RelationInfo> versParents)
        {
            foreach (ColonneInfo colonne in colonnes)
            {
                colonne.RangCle = 0;
                colonne.Reference = null;
            }

            if (identification != null)
            {
                for (int i = 0; i < identification.Colonnes.Count; i++)
                {
                    ColonneInfo colonne = Trouver(identification.Colonnes[i], colonnes);
                    if (colonne != null) colonne.RangCle = i + 1;
                }
            }

            foreach (RelationInfo relation in versParents)
            {
                int nombre = Math.Min(relation.ColonnesEnfant.Count, relation.ColonnesParent.Count);
                for (int i = 0; i < nombre; i++)
                {
                    ColonneInfo colonne = Trouver(relation.ColonnesEnfant[i], colonnes);
                    if (colonne == null) continue;
                    string cible = relation.TableParent + "." + relation.ColonnesParent[i];
                    // Une colonne peut appartenir à plusieurs clés étrangères.
                    colonne.Reference = string.IsNullOrEmpty(colonne.Reference) ? cible : colonne.Reference + " ; " + cible;
                }
            }
        }
    }
}

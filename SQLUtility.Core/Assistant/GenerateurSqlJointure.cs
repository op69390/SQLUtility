using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Génère la consultation multi-tables du niveau 2 (jointures INNER / LEFT OUTER). (EF-44)
    /// Chaque colonne est qualifiée par l'alias de sa table. Les erreurs bloquent la
    /// génération ; les avertissements (lignes multipliées, LEFT neutralisé) l'accompagnent.
    /// Le FROM et le WHERE sont partagés avec <see cref="GenerateurSqlAgregat"/>.
    /// </summary>
    public static class GenerateurSqlJointure
    {
        private const string Retrait = "       ";

        public static ResultatGeneration ConstruireSelect(RequeteJointure requete)
        {
            if (requete == null) throw new ArgumentNullException(nameof(requete));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();
            List<TableRequete> introduites = new List<TableRequete>();
            HashSet<TableRequete> externes = new HashSet<TableRequete>();

            string from = ConstruireFrom(requete, true, introduites, externes, erreurs, avertissements);

            // Liste de sélection : colonnes choisies, sinon toutes les colonnes de toutes les tables.
            List<ColonneRequete> colonnes = new List<ColonneRequete>(requete.Colonnes);
            if (colonnes.Count == 0)
            {
                foreach (TableRequete table in introduites)
                    foreach (Catalogue.ColonneInfo colonne in table.Structure.Colonnes) colonnes.Add(new ColonneRequete(table, colonne));
            }

            StringBuilder sql = new StringBuilder();
            sql.Append(requete.Distinct ? "SELECT DISTINCT " : "SELECT ");
            for (int i = 0; i < colonnes.Count; i++)
            {
                ColonneRequete colonne = colonnes[i];
                if (!introduites.Contains(colonne.Table))
                {
                    erreurs.Add("Colonne " + colonne.Colonne.Nom + " d'une table absente de la requête.");
                    continue;
                }
                if (i > 0) sql.Append(Retrait);
                sql.Append(Qualifier(colonne.Table, colonne.Colonne.Nom));
                if (i < colonnes.Count - 1) sql.Append(",");
                string libelle = Commentaire(colonne.Colonne.Libelle);
                if (libelle != null) sql.Append("  -- ").Append(libelle);
                sql.AppendLine();
            }
            if (colonnes.Count == 0) sql.AppendLine("*");
            sql.Append(from);

            AjouterWhere(sql, ConstruireConditionsWhere(requete, true, introduites, externes, erreurs, avertissements));

            List<string> tris = new List<string>();
            foreach (CritereTri tri in requete.Tris)
            {
                if (tri.Table == null || !introduites.Contains(tri.Table)) continue;
                tris.Add(Qualifier(tri.Table, tri.Colonne.Nom) + (tri.Descendant ? " DESC" : string.Empty));
            }
            if (tris.Count > 0) sql.AppendLine().Append("ORDER BY ").Append(string.Join(", ", tris));
            AjouterLimite(sql, requete.Limite);

            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        // ------------------------------------------------------------------ Parties partagées

        /// <summary>
        /// FROM et jointures. Remplit <paramref name="introduites"/> (tables dans l'ordre) et
        /// <paramref name="externes"/> (tables en LEFT OUTER). Sans alias, une table seule est
        /// écrite sans corrélation (SQL du niveau 1).
        /// </summary>
        internal static string ConstruireFrom(RequeteJointure requete, bool avecAlias, List<TableRequete> introduites,
            HashSet<TableRequete> externes, List<string> erreurs, List<string> avertissements)
        {
            introduites.Add(requete.Principale);
            HashSet<string> alias = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { requete.Principale.Alias };

            StringBuilder from = new StringBuilder();
            from.Append("FROM ").Append(avecAlias ? NomTable(requete.Principale) : GenerateurSqlSimple.NomTable(requete.Principale.Table));
            foreach (Jointure jointure in requete.Jointures)
            {
                if (!alias.Add(jointure.Table.Alias))
                    erreurs.Add("Alias « " + jointure.Table.Alias + " » utilisé deux fois.");

                List<string> conditions = new List<string>();
                foreach (ConditionJointure condition in jointure.Conditions)
                {
                    if (condition.ColonneJointe == null || condition.TableLiee == null || condition.ColonneLiee == null) continue;
                    if (!introduites.Contains(condition.TableLiee))
                    {
                        erreurs.Add(jointure.Table.Alias + " ne peut être liée qu'à une table placée avant elle (pas à " + condition.TableLiee.Alias + ").");
                        continue;
                    }
                    if (!DetecteurJointures.TypesCompatibles(condition.ColonneJointe, condition.ColonneLiee))
                        erreurs.Add(Qualifier(jointure.Table, condition.ColonneJointe.Nom) + " et " + Qualifier(condition.TableLiee, condition.ColonneLiee.Nom)
                            + " ne sont pas de types comparables.");
                    conditions.Add(Qualifier(jointure.Table, condition.ColonneJointe.Nom) + " = " + Qualifier(condition.TableLiee, condition.ColonneLiee.Nom));
                }
                if (conditions.Count == 0)
                    erreurs.Add("Jointure de " + jointure.Table.Alias + " (" + jointure.Table.Table.Nom + ") sans condition : produit cartésien refusé.");
                if (jointure.Origine == OrigineJointure.ClePartielleDeduite)
                    avertissements.Add("Jointure de " + jointure.Table.Alias + " sur une clé partielle : des lignes peuvent être multipliées.");

                from.AppendLine();
                from.Append(jointure.Type == TypeJointure.ExterneGauche ? "LEFT OUTER JOIN " : "INNER JOIN ");
                from.Append(NomTable(jointure.Table)).Append(" ON ").Append(string.Join(" AND ", conditions));

                if (jointure.Type == TypeJointure.ExterneGauche) externes.Add(jointure.Table);
                introduites.Add(jointure.Table);
            }
            return from.ToString();
        }

        /// <summary>
        /// Conditions du WHERE. Un filtre sur une table en LEFT OUTER (hors IS NULL) élimine
        /// les lignes sans correspondance : la jointure redevient interne (avertissement).
        /// </summary>
        internal static List<string> ConstruireConditionsWhere(RequeteJointure requete, bool qualifier, List<TableRequete> introduites,
            HashSet<TableRequete> externes, List<string> erreurs, List<string> avertissements)
        {
            List<string> conditions = new List<string>();
            foreach (CritereFiltre critere in requete.Criteres)
            {
                if (critere == null || critere.Colonne == null) continue;
                TableRequete table = critere.Table ?? requete.Principale;
                if (!introduites.Contains(table))
                {
                    erreurs.Add("Filtre sur " + critere.Colonne.Nom + " : table absente de la requête.");
                    continue;
                }
                string condition = GenerateurSqlSimple.ConstruireCondition(critere, Expression(table, critere.Colonne.Nom, qualifier), erreurs);
                if (condition == null) continue;
                if (externes.Contains(table) && critere.Operateur != OperateurFiltre.EstNul)
                    avertissements.Add("Le filtre sur " + Qualifier(table, critere.Colonne.Nom)
                        + " écarte les lignes sans correspondance : la jointure externe de " + table.Alias + " se comporte comme une jointure interne.");
                conditions.Add(condition);
            }
            foreach (CritereExistence existence in requete.Existences)
            {
                string condition = ConstruireExistence(existence, introduites, erreurs);
                if (condition != null) conditions.Add(condition);
            }
            return conditions;
        }

        /// <summary>
        /// [NOT] EXISTS (SELECT 1 FROM T S1 WHERE S1.x = A.y AND filtres de S1). (EF-48)
        /// Sans lien vers la requête, la sous-requête porterait sur toute la table : refusé.
        /// </summary>
        internal static string ConstruireExistence(CritereExistence existence, List<TableRequete> introduites, List<string> erreurs)
        {
            TableRequete table = existence.Table;
            foreach (TableRequete presente in introduites)
            {
                if (string.Equals(presente.Alias, table.Alias, StringComparison.OrdinalIgnoreCase))
                {
                    erreurs.Add("Alias « " + table.Alias + " » utilisé deux fois.");
                    return null;
                }
            }

            List<string> conditions = new List<string>();
            foreach (ConditionJointure condition in existence.Conditions)
            {
                if (condition.ColonneJointe == null || condition.TableLiee == null || condition.ColonneLiee == null) continue;
                if (!introduites.Contains(condition.TableLiee))
                {
                    erreurs.Add("Condition d'existence " + table.Alias + " : table " + condition.TableLiee.Alias + " absente de la requête.");
                    continue;
                }
                if (!DetecteurJointures.TypesCompatibles(condition.ColonneJointe, condition.ColonneLiee))
                    erreurs.Add(Qualifier(table, condition.ColonneJointe.Nom) + " et " + Qualifier(condition.TableLiee, condition.ColonneLiee.Nom)
                        + " ne sont pas de types comparables.");
                conditions.Add(Qualifier(table, condition.ColonneJointe.Nom) + " = " + Qualifier(condition.TableLiee, condition.ColonneLiee.Nom));
            }
            if (conditions.Count == 0)
            {
                erreurs.Add("Condition d'existence sur " + table.Alias + " (" + table.Table.Nom + ") sans lien avec la requête : refusée.");
                return null;
            }
            foreach (CritereFiltre critere in existence.Criteres)
            {
                if (critere == null || critere.Colonne == null) continue;
                string condition = GenerateurSqlSimple.ConstruireCondition(critere, Qualifier(table, critere.Colonne.Nom), erreurs);
                if (condition != null) conditions.Add(condition);
            }
            return (existence.Negation ? "NOT EXISTS" : "EXISTS") + " (SELECT 1 FROM " + NomTable(table)
                + " WHERE " + string.Join(" AND ", conditions) + ")";
        }

        internal static void AjouterWhere(StringBuilder sql, List<string> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
                sql.AppendLine().Append(i == 0 ? "WHERE  " : "  AND  ").Append(conditions[i]);
        }

        internal static void AjouterLimite(StringBuilder sql, int limite)
        {
            if (limite > 0)
                sql.AppendLine().Append("FETCH FIRST ").Append(limite.ToString(CultureInfo.InvariantCulture)).Append(" ROWS ONLY");
        }

        /// <summary>Colonne qualifiée par l'alias (jointure) ou simple (table seule).</summary>
        internal static string Expression(TableRequete table, string colonne, bool qualifier)
        {
            return qualifier ? Qualifier(table, colonne) : GenerateurSqlSimple.NomSql(colonne);
        }

        internal static string Qualifier(TableRequete table, string colonne)
        {
            return table.Alias + "." + GenerateurSqlSimple.NomSql(colonne);
        }

        internal static string Commentaire(string texte)
        {
            if (string.IsNullOrWhiteSpace(texte)) return null;
            return texte.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string NomTable(TableRequete table)
        {
            return GenerateurSqlSimple.NomTable(table.Table) + " " + table.Alias;
        }
    }
}

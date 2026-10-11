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
    /// </summary>
    public static class GenerateurSqlJointure
    {
        private const string Retrait = "       ";

        public static ResultatGeneration ConstruireSelect(RequeteJointure requete)
        {
            if (requete == null) throw new ArgumentNullException(nameof(requete));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();

            // Tables connues au fil des jointures : une table ne peut se lier qu'aux précédentes.
            List<TableRequete> introduites = new List<TableRequete> { requete.Principale };
            HashSet<string> alias = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { requete.Principale.Alias };
            HashSet<TableRequete> externes = new HashSet<TableRequete>();

            StringBuilder from = new StringBuilder();
            from.Append("FROM ").Append(NomTable(requete.Principale));
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

            // WHERE : critères qualifiés. Un filtre sur une table en LEFT OUTER (hors IS NULL)
            // élimine les lignes sans correspondance : la jointure redevient interne.
            bool premier = true;
            foreach (CritereFiltre critere in requete.Criteres)
            {
                if (critere == null || critere.Colonne == null) continue;
                if (critere.Table == null || !introduites.Contains(critere.Table))
                {
                    erreurs.Add("Filtre sur " + critere.Colonne.Nom + " : table absente de la requête.");
                    continue;
                }
                string condition = GenerateurSqlSimple.ConstruireCondition(critere, Qualifier(critere.Table, critere.Colonne.Nom), erreurs);
                if (condition == null) continue;
                if (externes.Contains(critere.Table) && critere.Operateur != OperateurFiltre.EstNul)
                    avertissements.Add("Le filtre sur " + Qualifier(critere.Table, critere.Colonne.Nom)
                        + " écarte les lignes sans correspondance : la jointure externe de " + critere.Table.Alias + " se comporte comme une jointure interne.");
                sql.AppendLine().Append(premier ? "WHERE  " : "  AND  ").Append(condition);
                premier = false;
            }

            List<string> tris = new List<string>();
            foreach (CritereTri tri in requete.Tris)
            {
                if (tri.Table == null || !introduites.Contains(tri.Table)) continue;
                tris.Add(Qualifier(tri.Table, tri.Colonne.Nom) + (tri.Descendant ? " DESC" : string.Empty));
            }
            if (tris.Count > 0) sql.AppendLine().Append("ORDER BY ").Append(string.Join(", ", tris));
            if (requete.Limite > 0)
                sql.AppendLine().Append("FETCH FIRST ").Append(requete.Limite.ToString(CultureInfo.InvariantCulture)).Append(" ROWS ONLY");

            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        private static string NomTable(TableRequete table)
        {
            return GenerateurSqlSimple.NomTable(table.Table) + " " + table.Alias;
        }

        private static string Qualifier(TableRequete table, string colonne)
        {
            return table.Alias + "." + GenerateurSqlSimple.NomSql(colonne);
        }

        private static string Commentaire(string texte)
        {
            if (string.IsNullOrWhiteSpace(texte)) return null;
            return texte.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}

using System;
using System.Collections.Generic;
using SQLUtility.Core.Edition;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Analyse une instruction SQL (logique pure) : type, présence d'une clause WHERE,
    /// et évaluation graduée du risque pour la confirmation à l'exécution. (EF-22 à EF-32)
    /// </summary>
    public static class AnalyseurInstruction
    {
        /// <summary>Détecte le type d'instruction d'après le premier mot significatif.</summary>
        public static TypeInstructionSql DetecterType(string sql)
        {
            foreach (JetonSql jeton in TokeniseurSql.Tokeniser(sql))
            {
                if (jeton.Type == TypeJetonSql.Espace || jeton.Type == TypeJetonSql.Commentaire)
                    continue;

                switch ((jeton.Texte ?? string.Empty).ToUpperInvariant())
                {
                    case "SELECT":
                    case "WITH":
                    case "VALUES":
                        return TypeInstructionSql.Select;
                    case "INSERT":
                        return TypeInstructionSql.Insert;
                    case "UPDATE":
                        return TypeInstructionSql.Update;
                    case "DELETE":
                        return TypeInstructionSql.Delete;
                    case "CREATE":
                    case "ALTER":
                    case "DROP":
                    case "GRANT":
                    case "REVOKE":
                    case "LABEL":
                    case "COMMENT":
                    case "RENAME":
                        return TypeInstructionSql.Ddl;
                    case "MERGE":
                    case "CALL":
                        return TypeInstructionSql.Autre;
                    default:
                        return TypeInstructionSql.Inconnu;
                }
            }
            return TypeInstructionSql.Inconnu;
        }

        /// <summary>
        /// Indique si l'instruction comporte une clause WHERE principale. Un WHERE placé dans une
        /// sous-requête (entre parenthèses) ne limite pas les lignes modifiées : il ne compte pas.
        /// </summary>
        public static bool ContientWhere(string sql)
        {
            return TrouverMotCle(TokeniseurSql.Tokeniser(sql), "WHERE") != null;
        }

        /// <summary>
        /// Évalue le risque d'une instruction. <paramref name="tableJournalisee"/> provient de
        /// la détection EF-30 (null si indéterminé) ; false renforce le risque d'un DML.
        /// </summary>
        public static EvaluationRisque Evaluer(string sql, bool? tableJournalisee)
        {
            TypeInstructionSql type = DetecterType(sql);
            List<string> raisons = new List<string>();
            NiveauRisque niveau;
            bool sansWhere = false;

            switch (type)
            {
                case TypeInstructionSql.Select:
                    niveau = NiveauRisque.Faible;
                    break;

                case TypeInstructionSql.Insert:
                    niveau = NiveauRisque.Modere;
                    raisons.Add("Ajout de données (INSERT).");
                    break;

                case TypeInstructionSql.Update:
                case TypeInstructionSql.Delete:
                    sansWhere = !ContientWhere(sql);
                    string verbe = (type == TypeInstructionSql.Update) ? "UPDATE" : "DELETE";
                    if (sansWhere)
                    {
                        niveau = NiveauRisque.Critique;
                        raisons.Add(verbe + " sans clause WHERE : affecte TOUTES les lignes de la table.");
                    }
                    else
                    {
                        niveau = NiveauRisque.Eleve;
                        raisons.Add(verbe + " de données (clause WHERE présente).");
                    }
                    break;

                case TypeInstructionSql.Ddl:
                    niveau = NiveauRisque.Eleve;
                    raisons.Add("Instruction DDL : modifie la structure (souvent irréversible).");
                    break;

                default:
                    niveau = NiveauRisque.Modere;
                    raisons.Add("Type d'instruction non reconnu : à exécuter avec prudence.");
                    break;
            }

            // EF-30 : un DML sur une table non journalisée est irréversible (aucun ROLLBACK).
            bool estDml = type == TypeInstructionSql.Insert
                       || type == TypeInstructionSql.Update
                       || type == TypeInstructionSql.Delete
                       || type == TypeInstructionSql.Autre;
            if (estDml && tableJournalisee.HasValue && !tableJournalisee.Value)
            {
                raisons.Add("Table non journalisée : opération IRRÉVERSIBLE (aucun ROLLBACK possible).");
                if (niveau < NiveauRisque.Eleve) niveau = NiveauRisque.Eleve;
                if (sansWhere) niveau = NiveauRisque.Critique;
            }

            bool renforcee = niveau == NiveauRisque.Critique
                          || type == TypeInstructionSql.Ddl
                          || (estDml && tableJournalisee.HasValue && !tableJournalisee.Value);

            return new EvaluationRisque(type, niveau, raisons, renforcee);
        }

        /// <summary>
        /// Construit le « SELECT COUNT(*) » qui estime les lignes touchées : même table et même
        /// WHERE principal pour un UPDATE / DELETE (les sous-requêtes, entre parenthèses, sont
        /// ignorées dans la recherche des mots-clés) ; comptage du SELECT pour un INSERT … SELECT.
        /// Retourne null si non applicable (ex. INSERT … VALUES).
        /// </summary>
        public static string ConstruireComptage(string sql)
        {
            TypeInstructionSql type = DetecterType(sql);
            IReadOnlyList<JetonSql> jetons = TokeniseurSql.Tokeniser(sql);

            if (type == TypeInstructionSql.Insert)
            {
                JetonSql jetonSelect = TrouverMotCle(jetons, "SELECT");
                if (jetonSelect == null) return null;
                string selection = sql.Substring(jetonSelect.Position).Trim().TrimEnd(';', ' ', '\r', '\n', '\t');
                return "SELECT COUNT(*) FROM (" + selection + ") AS LIGNES_A_COPIER";
            }
            if (type != TypeInstructionSql.Update && type != TypeInstructionSql.Delete)
                return null;

            JetonSql jetonWhere = TrouverMotCle(jetons, "WHERE");

            string table;
            if (type == TypeInstructionSql.Delete)
            {
                JetonSql jetonFrom = TrouverMotCle(jetons, "FROM");
                if (jetonFrom == null) return null;
                int debut = jetonFrom.Position + jetonFrom.Longueur;
                int fin = (jetonWhere != null) ? jetonWhere.Position : sql.Length;
                table = sql.Substring(debut, fin - debut).Trim();
            }
            else
            {
                JetonSql jetonUpdate = TrouverMotCle(jetons, "UPDATE");
                JetonSql jetonSet = TrouverMotCle(jetons, "SET");
                if (jetonUpdate == null || jetonSet == null) return null;
                int debut = jetonUpdate.Position + jetonUpdate.Longueur;
                int fin = jetonSet.Position;
                if (fin <= debut) return null;
                table = sql.Substring(debut, fin - debut).Trim();
            }

            if (string.IsNullOrEmpty(table)) return null;

            string comptage = "SELECT COUNT(*) FROM " + table;
            if (jetonWhere != null)
            {
                string clause = sql.Substring(jetonWhere.Position).Trim().TrimEnd(';', ' ', '\r', '\n', '\t');
                comptage += " " + clause;
            }
            return comptage;
        }

        /// <summary>Premier mot-clé <paramref name="mot"/> hors parenthèses (niveau principal de l'instruction).</summary>
        private static JetonSql TrouverMotCle(IReadOnlyList<JetonSql> jetons, string mot)
        {
            int profondeur = 0;
            foreach (JetonSql jeton in jetons)
            {
                if (jeton.Type == TypeJetonSql.Ponctuation)
                {
                    if (jeton.Texte == "(") profondeur++;
                    else if (jeton.Texte == ")" && profondeur > 0) profondeur--;
                    continue;
                }
                if (profondeur == 0 && jeton.Type == TypeJetonSql.MotCle
                    && string.Equals(jeton.Texte, mot, StringComparison.OrdinalIgnoreCase))
                    return jeton;
            }
            return null;
        }
    }
}

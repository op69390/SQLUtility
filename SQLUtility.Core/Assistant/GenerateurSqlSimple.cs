using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Génère les instructions du niveau 1 de l'assistant (une table) : consultation,
    /// ajout, modification et suppression d'une ligne. (EF-40 à EF-43)
    /// Le SQL produit est présenté dans l'éditeur, jamais exécuté directement : il passe
    /// par l'analyse de risque et les confirmations de l'exécution (EF-22 à EF-32).
    /// </summary>
    public static class GenerateurSqlSimple
    {
        /// <summary>Corrélation utilisée quand RRN() est nécessaire.</summary>
        public const string Alias = "T";

        private const string Retrait = "       ";

        // ------------------------------------------------------------------ Consultation

        public static ResultatGeneration ConstruireSelect(RequeteConsultation requete)
        {
            if (requete == null) throw new ArgumentNullException(nameof(requete));
            List<string> erreurs = new List<string>();
            if (requete.Table == null)
                return new ResultatGeneration(null, new[] { "Choisissez une table." });

            List<ColonneInfo> colonnes = new List<ColonneInfo>(requete.Colonnes);
            if (colonnes.Count == 0 && requete.ToutesColonnes != null) colonnes.AddRange(requete.ToutesColonnes);
            bool alias = requete.AvecRrn;

            // Liste de sélection, une colonne par ligne avec son libellé en commentaire.
            List<string> elements = new List<string>();
            List<string> libelles = new List<string>();
            if (requete.AvecRrn)
            {
                elements.Add("RRN(" + Alias + ") AS RRN");
                libelles.Add("N° relatif d'enregistrement");
            }
            foreach (ColonneInfo colonne in colonnes)
            {
                elements.Add(NomColonne(colonne));
                libelles.Add(colonne.Libelle);
            }
            if (elements.Count == 0) elements.Add("*");

            StringBuilder sql = new StringBuilder();
            sql.Append(requete.Distinct ? "SELECT DISTINCT " : "SELECT ");
            for (int i = 0; i < elements.Count; i++)
            {
                if (i > 0) sql.Append(Retrait);
                sql.Append(elements[i]);
                if (i < elements.Count - 1) sql.Append(",");
                string libelle = i < libelles.Count ? NettoyerCommentaire(libelles[i]) : null;
                if (!string.IsNullOrEmpty(libelle)) sql.Append("  -- ").Append(libelle);
                sql.AppendLine();
            }
            sql.Append("FROM ").Append(NomTable(requete.Table));
            if (alias) sql.Append(" ").Append(Alias);

            List<string> conditions = new List<string>();
            foreach (CritereFiltre critere in requete.Criteres)
            {
                string condition = ConstruireCondition(critere, erreurs);
                if (condition != null) conditions.Add(condition);
            }
            AjouterWhere(sql, conditions);

            if (requete.Tris.Count > 0)
            {
                List<string> tris = new List<string>();
                foreach (CritereTri tri in requete.Tris)
                    tris.Add(NomColonne(tri.Colonne) + (tri.Descendant ? " DESC" : string.Empty));
                sql.AppendLine().Append("ORDER BY ").Append(string.Join(", ", tris));
            }
            if (requete.Limite > 0)
                sql.AppendLine().Append("FETCH FIRST ").Append(requete.Limite.ToString(CultureInfo.InvariantCulture)).Append(" ROWS ONLY");

            return new ResultatGeneration(sql.ToString(), erreurs);
        }

        /// <summary>Relecture d'une ligne par sa clé ou son RRN, pour préremplir la modification.</summary>
        public static ResultatGeneration ConstruireLectureLigne(TableInfo table, IList<ColonneInfo> colonnes, IdentificationLigne identification)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (identification == null) throw new ArgumentNullException(nameof(identification));
            List<string> erreurs = new List<string>();
            bool alias = identification.Rrn.HasValue;

            List<string> noms = new List<string>();
            foreach (ColonneInfo colonne in colonnes ?? new List<ColonneInfo>()) noms.Add(NomColonne(colonne));
            if (noms.Count == 0) noms.Add("*");

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ").Append(string.Join(", ", noms));
            sql.AppendLine().Append("FROM ").Append(NomTable(table));
            if (alias) sql.Append(" ").Append(Alias);
            AjouterWhere(sql, ConditionsIdentification(identification, erreurs));
            // Deux lignes au plus : une seconde révèle une clé qui ne désigne pas une ligne unique.
            sql.AppendLine().Append("FETCH FIRST 2 ROWS ONLY");
            return new ResultatGeneration(sql.ToString(), erreurs);
        }

        // ------------------------------------------------------------------ Ajout / modification / suppression

        public static ResultatGeneration ConstruireInsert(TableInfo table, IList<AffectationColonne> valeurs)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            List<string> erreurs = new List<string>();
            List<string> noms = new List<string>();
            List<string> litteraux = new List<string>();

            foreach (AffectationColonne affectation in valeurs ?? new List<AffectationColonne>())
            {
                string litteral = Litteral(affectation, erreurs);
                if (litteral == null) continue;
                noms.Add(NomSql(affectation.Colonne.Nom));
                litteraux.Add(litteral);
            }
            if (noms.Count == 0 && erreurs.Count == 0) erreurs.Add("Saisissez au moins une valeur.");

            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO ").Append(NomTable(table)).AppendLine();
            sql.Append("       (").Append(string.Join(", ", noms)).AppendLine(")");
            sql.Append("VALUES (").Append(string.Join(", ", litteraux)).Append(")");
            return new ResultatGeneration(sql.ToString(), erreurs);
        }

        public static ResultatGeneration ConstruireUpdate(TableInfo table, IList<AffectationColonne> modifications, IdentificationLigne identification)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (identification == null) throw new ArgumentNullException(nameof(identification));
            List<string> erreurs = new List<string>();
            bool alias = identification.Rrn.HasValue;

            List<string> affectations = new List<string>();
            foreach (AffectationColonne affectation in modifications ?? new List<AffectationColonne>())
            {
                string litteral = Litteral(affectation, erreurs);
                if (litteral != null) affectations.Add(NomColonne(affectation.Colonne) + " = " + litteral);
            }
            if (affectations.Count == 0 && erreurs.Count == 0) erreurs.Add("Aucune modification saisie.");

            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE ").Append(NomTable(table));
            if (alias) sql.Append(" ").Append(Alias);
            sql.AppendLine().Append("SET    ").Append(string.Join("," + Environment.NewLine + Retrait, affectations));
            AjouterWhere(sql, ConditionsIdentification(identification, erreurs));
            return new ResultatGeneration(sql.ToString(), erreurs);
        }

        public static ResultatGeneration ConstruireDelete(TableInfo table, IdentificationLigne identification)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (identification == null) throw new ArgumentNullException(nameof(identification));
            List<string> erreurs = new List<string>();

            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM ").Append(NomTable(table));
            if (identification.Rrn.HasValue) sql.Append(" ").Append(Alias);
            AjouterWhere(sql, ConditionsIdentification(identification, erreurs));
            return new ResultatGeneration(sql.ToString(), erreurs);
        }

        // ------------------------------------------------------------------ Identifiants

        /// <summary>
        /// Nom SQL : tel quel s'il est ordinaire (majuscules, chiffres, _ @ # $), délimité
        /// par des guillemets sinon.
        /// </summary>
        public static string NomSql(string nom)
        {
            if (string.IsNullOrEmpty(nom)) return "\"\"";
            bool ordinaire = char.IsLetter(nom[0]) || nom[0] == '@' || nom[0] == '#' || nom[0] == '$';
            foreach (char c in nom)
            {
                bool permis = (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '@' || c == '#' || c == '$';
                if (!permis) { ordinaire = false; break; }
            }
            return ordinaire ? nom : "\"" + nom.Replace("\"", "\"\"") + "\"";
        }

        public static string NomTable(TableInfo table)
        {
            return string.IsNullOrEmpty(table.Schema) ? NomSql(table.Nom) : NomSql(table.Schema) + "." + NomSql(table.Nom);
        }

        // ------------------------------------------------------------------ Outils

        /// <summary>
        /// Colonne non qualifiée : une seule table est en jeu, et DB2 for i refuse une colonne
        /// qualifiée dans SET. L'alias ne sert qu'à RRN().
        /// </summary>
        private static string NomColonne(ColonneInfo colonne)
        {
            return NomSql(colonne.Nom);
        }

        private static void AjouterWhere(StringBuilder sql, List<string> conditions)
        {
            for (int i = 0; i < conditions.Count; i++)
                sql.AppendLine().Append(i == 0 ? "WHERE  " : "  AND  ").Append(conditions[i]);
        }

        private static List<string> ConditionsIdentification(IdentificationLigne identification, List<string> erreurs)
        {
            List<string> conditions = new List<string>();
            if (identification.Rrn.HasValue)
            {
                if (identification.Rrn.Value < 1) erreurs.Add("Le numéro d'enregistrement RRN doit être positif.");
                conditions.Add("RRN(" + Alias + ") = " + identification.Rrn.Value.ToString(CultureInfo.InvariantCulture));
                return conditions;
            }

            if (identification.ValeursCle.Count == 0)
            {
                // Garde-fou : jamais d'UPDATE / DELETE sans désignation de ligne.
                erreurs.Add("Désignez la ligne : valeurs de la clé ou numéro d'enregistrement RRN.");
                return conditions;
            }
            foreach (AffectationColonne valeur in identification.ValeursCle)
            {
                if (valeur.EstNull)
                {
                    conditions.Add(NomSql(valeur.Colonne.Nom) + " IS NULL");
                    continue;
                }
                string litteral = Litteral(valeur, erreurs);
                if (litteral != null) conditions.Add(NomSql(valeur.Colonne.Nom) + " = " + litteral);
            }
            return conditions;
        }

        private static string Litteral(AffectationColonne affectation, List<string> erreurs)
        {
            if (affectation.EstNull)
            {
                if (!affectation.Colonne.AccepteNull)
                {
                    erreurs.Add(affectation.Colonne.Nom + " n'accepte pas la valeur NULL.");
                    return null;
                }
                return "NULL";
            }
            string litteral;
            string erreur;
            if (FormateurLitteral.TryFormater(affectation.Colonne, affectation.Valeur, affectation.FormatDate, out litteral, out erreur))
                return litteral;
            erreurs.Add(erreur);
            return null;
        }

        private static string ConstruireCondition(CritereFiltre critere, List<string> erreurs)
        {
            if (critere == null || critere.Colonne == null) return null;
            string colonne = NomColonne(critere.Colonne);

            switch (critere.Operateur)
            {
                case OperateurFiltre.EstNul: return colonne + " IS NULL";
                case OperateurFiltre.NestPasNul: return colonne + " IS NOT NULL";
                case OperateurFiltre.Commence:
                case OperateurFiltre.Contient:
                case OperateurFiltre.Finit:
                    return ConstruireLike(critere, colonne, erreurs);
                case OperateurFiltre.Dans:
                    return ConstruireDans(critere, colonne, erreurs);
            }

            string valeur = LitteralFiltre(critere, critere.Valeur, erreurs);
            if (valeur == null) return null;
            switch (critere.Operateur)
            {
                case OperateurFiltre.Egal: return colonne + " = " + valeur;
                case OperateurFiltre.Different: return colonne + " <> " + valeur;
                case OperateurFiltre.Inferieur: return colonne + " < " + valeur;
                case OperateurFiltre.InferieurOuEgal: return colonne + " <= " + valeur;
                case OperateurFiltre.Superieur: return colonne + " > " + valeur;
                case OperateurFiltre.SuperieurOuEgal: return colonne + " >= " + valeur;
                case OperateurFiltre.Entre:
                    string borne = LitteralFiltre(critere, critere.Valeur2, erreurs);
                    return borne == null ? null : colonne + " BETWEEN " + valeur + " AND " + borne;
                default:
                    return null;
            }
        }

        private static string LitteralFiltre(CritereFiltre critere, string saisie, List<string> erreurs)
        {
            string litteral;
            string erreur;
            if (FormateurLitteral.TryFormater(critere.Colonne, saisie, critere.FormatDate, out litteral, out erreur))
                return litteral;
            erreurs.Add(erreur);
            return null;
        }

        private static string ConstruireLike(CritereFiltre critere, string colonne, List<string> erreurs)
        {
            if (TypesSql.Categorie(critere.Colonne.TypeDonnee) != CategorieType.Texte)
            {
                erreurs.Add(critere.Colonne.Nom + " : « commence / contient / finit par » ne s'applique qu'au texte.");
                return null;
            }
            string valeur = critere.Valeur ?? string.Empty;
            // % et _ saisis sont des caractères ordinaires : on les protège par ESCAPE.
            bool echappe = valeur.IndexOf('%') >= 0 || valeur.IndexOf('_') >= 0 || valeur.IndexOf('!') >= 0;
            if (echappe) valeur = valeur.Replace("!", "!!").Replace("%", "!%").Replace("_", "!_");

            string motif;
            string cible = colonne;
            switch (critere.Operateur)
            {
                case OperateurFiltre.Commence: motif = valeur + "%"; break;
                case OperateurFiltre.Contient: motif = "%" + valeur + "%"; break;
                default:
                    motif = "%" + valeur;
                    cible = "RTRIM(" + colonne + ")"; // CHAR fixe : les blancs de fin empêcheraient la correspondance.
                    break;
            }
            return cible + " LIKE " + FormateurLitteral.Chaine(motif) + (echappe ? " ESCAPE '!'" : string.Empty);
        }

        private static string ConstruireDans(CritereFiltre critere, string colonne, List<string> erreurs)
        {
            List<string> litteraux = new List<string>();
            foreach (string morceau in (critere.Valeur ?? string.Empty).Split(';'))
            {
                string valeur = morceau.Trim();
                if (valeur.Length == 0) continue;
                string litteral = LitteralFiltre(critere, valeur, erreurs);
                if (litteral != null) litteraux.Add(litteral);
            }
            if (litteraux.Count == 0)
            {
                erreurs.Add(critere.Colonne.Nom + " : saisissez des valeurs séparées par « ; ».");
                return null;
            }
            return colonne + " IN (" + string.Join(", ", litteraux) + ")";
        }

        private static string NettoyerCommentaire(string texte)
        {
            if (string.IsNullOrWhiteSpace(texte)) return null;
            return texte.Replace("\r", " ").Replace("\n", " ").Trim();
        }
    }
}

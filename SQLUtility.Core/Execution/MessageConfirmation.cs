using System;
using System.Globalization;
using System.Text;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Textes de la confirmation graduée avant un DML / DDL (EF-22 à EF-32) :
    /// première confirmation détaillée, seconde confirmation si elle est renforcée.
    /// </summary>
    public static class MessageConfirmation
    {
        /// <summary>Nombre maximal de caractères de l'instruction recopiés dans le message.</summary>
        public const int LongueurMaxApercu = 600;

        /// <param name="cible">Table modifiée, ou null si non identifiée.</param>
        /// <param name="lignesEstimees">Résultat du COUNT(*) préalable, ou null s'il n'a pas pu être fait.</param>
        /// <param name="sousControleValidation">Vrai si l'opération pourra être annulée (ROLLBACK).</param>
        public static string Construire(string sql, EvaluationRisque evaluation, TableCible cible,
            long? lignesEstimees, bool sousControleValidation)
        {
            if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));

            StringBuilder texte = new StringBuilder();
            texte.AppendLine("Niveau de risque : " + LibelleNiveau(evaluation.Niveau).ToUpperInvariant());
            texte.AppendLine("Table : " + (cible != null ? cible.ToString() : "non identifiée (traitée comme non journalisée par prudence)"));
            if (lignesEstimees.HasValue)
                texte.AppendLine("Lignes concernées (comptage préalable) : " + lignesEstimees.Value.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")));
            texte.AppendLine();

            foreach (string raison in evaluation.Raisons)
                texte.AppendLine("• " + raison);
            texte.AppendLine();

            texte.AppendLine(sousControleValidation
                ? "L'instruction sera exécutée DANS UNE TRANSACTION : vous pourrez ensuite la valider (COMMIT) ou l'annuler (ROLLBACK)."
                : "L'instruction sera validée IMMÉDIATEMENT : aucun retour arrière possible.");
            texte.AppendLine();
            texte.AppendLine(Apercu(sql));
            texte.AppendLine();
            texte.Append("Exécuter cette instruction ?");
            return texte.ToString();
        }

        /// <summary>Seconde confirmation, posée quand <see cref="EvaluationRisque.ConfirmationRenforcee"/> est vrai.</summary>
        public static string ConstruireRenforcee(EvaluationRisque evaluation, long? lignesEstimees, bool sousControleValidation)
        {
            if (evaluation == null) throw new ArgumentNullException(nameof(evaluation));

            StringBuilder texte = new StringBuilder();
            texte.AppendLine("DERNIÈRE CONFIRMATION");
            texte.AppendLine();
            if (lignesEstimees.HasValue)
                texte.AppendLine(lignesEstimees.Value.ToString("N0", CultureInfo.GetCultureInfo("fr-FR")) + " ligne(s) seront touchées.");
            if (!sousControleValidation)
                texte.AppendLine("Cette opération est IRRÉVERSIBLE.");
            texte.AppendLine();
            texte.Append("Confirmez-vous vraiment l'exécution ?");
            return texte.ToString();
        }

        public static string LibelleNiveau(NiveauRisque niveau)
        {
            switch (niveau)
            {
                case NiveauRisque.Faible: return "Faible";
                case NiveauRisque.Modere: return "Modéré";
                case NiveauRisque.Eleve: return "Élevé";
                default: return "Critique";
            }
        }

        private static string Apercu(string sql)
        {
            string texte = (sql ?? string.Empty).Trim();
            return texte.Length <= LongueurMaxApercu ? texte : texte.Substring(0, LongueurMaxApercu) + " […]";
        }
    }
}

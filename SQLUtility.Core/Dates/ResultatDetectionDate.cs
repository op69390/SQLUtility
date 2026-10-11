namespace SQLUtility.Core.Dates
{
    /// <summary>Résultat de la détection d'une colonne stockant une date encodée. (EF-17/18)</summary>
    public sealed class ResultatDetectionDate
    {
        public ResultatDetectionDate(bool estProbablementDate, FormatDate formatParDefaut)
        {
            EstProbablementDate = estProbablementDate;
            FormatParDefaut = formatParDefaut;
        }

        /// <summary>Vrai si la colonne ressemble à une date encodée (numérique, longueur compatible).</summary>
        public bool EstProbablementDate { get; }

        /// <summary>Format proposé par défaut (déduit de la longueur) ; à confirmer par l'utilisateur.</summary>
        public FormatDate FormatParDefaut { get; }
    }
}

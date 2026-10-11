namespace SQLUtility.Core.Dates
{
    /// <summary>Métadonnées descriptives d'un <see cref="FormatDate"/>.</summary>
    public sealed class DescriptionFormatDate
    {
        public DescriptionFormatDate(FormatDate format, string libelle, int nombreCaracteres,
                                     bool numerique, bool implemente, string exemple)
        {
            Format = format;
            Libelle = libelle;
            NombreCaracteres = nombreCaracteres;
            Numerique = numerique;
            Implemente = implemente;
            Exemple = exemple;
        }

        public FormatDate Format { get; }
        public string Libelle { get; }

        /// <summary>Nombre de caractères de la représentation.</summary>
        public int NombreCaracteres { get; }

        /// <summary>Vrai si la valeur est purement numérique (encodage entier).</summary>
        public bool Numerique { get; }

        /// <summary>Indique si la conversion est déjà disponible (sinon : backlog 2).</summary>
        public bool Implemente { get; }

        /// <summary>Exemple de valeur encodée.</summary>
        public string Exemple { get; }

        public override string ToString()
        {
            return Libelle;
        }
    }
}

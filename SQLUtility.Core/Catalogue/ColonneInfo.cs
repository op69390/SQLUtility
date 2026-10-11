namespace SQLUtility.Core.Catalogue
{
    /// <summary>Métadonnées d'une colonne : libellé, type, longueur, échelle. (EF-08)</summary>
    public sealed class ColonneInfo
    {
        /// <summary>Nom SQL de la colonne.</summary>
        public string Nom { get; set; }

        /// <summary>Nom système court (10 caractères) sur IBM i, si différent du nom SQL.</summary>
        public string NomSysteme { get; set; }

        /// <summary>Libellé descriptif (COLUMN_TEXT).</summary>
        public string Libelle { get; set; }

        /// <summary>Type de données SQL (ex. DECIMAL, CHAR, DATE).</summary>
        public string TypeDonnee { get; set; }

        /// <summary>Longueur (précision pour les numériques).</summary>
        public int Longueur { get; set; }

        /// <summary>Échelle (nombre de décimales pour les numériques).</summary>
        public int Echelle { get; set; }

        /// <summary>Vrai si la colonne accepte la valeur NULL.</summary>
        public bool AccepteNull { get; set; }

        /// <summary>Position ordinale dans la table (1..n).</summary>
        public int Position { get; set; }

        /// <summary>Rang dans la clé d'identification de la table (1..n), 0 si hors clé. (EF-09)</summary>
        public int RangCle { get; set; }

        /// <summary>Rang de clé pour l'affichage (vide si hors clé).</summary>
        public string RangCleTexte
        {
            get { return RangCle > 0 ? RangCle.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty; }
        }

        /// <summary>Colonne parente référencée (TABLE.COLONNE) si la colonne porte une clé étrangère, sinon null. (EF-10)</summary>
        public string Reference { get; set; }

        public override string ToString()
        {
            return Nom;
        }
    }
}

namespace SQLUtility.Core.Catalogue
{
    /// <summary>Métadonnées d'une table ou d'une vue. (EF-07)</summary>
    public sealed class TableInfo
    {
        /// <summary>Schéma (bibliothèque) propriétaire.</summary>
        public string Schema { get; set; }

        /// <summary>Nom SQL de la table.</summary>
        public string Nom { get; set; }

        /// <summary>Nom système court (10 caractères) sur IBM i, si différent du nom SQL.</summary>
        public string NomSysteme { get; set; }

        /// <summary>Libellé descriptif (TABLE_TEXT).</summary>
        public string Libelle { get; set; }

        /// <summary>Type : "TABLE", "VIEW", ...</summary>
        public string Type { get; set; }

        /// <summary>SCHEMA.TABLE, ou le nom seul pour une table sans schéma (étape d'une requête WITH).</summary>
        public string NomQualifie
        {
            get { return string.IsNullOrEmpty(Schema) ? Nom : Schema + "." + Nom; }
        }

        public override string ToString()
        {
            return Nom;
        }
    }
}

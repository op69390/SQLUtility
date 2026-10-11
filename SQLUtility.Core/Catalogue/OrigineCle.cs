namespace SQLUtility.Core.Catalogue
{
    /// <summary>
    /// Origine d'un chemin d'accès à clé, par ordre de préférence pour identifier une ligne. (EF-09)
    /// Les fichiers DDS Atlas / MAPICS n'ont généralement pas de contrainte SQL : leur clé
    /// est celle du fichier physique (PHYSICAL) ou d'un logique (LOGICAL).
    /// </summary>
    public enum OrigineCle
    {
        /// <summary>Contrainte PRIMARY KEY.</summary>
        ClePrimaire = 0,

        /// <summary>Contrainte UNIQUE.</summary>
        ContrainteUnique = 1,

        /// <summary>Clé du fichier physique DDS (fichier physique à clé).</summary>
        FichierPhysique = 2,

        /// <summary>Index SQL.</summary>
        Index = 3,

        /// <summary>Fichier logique DDS à clé.</summary>
        FichierLogique = 4,

        /// <summary>Type non reconnu.</summary>
        Autre = 9
    }
}

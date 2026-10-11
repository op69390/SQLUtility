namespace SQLUtility.Core.Edition
{
    /// <summary>Catégorie d'un jeton SQL produit par le <see cref="TokeniseurSql"/>.</summary>
    public enum TypeJetonSql
    {
        Espace,
        MotCle,
        Identifiant,
        Chaine,
        Commentaire,
        Nombre,
        Operateur,
        Ponctuation,
        Inconnu
    }
}

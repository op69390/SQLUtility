namespace SQLUtility.Core.Edition
{
    /// <summary>Jeton SQL : catégorie, position et longueur dans le texte source.</summary>
    public sealed class JetonSql
    {
        public JetonSql(TypeJetonSql type, int position, int longueur, string texte)
        {
            Type = type;
            Position = position;
            Longueur = longueur;
            Texte = texte;
        }

        public TypeJetonSql Type { get; }

        /// <summary>Position (base 0) du jeton dans le texte.</summary>
        public int Position { get; }

        public int Longueur { get; }

        /// <summary>Texte exact du jeton.</summary>
        public string Texte { get; }

        public override string ToString()
        {
            return Type + " '" + Texte + "'";
        }
    }
}

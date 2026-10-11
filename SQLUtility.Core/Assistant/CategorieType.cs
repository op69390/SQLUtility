using System;

namespace SQLUtility.Core.Assistant
{
    /// <summary>Famille d'un type de données DB2 for i, pour la saisie guidée. (EF-40)</summary>
    public enum CategorieType
    {
        Texte,
        Numerique,
        Date,
        Heure,
        Horodatage,
        /// <summary>Binaire, LOB binaire, XML, ROWID... : non saisissable dans l'assistant.</summary>
        NonPrisEnCharge
    }

    /// <summary>Classement des valeurs DATA_TYPE de QSYS2.SYSCOLUMNS.</summary>
    public static class TypesSql
    {
        public static CategorieType Categorie(string typeDonnee)
        {
            string t = (typeDonnee ?? string.Empty).Trim().ToUpperInvariant();
            if (t.StartsWith("TIMEST", StringComparison.Ordinal)) return CategorieType.Horodatage;

            switch (t)
            {
                case "CHAR":
                case "CHARACTER":
                case "VARCHAR":
                case "GRAPHIC":
                case "VARG":
                case "VARGRAPHIC":
                case "NCHAR":
                case "NVARCHAR":
                case "CLOB":
                case "DBCLOB":
                case "NCLOB":
                    return CategorieType.Texte;

                case "DECIMAL":
                case "NUMERIC":
                case "INTEGER":
                case "INT":
                case "SMALLINT":
                case "BIGINT":
                case "FLOAT":
                case "DOUBLE":
                case "REAL":
                case "DECFLOAT":
                    return CategorieType.Numerique;

                case "DATE":
                    return CategorieType.Date;
                case "TIME":
                    return CategorieType.Heure;

                default:
                    return CategorieType.NonPrisEnCharge;
            }
        }

        /// <summary>Type lisible : « DECIMAL(7,0) », « CHAR(15) », « DATE ».</summary>
        public static string Libelle(string typeDonnee, int longueur, int echelle)
        {
            string t = (typeDonnee ?? string.Empty).Trim().ToUpperInvariant();
            switch (t)
            {
                case "DECIMAL":
                case "NUMERIC":
                    return t + "(" + longueur + "," + echelle + ")";
                case "CHAR":
                case "CHARACTER":
                case "VARCHAR":
                case "GRAPHIC":
                case "VARG":
                case "VARGRAPHIC":
                case "NCHAR":
                case "NVARCHAR":
                case "BINARY":
                case "VARBIN":
                    return t + "(" + longueur + ")";
                default:
                    return t;
            }
        }
    }
}

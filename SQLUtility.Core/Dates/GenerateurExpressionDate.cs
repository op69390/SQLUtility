using System;
using System.Globalization;

namespace SQLUtility.Core.Dates
{
    /// <summary>
    /// Génère l'expression SQL DB2 for i qui décode une colonne numérique encodée
    /// (selon un <see cref="FormatDate"/>) en DATE. (EF-17/18)
    /// Hypothèses : colonne numérique entière (échelle 0) ; pivot de siècle =
    /// <see cref="ConvertisseurDate.PivotSiecle"/> ; indicateur C : 0 → 19xx, 1 → 20xx.
    /// Pendant .NET : voir <see cref="ConvertisseurDate"/>.
    /// </summary>
    public static class GenerateurExpressionDate
    {
        /// <summary>Retourne l'expression SQL décodant <paramref name="colonne"/> en DATE.</summary>
        public static string Generer(string colonne, FormatDate format)
        {
            if (string.IsNullOrEmpty(colonne)) throw new ArgumentNullException(nameof(colonne));

            switch (format)
            {
                case FormatDate.Iso: return ExprIso(colonne);
                case FormatDate.Cymd: return ExprCymd(colonne);
                case FormatDate.Yymmdd: return Expr6(colonne, 1, 3, 5);   // AA MM JJ
                case FormatDate.Mmddyy: return Expr6(colonne, 5, 1, 3);   // MM JJ AA
                case FormatDate.Ddmmyy: return Expr6(colonne, 5, 3, 1);   // JJ MM AA
                case FormatDate.JulienYyddd: return ExprJulienYyddd(colonne);
                case FormatDate.JulienCyyddd: return ExprJulienCyyddd(colonne);
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Format de date non géré.");
            }
        }

        // Dn := DIGITS(DECIMAL(colonne, n, 0)) — chaîne de n chiffres, zéro-complétée.
        private static string Digits(string colonne, int n)
        {
            return "DIGITS(DECIMAL(" + colonne + ", " + n.ToString(CultureInfo.InvariantCulture) + ", 0))";
        }

        // Siècle pivoté à partir d'une expression d'année à 2 chiffres : '20' si AA <= pivot, sinon '19'.
        private static string Siecle(string anneeDeuxChiffres)
        {
            string pivot = ConvertisseurDate.PivotSiecle.ToString(CultureInfo.InvariantCulture);
            return "(CASE WHEN INTEGER(" + anneeDeuxChiffres + ") <= " + pivot + " THEN '20' ELSE '19' END)";
        }

        // Année 4 chiffres à partir d'un indicateur de siècle C (1 chiffre) et AA : (19 + C) || AA.
        private static string AnneeAvecIndicateur(string d, int posC, int posAa)
        {
            string c = "SUBSTR(" + d + "," + posC + ",1)";
            string aa = "SUBSTR(" + d + "," + posAa + ",2)";
            return "DIGITS(DECIMAL(19 + INTEGER(" + c + "), 2, 0)) || " + aa;
        }

        private static string ExprIso(string colonne)
        {
            string d8 = Digits(colonne, 8);
            return "DATE(SUBSTR(" + d8 + ",1,4) || '-' || SUBSTR(" + d8 + ",5,2) || '-' || SUBSTR(" + d8 + ",7,2))";
        }

        private static string ExprCymd(string colonne)
        {
            string d7 = Digits(colonne, 7);
            string annee = AnneeAvecIndicateur(d7, 1, 2);
            return "DATE(" + annee + " || '-' || SUBSTR(" + d7 + ",4,2) || '-' || SUBSTR(" + d7 + ",6,2))";
        }

        // Formats 6 chiffres : positions (base 1) de l'année, du mois et du jour dans D6.
        private static string Expr6(string colonne, int posAa, int posMm, int posJj)
        {
            string d6 = Digits(colonne, 6);
            string aa = "SUBSTR(" + d6 + "," + posAa.ToString(CultureInfo.InvariantCulture) + ",2)";
            string mm = "SUBSTR(" + d6 + "," + posMm.ToString(CultureInfo.InvariantCulture) + ",2)";
            string jj = "SUBSTR(" + d6 + "," + posJj.ToString(CultureInfo.InvariantCulture) + ",2)";
            string annee = Siecle(aa) + " || " + aa;
            return "DATE(" + annee + " || '-' || " + mm + " || '-' || " + jj + ")";
        }

        private static string ExprJulienYyddd(string colonne)
        {
            string d5 = Digits(colonne, 5);
            string aa = "SUBSTR(" + d5 + ",1,2)";
            string annee = Siecle(aa) + " || " + aa;
            string ddd = "SUBSTR(" + d5 + ",3,3)";
            return "(DATE(" + annee + " || '-01-01') + (INTEGER(" + ddd + ") - 1) DAYS)";
        }

        private static string ExprJulienCyyddd(string colonne)
        {
            string d6 = Digits(colonne, 6);
            string annee = AnneeAvecIndicateur(d6, 1, 2);
            string ddd = "SUBSTR(" + d6 + ",4,3)";
            return "(DATE(" + annee + " || '-01-01') + (INTEGER(" + ddd + ") - 1) DAYS)";
        }
    }
}

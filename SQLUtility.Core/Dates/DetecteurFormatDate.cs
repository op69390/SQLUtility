using System;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Dates
{
    /// <summary>
    /// Heuristique de détection des colonnes stockant une date encodée (EF-17/18).
    /// Signal : type DECIMAL/NUMERIC, échelle 0, longueur dans {5,6,7,8}. Le format est
    /// déduit de la longueur (valeur par défaut, l'utilisateur confirme à l'insertion).
    /// </summary>
    public static class DetecteurFormatDate
    {
        public static ResultatDetectionDate Detecter(ColonneInfo colonne)
        {
            if (colonne == null)
                return new ResultatDetectionDate(false, FormatDate.Iso);

            if (colonne.Echelle != 0 || !EstTypeDecimal(colonne.TypeDonnee))
                return new ResultatDetectionDate(false, FormatDate.Iso);

            FormatDate format;
            if (!TryFormatParLongueur(colonne.Longueur, out format))
                return new ResultatDetectionDate(false, FormatDate.Iso);

            return new ResultatDetectionDate(true, format);
        }

        private static bool EstTypeDecimal(string type)
        {
            if (string.IsNullOrEmpty(type)) return false;
            string t = type.Trim().ToUpperInvariant();
            // DECIMAL / NUMERIC : la longueur du catalogue correspond au nombre de chiffres.
            return t == "DECIMAL" || t == "NUMERIC" || t.StartsWith("DEC", StringComparison.Ordinal) || t.StartsWith("NUM", StringComparison.Ordinal);
        }

        private static bool TryFormatParLongueur(int longueur, out FormatDate format)
        {
            switch (longueur)
            {
                case 8: format = FormatDate.Iso; return true;          // AAAAMMJJ
                case 7: format = FormatDate.Cymd; return true;         // CYYMMDD
                case 6: format = FormatDate.Yymmdd; return true;       // YYMMDD (défaut ; aussi MMDDYY/DDMMYY/CYYDDD)
                case 5: format = FormatDate.JulienYyddd; return true;  // YYDDD
                default: format = FormatDate.Iso; return false;
            }
        }
    }
}

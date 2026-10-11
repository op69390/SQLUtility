using System;
using System.Globalization;

namespace SQLUtility.Core.Dates
{
    /// <summary>
    /// Conversions entre une valeur encodée (telle que stockée dans DB2 for i)
    /// et un <see cref="DateTime"/> .NET, pour les 7 formats du référentiel.
    /// Réf. cahier des charges : gestion des dates AS/400 (Atlas / MAPICS).
    /// </summary>
    public static class ConvertisseurDate
    {
        /// <summary>
        /// Pivot de siècle pour les formats sans indicateur de siècle (YYMMDD, YYDDD, MMDDYY, DDMMYY).
        /// Une année AA inférieure ou égale au pivot est interprétée comme 20AA, sinon 19AA.
        /// </summary>
        public const int PivotSiecle = 40;

        /// <summary>Décode une valeur encodée vers un <see cref="DateTime"/>.</summary>
        public static DateTime VersDate(string valeur, FormatDate format)
        {
            if (valeur == null) throw new ArgumentNullException(nameof(valeur));
            string v = valeur.Trim();

            switch (format)
            {
                case FormatDate.Iso:
                    return DecoderIso(v);
                case FormatDate.Cymd:
                    return DecoderCymd(v);
                case FormatDate.Yymmdd:
                    return DecoderYymmdd(v);
                case FormatDate.JulienYyddd:
                    return DecoderJulienYyddd(v);
                case FormatDate.Mmddyy:
                    return DecoderMmddyy(v);
                case FormatDate.Ddmmyy:
                    return DecoderDdmmyy(v);
                case FormatDate.JulienCyyddd:
                    return DecoderJulienCyyddd(v);

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Format de date non géré.");
            }
        }

        /// <summary>Encode un <see cref="DateTime"/> vers la représentation du format.</summary>
        public static string DepuisDate(DateTime date, FormatDate format)
        {
            switch (format)
            {
                case FormatDate.Iso:
                    return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                case FormatDate.Cymd:
                    return EncoderCymd(date);
                case FormatDate.Yymmdd:
                    return date.ToString("yyMMdd", CultureInfo.InvariantCulture);
                case FormatDate.JulienYyddd:
                    return EncoderJulienYyddd(date);
                case FormatDate.Mmddyy:
                    return date.ToString("MMddyy", CultureInfo.InvariantCulture);
                case FormatDate.Ddmmyy:
                    return date.ToString("ddMMyy", CultureInfo.InvariantCulture);
                case FormatDate.JulienCyyddd:
                    return EncoderJulienCyyddd(date);

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Format de date non géré.");
            }
        }

        // ---------------------------------------------------------------- Décodage

        private static DateTime DecoderIso(string v)
        {
            // Accepte "AAAA-MM-JJ" ou 8 chiffres "AAAAMMJJ".
            DateTime resultat;
            if (DateTime.TryParseExact(v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out resultat))
                return resultat;
            if (DateTime.TryParseExact(v, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out resultat))
                return resultat;
            throw new FormatException("Valeur ISO invalide : '" + v + "' (attendu AAAA-MM-JJ ou AAAAMMJJ).");
        }

        private static DateTime DecoderCymd(string v)
        {
            // CYYMMDD : 7 chiffres. C = siècle (0 -> 19xx, 1 -> 20xx, ...).
            string n = ValiderNumerique(v, 7, "CYYMMDD");
            int c = int.Parse(n.Substring(0, 1), CultureInfo.InvariantCulture);
            int yy = int.Parse(n.Substring(1, 2), CultureInfo.InvariantCulture);
            int mm = int.Parse(n.Substring(3, 2), CultureInfo.InvariantCulture);
            int jj = int.Parse(n.Substring(5, 2), CultureInfo.InvariantCulture);
            int annee = 1900 + (c * 100) + yy;
            return new DateTime(annee, mm, jj);
        }

        private static DateTime DecoderYymmdd(string v)
        {
            string n = ValiderNumerique(v, 6, "YYMMDD");
            int yy = int.Parse(n.Substring(0, 2), CultureInfo.InvariantCulture);
            int mm = int.Parse(n.Substring(2, 2), CultureInfo.InvariantCulture);
            int jj = int.Parse(n.Substring(4, 2), CultureInfo.InvariantCulture);
            return new DateTime(AnneeDepuisPivot(yy), mm, jj);
        }

        private static DateTime DecoderJulienYyddd(string v)
        {
            // YYDDD : 5 chiffres. DDD = quantième (1..366).
            string n = ValiderNumerique(v, 5, "YYDDD");
            int yy = int.Parse(n.Substring(0, 2), CultureInfo.InvariantCulture);
            int ddd = int.Parse(n.Substring(2, 3), CultureInfo.InvariantCulture);
            int annee = AnneeDepuisPivot(yy);
            ValiderQuantieme(ddd, annee);
            return new DateTime(annee, 1, 1).AddDays(ddd - 1);
        }

        private static DateTime DecoderMmddyy(string v)
        {
            // MMDDYY : 6 chiffres. Siècle déduit par pivot.
            string n = ValiderNumerique(v, 6, "MMDDYY");
            int mm = int.Parse(n.Substring(0, 2), CultureInfo.InvariantCulture);
            int jj = int.Parse(n.Substring(2, 2), CultureInfo.InvariantCulture);
            int yy = int.Parse(n.Substring(4, 2), CultureInfo.InvariantCulture);
            return new DateTime(AnneeDepuisPivot(yy), mm, jj);
        }

        private static DateTime DecoderDdmmyy(string v)
        {
            // DDMMYY : 6 chiffres. Siècle déduit par pivot.
            string n = ValiderNumerique(v, 6, "DDMMYY");
            int jj = int.Parse(n.Substring(0, 2), CultureInfo.InvariantCulture);
            int mm = int.Parse(n.Substring(2, 2), CultureInfo.InvariantCulture);
            int yy = int.Parse(n.Substring(4, 2), CultureInfo.InvariantCulture);
            return new DateTime(AnneeDepuisPivot(yy), mm, jj);
        }

        private static DateTime DecoderJulienCyyddd(string v)
        {
            // CYYDDD : 6 chiffres. C = siècle (0 -> 19xx, 1 -> 20xx), DDD = quantième (1..366).
            string n = ValiderNumerique(v, 6, "CYYDDD");
            int c = int.Parse(n.Substring(0, 1), CultureInfo.InvariantCulture);
            int yy = int.Parse(n.Substring(1, 2), CultureInfo.InvariantCulture);
            int ddd = int.Parse(n.Substring(3, 3), CultureInfo.InvariantCulture);
            int annee = 1900 + (c * 100) + yy;
            ValiderQuantieme(ddd, annee);
            return new DateTime(annee, 1, 1).AddDays(ddd - 1);
        }

        // ---------------------------------------------------------------- Encodage

        private static string EncoderCymd(DateTime date)
        {
            int c = ValiderSiecleEtCalculer(date);
            int yy = (date.Year - 1900) % 100;
            return c.ToString(CultureInfo.InvariantCulture)
                 + yy.ToString("00", CultureInfo.InvariantCulture)
                 + date.ToString("MMdd", CultureInfo.InvariantCulture);
        }

        private static string EncoderJulienYyddd(DateTime date)
        {
            int yy = date.Year % 100;
            int ddd = date.DayOfYear;
            return yy.ToString("00", CultureInfo.InvariantCulture)
                 + ddd.ToString("000", CultureInfo.InvariantCulture);
        }

        private static string EncoderJulienCyyddd(DateTime date)
        {
            int c = ValiderSiecleEtCalculer(date);
            int yy = (date.Year - 1900) % 100;
            int ddd = date.DayOfYear;
            return c.ToString(CultureInfo.InvariantCulture)
                 + yy.ToString("00", CultureInfo.InvariantCulture)
                 + ddd.ToString("000", CultureInfo.InvariantCulture);
        }

        // ---------------------------------------------------------------- Utilitaires

        private static int AnneeDepuisPivot(int yy)
        {
            return (yy <= PivotSiecle) ? (2000 + yy) : (1900 + yy);
        }

        /// <summary>Vérifie la plage des formats à indicateur de siècle et retourne C (chiffre de siècle).</summary>
        private static int ValiderSiecleEtCalculer(DateTime date)
        {
            if (date.Year < 1900 || date.Year > 2899)
                throw new ArgumentOutOfRangeException(nameof(date), "Année hors plage pour un format à siècle (1900-2899).");
            return (date.Year - 1900) / 100;
        }

        private static string ValiderNumerique(string v, int longueur, string libelleFormat)
        {
            if (string.IsNullOrEmpty(v))
                throw new FormatException("Valeur vide pour le format " + libelleFormat + ".");

            // Tolère un zéro de tête manquant en complétant à gauche.
            string n = v.PadLeft(longueur, '0');
            if (n.Length != longueur)
                throw new FormatException("Valeur '" + v + "' invalide pour " + libelleFormat
                                          + " (longueur " + longueur + " attendue).");
            foreach (char ch in n)
            {
                if (ch < '0' || ch > '9')
                    throw new FormatException("Valeur '" + v + "' non numérique pour " + libelleFormat + ".");
            }
            return n;
        }

        private static void ValiderQuantieme(int ddd, int annee)
        {
            int max = DateTime.IsLeapYear(annee) ? 366 : 365;
            if (ddd < 1 || ddd > max)
                throw new FormatException("Quantième " + ddd + " hors plage (1.." + max
                                          + ") pour l'année " + annee + ".");
        }
    }
}

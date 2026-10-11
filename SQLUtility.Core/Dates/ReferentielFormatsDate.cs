using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Dates
{
    /// <summary>
    /// Référentiel des 7 formats de date : libellé, longueur, exemple et état d'implémentation.
    /// </summary>
    public static class ReferentielFormatsDate
    {
        private static readonly Dictionary<FormatDate, DescriptionFormatDate> Descriptions =
            new Dictionary<FormatDate, DescriptionFormatDate>
            {
                { FormatDate.Iso,          new DescriptionFormatDate(FormatDate.Iso,          "ISO AAAA-MM-JJ",          10, false, true,  "2026-06-15") },
                { FormatDate.Cymd,         new DescriptionFormatDate(FormatDate.Cymd,         "CYYMMDD (siècle+AAMMJJ)",  7, true,  true,  "1260615") },
                { FormatDate.Yymmdd,       new DescriptionFormatDate(FormatDate.Yymmdd,       "YYMMDD",                   6, true,  true,  "260615") },
                { FormatDate.JulienYyddd,  new DescriptionFormatDate(FormatDate.JulienYyddd,  "Julien YYDDD",             5, true,  true,  "26166") },
                { FormatDate.Mmddyy,       new DescriptionFormatDate(FormatDate.Mmddyy,       "MMDDYY",                   6, true,  true,  "061526") },
                { FormatDate.Ddmmyy,       new DescriptionFormatDate(FormatDate.Ddmmyy,       "DDMMYY",                   6, true,  true,  "150626") },
                { FormatDate.JulienCyyddd, new DescriptionFormatDate(FormatDate.JulienCyyddd, "Julien CYYDDD",            6, true,  true,  "126166") }
            };

        /// <summary>Toutes les descriptions de format, dans l'ordre de l'énumération.</summary>
        public static IReadOnlyCollection<DescriptionFormatDate> Tous
        {
            get { return new ReadOnlyCollection<DescriptionFormatDate>(new List<DescriptionFormatDate>(Descriptions.Values)); }
        }

        /// <summary>Retourne la description d'un format donné.</summary>
        public static DescriptionFormatDate Obtenir(FormatDate format)
        {
            DescriptionFormatDate description;
            if (!Descriptions.TryGetValue(format, out description))
                throw new ArgumentOutOfRangeException(nameof(format), format, "Format de date inconnu.");
            return description;
        }

        /// <summary>Indique si la conversion du format est disponible.</summary>
        public static bool EstImplemente(FormatDate format)
        {
            return Obtenir(format).Implemente;
        }
    }
}

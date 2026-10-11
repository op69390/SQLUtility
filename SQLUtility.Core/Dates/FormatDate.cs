namespace SQLUtility.Core.Dates
{
    /// <summary>
    /// Les 7 formats de date pris en charge (encodages de stockage AS/400, ERP Atlas / MAPICS).
    /// </summary>
    public enum FormatDate
    {
        /// <summary>ISO 8601 : AAAA-MM-JJ (ou AAAAMMJJ). Déjà codé.</summary>
        Iso = 0,

        /// <summary>CYYMMDD : 7 chiffres, C = indicateur de siècle (0 = 19xx, 1 = 20xx). Déjà codé.</summary>
        Cymd = 1,

        /// <summary>YYMMDD : 6 chiffres, siècle déduit par pivot. Déjà codé.</summary>
        Yymmdd = 2,

        /// <summary>YYDDD : julien 5 chiffres (AA + quantième). Déjà codé.</summary>
        JulienYyddd = 3,

        /// <summary>MMDDYY : 6 chiffres, siècle déduit par pivot. Codé.</summary>
        Mmddyy = 4,

        /// <summary>DDMMYY : 6 chiffres, siècle déduit par pivot. Codé.</summary>
        Ddmmyy = 5,

        /// <summary>CYYDDD : julien 6 chiffres avec indicateur de siècle. Codé.</summary>
        JulienCyyddd = 6
    }
}

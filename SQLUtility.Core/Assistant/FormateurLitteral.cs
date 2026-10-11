using System;
using System.Globalization;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Dates;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Transforme une saisie utilisateur en littéral SQL DB2 for i conforme au type de la
    /// colonne, avec contrôle de longueur, de précision et d'échelle. (EF-40)
    /// Les dates Atlas stockées en numérique (CYYMMDD...) sont converties en valeur stockée :
    /// la comparaison porte alors sur la colonne brute et reste exploitable par un index.
    /// </summary>
    public static class FormateurLitteral
    {
        private static readonly string[] FormatsDate = { "d/M/yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "d.M.yyyy", "d-M-yyyy" };
        private static readonly string[] FormatsHeure = { "H:mm", "H:mm:ss", "H.mm", "H.mm.ss", "HH:mm:ss" };
        private static readonly string[] FormatsHorodatage =
        {
            "d/M/yyyy H:mm", "d/M/yyyy H:mm:ss", "d/M/yyyy", "dd/MM/yyyy HH:mm:ss",
            "yyyy-MM-dd HH:mm", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd-HH.mm.ss", "yyyy-MM-dd-HH.mm.ss.ffffff",
            "yyyy-MM-dd HH:mm:ss.ffffff", "yyyy-MM-dd"
        };

        /// <param name="formatDateAtlas">Format de date stocké si la colonne numérique contient une date, sinon null.</param>
        /// <returns>Vrai si la saisie est valide ; <paramref name="litteral"/> contient alors le littéral SQL.</returns>
        public static bool TryFormater(ColonneInfo colonne, string saisie, FormatDate? formatDateAtlas,
            out string litteral, out string erreur)
        {
            if (colonne == null) throw new ArgumentNullException(nameof(colonne));
            litteral = null;
            erreur = null;
            string valeur = saisie ?? string.Empty;

            switch (TypesSql.Categorie(colonne.TypeDonnee))
            {
                case CategorieType.Texte:
                    return FormaterTexte(colonne, valeur, out litteral, out erreur);
                case CategorieType.Numerique:
                    if (formatDateAtlas.HasValue && ResembleDate(valeur))
                        return FormaterDateAtlas(colonne, valeur.Trim(), formatDateAtlas.Value, out litteral, out erreur);
                    return FormaterNombre(colonne, valeur.Trim(), out litteral, out erreur);
                case CategorieType.Date:
                    return FormaterDateHeure(colonne, valeur.Trim(), FormatsDate, "yyyy-MM-dd", "jj/mm/aaaa", out litteral, out erreur);
                case CategorieType.Heure:
                    return FormaterDateHeure(colonne, valeur.Trim(), FormatsHeure, "HH:mm:ss", "hh:mm ou hh:mm:ss", out litteral, out erreur);
                case CategorieType.Horodatage:
                    return FormaterDateHeure(colonne, valeur.Trim(), FormatsHorodatage, "yyyy-MM-dd-HH.mm.ss.ffffff", "jj/mm/aaaa hh:mm:ss", out litteral, out erreur);
                default:
                    erreur = colonne.Nom + " : type " + colonne.TypeDonnee + " non pris en charge par l'assistant.";
                    return false;
            }
        }

        /// <summary>Délimite une chaîne SQL ('...'), en doublant les apostrophes.</summary>
        public static string Chaine(string valeur)
        {
            return "'" + (valeur ?? string.Empty).Replace("'", "''") + "'";
        }

        /// <summary>
        /// Une saisie contenant « / », un « - » hors signe de tête ou deux « . » est lue comme
        /// une date ; sinon comme la valeur stockée elle-même (ex. 1261001 ou 0).
        /// </summary>
        private static bool ResembleDate(string valeur)
        {
            string v = valeur.Trim();
            if (v.Length == 0) return false;
            bool deuxPoints = v.IndexOf('.') >= 0 && v.IndexOf('.') != v.LastIndexOf('.');
            return v.IndexOf('/') >= 0 || v.IndexOf('-', 1) >= 0 || deuxPoints;
        }

        private static bool FormaterTexte(ColonneInfo colonne, string valeur, out string litteral, out string erreur)
        {
            erreur = null;
            litteral = null;
            // CLOB / DBCLOB : longueur du catalogue non bornante ici ; sinon contrôle strict.
            string type = (colonne.TypeDonnee ?? string.Empty).ToUpperInvariant();
            bool lob = type.EndsWith("CLOB", StringComparison.Ordinal);
            if (!lob && colonne.Longueur > 0 && valeur.Length > colonne.Longueur)
            {
                erreur = colonne.Nom + " : " + valeur.Length + " caractères pour " + colonne.Longueur + " au maximum.";
                return false;
            }
            litteral = Chaine(valeur);
            return true;
        }

        private static bool FormaterNombre(ColonneInfo colonne, string valeur, out string litteral, out string erreur)
        {
            litteral = null;
            erreur = null;
            // Accepte la virgule ou le point décimal, et les espaces de milliers.
            string normalise = valeur.Replace(" ", string.Empty).Replace(" ", string.Empty).Replace(" ", string.Empty).Replace(',', '.');
            decimal nombre;
            if (normalise.Length == 0 || !decimal.TryParse(normalise, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out nombre))
            {
                erreur = colonne.Nom + " : « " + valeur + " » n'est pas un nombre.";
                return false;
            }

            string type = (colonne.TypeDonnee ?? string.Empty).Trim().ToUpperInvariant();
            if (type == "DECIMAL" || type == "NUMERIC")
            {
                int decimales = NombreDecimales(normalise);
                if (decimales > colonne.Echelle)
                {
                    erreur = colonne.Nom + " : " + decimales + " décimale(s) pour " + colonne.Echelle + " au maximum.";
                    return false;
                }
                int chiffresEntiers = ChiffresEntiers(nombre);
                if (chiffresEntiers > colonne.Longueur - colonne.Echelle)
                {
                    erreur = colonne.Nom + " : trop grand pour " + TypesSql.Libelle(type, colonne.Longueur, colonne.Echelle) + ".";
                    return false;
                }
            }
            else if (type == "SMALLINT" || type == "INTEGER" || type == "INT" || type == "BIGINT")
            {
                if (decimal.Truncate(nombre) != nombre)
                {
                    erreur = colonne.Nom + " : un entier est attendu.";
                    return false;
                }
                decimal min = type == "SMALLINT" ? short.MinValue : type == "BIGINT" ? long.MinValue : int.MinValue;
                decimal max = type == "SMALLINT" ? short.MaxValue : type == "BIGINT" ? long.MaxValue : int.MaxValue;
                if (nombre < min || nombre > max)
                {
                    erreur = colonne.Nom + " : hors des bornes du type " + type + ".";
                    return false;
                }
            }

            litteral = nombre.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        private static bool FormaterDateAtlas(ColonneInfo colonne, string valeur, FormatDate format, out string litteral, out string erreur)
        {
            litteral = null;
            erreur = null;
            DateTime date;
            if (!DateTime.TryParseExact(valeur, FormatsDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                erreur = colonne.Nom + " : date « " + valeur + " » illisible (jj/mm/aaaa attendu).";
                return false;
            }
            string encodee;
            try
            {
                encodee = ConvertisseurDate.DepuisDate(date, format);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                erreur = colonne.Nom + " : " + ex.Message;
                return false;
            }
            // Le format ISO s'écrit AAAA-MM-JJ ; stocké en numérique, il devient AAAAMMJJ.
            encodee = encodee.Replace("-", string.Empty);
            return FormaterNombre(colonne, encodee, out litteral, out erreur);
        }

        private static bool FormaterDateHeure(ColonneInfo colonne, string valeur, string[] formats, string formatSql,
            string attendu, out string litteral, out string erreur)
        {
            litteral = null;
            erreur = null;
            DateTime instant;
            if (!DateTime.TryParseExact(valeur, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out instant))
            {
                erreur = colonne.Nom + " : « " + valeur + " » illisible (" + attendu + " attendu).";
                return false;
            }
            // Formats ISO / JIS : reconnus par DB2 for i quel que soit le format de date du travail.
            litteral = "'" + instant.ToString(formatSql, CultureInfo.InvariantCulture) + "'";
            return true;
        }

        private static int NombreDecimales(string normalise)
        {
            int point = normalise.IndexOf('.');
            if (point < 0) return 0;
            return normalise.Substring(point + 1).TrimEnd('0').Length;
        }

        private static int ChiffresEntiers(decimal nombre)
        {
            decimal entier = decimal.Truncate(Math.Abs(nombre));
            return entier == 0m ? 0 : entier.ToString(CultureInfo.InvariantCulture).Length;
        }
    }
}

using System;
using System.IO;
using System.Text;

namespace SQLUtility.Data.Fichiers
{
    /// <summary>
    /// Lecture et écriture des scripts .sql de l'éditeur. (EF-33)
    /// Lecture tolérante : UTF-8 (avec ou sans BOM), UTF-16 (BOM), sinon ANSI Windows-1252,
    /// encodage des scripts enregistrés par les outils Windows plus anciens.
    /// Écriture en UTF-8 avec BOM (accents préservés, reconnus par ACS « Exécuter des scripts
    /// SQL »), fins de ligne Windows, et atomique : fichier temporaire puis remplacement.
    /// </summary>
    public static class FichierSql
    {
        /// <summary>Filtre des boîtes de dialogue Ouvrir / Enregistrer.</summary>
        public const string Filtre = "Scripts SQL (*.sql)|*.sql|Fichiers texte (*.txt)|*.txt|Tous les fichiers (*.*)|*.*";

        /// <summary>Encodage d'écriture : UTF-8 avec BOM.</summary>
        public static readonly Encoding EncodageEcriture = new UTF8Encoding(true);

        public static string Lire(string chemin)
        {
            if (string.IsNullOrEmpty(chemin)) throw new ArgumentNullException(nameof(chemin));
            return Decoder(File.ReadAllBytes(chemin));
        }

        /// <summary>Décode le contenu d'un script selon son BOM, ou à défaut UTF-8 strict puis Windows-1252.</summary>
        public static string Decoder(byte[] octets)
        {
            if (octets == null || octets.Length == 0) return string.Empty;
            if (octets.Length >= 3 && octets[0] == 0xEF && octets[1] == 0xBB && octets[2] == 0xBF)
                return Encoding.UTF8.GetString(octets, 3, octets.Length - 3);
            if (octets.Length >= 2 && octets[0] == 0xFF && octets[1] == 0xFE)
                return Encoding.Unicode.GetString(octets, 2, octets.Length - 2);
            if (octets.Length >= 2 && octets[0] == 0xFE && octets[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(octets, 2, octets.Length - 2);
            try
            {
                // UTF-8 strict : une séquence invalide signale un fichier ANSI.
                return new UTF8Encoding(false, true).GetString(octets);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.GetEncoding(1252).GetString(octets);
            }
        }

        /// <summary>Écrit le script (UTF-8 avec BOM, CRLF), sans jamais laisser un fichier à moitié écrit.</summary>
        public static void Ecrire(string chemin, string texte)
        {
            if (string.IsNullOrEmpty(chemin)) throw new ArgumentNullException(nameof(chemin));
            string repertoire = Path.GetDirectoryName(Path.GetFullPath(chemin));
            if (!string.IsNullOrEmpty(repertoire) && !Directory.Exists(repertoire)) Directory.CreateDirectory(repertoire);

            string cheminTemp = chemin + ".tmp";
            File.WriteAllText(cheminTemp, NormaliserFinsDeLigne(texte), EncodageEcriture);
            if (File.Exists(chemin))
                File.Replace(cheminTemp, chemin, null);
            else
                File.Move(cheminTemp, chemin);
        }

        /// <summary>Fins de ligne Windows (CRLF), quel que soit le texte reçu (l'éditeur travaille en LF).</summary>
        public static string NormaliserFinsDeLigne(string texte)
        {
            if (string.IsNullOrEmpty(texte)) return string.Empty;
            return texte.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\r\n");
        }
    }
}

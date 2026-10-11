using System;
using System.Security.Cryptography;
using System.Text;

namespace SQLUtility.Data.Securite
{
    /// <summary>
    /// Chiffrement / déchiffrement de secrets via DPAPI (portée utilisateur courant).
    /// Le secret chiffré est encodé en Base64 pour un stockage texte (JSON). (EF-05)
    /// </summary>
    public static class ProtecteurSecret
    {
        // Entropie additionnelle pour cloisonner les secrets de cette application.
        private static readonly byte[] EntropieApplication =
            Encoding.UTF8.GetBytes("SQLUtility.DPAPI.v1");

        /// <summary>Chiffre un secret en clair ; retourne du Base64 (une valeur vide ou nulle est renvoyée telle quelle).</summary>
        public static string Chiffrer(string secretEnClair)
        {
            if (string.IsNullOrEmpty(secretEnClair))
                return secretEnClair;

            byte[] donnees = Encoding.UTF8.GetBytes(secretEnClair);
            byte[] chiffre = ProtectedData.Protect(donnees, EntropieApplication, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(chiffre);
        }

        /// <summary>Déchiffre un secret Base64 produit par <see cref="Chiffrer"/>.</summary>
        public static string Dechiffrer(string secretChiffreBase64)
        {
            if (string.IsNullOrEmpty(secretChiffreBase64))
                return secretChiffreBase64;

            byte[] chiffre = Convert.FromBase64String(secretChiffreBase64);
            byte[] donnees = ProtectedData.Unprotect(chiffre, EntropieApplication, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(donnees);
        }
    }
}

using System;
using System.Collections.Generic;

namespace SQLUtility.Core.Edition
{
    /// <summary>
    /// Sélection et classement des suggestions d'auto-complétion (logique pure, EF-16).
    /// Les candidats dont le préfixe correspond passent avant ceux qui le contiennent.
    /// </summary>
    public static class MoteurCompletion
    {
        /// <summary>
        /// Retourne les suggestions correspondant au préfixe, dédoublonnées (insensible à la
        /// casse), préfixes d'abord puis « contient », triées, limitées à <paramref name="maxResultats"/>.
        /// </summary>
        public static IReadOnlyList<string> Suggerer(string prefixe, IEnumerable<string> candidats, int maxResultats)
        {
            List<string> resultats = new List<string>();
            if (candidats == null) return resultats;

            string p = prefixe ?? string.Empty;
            HashSet<string> vus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<string> debutent = new List<string>();
            List<string> contiennent = new List<string>();

            foreach (string candidat in candidats)
            {
                if (string.IsNullOrEmpty(candidat)) continue;
                if (string.Equals(candidat, p, StringComparison.OrdinalIgnoreCase)) continue; // déjà saisi en entier
                if (!vus.Add(candidat)) continue;

                if (p.Length == 0 || candidat.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                    debutent.Add(candidat);
                else if (candidat.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0)
                    contiennent.Add(candidat);
            }

            debutent.Sort(StringComparer.OrdinalIgnoreCase);
            contiennent.Sort(StringComparer.OrdinalIgnoreCase);
            resultats.AddRange(debutent);
            resultats.AddRange(contiennent);

            if (maxResultats > 0 && resultats.Count > maxResultats)
                resultats = resultats.GetRange(0, maxResultats);
            return resultats;
        }

        /// <summary>Identifiants distincts (longueur > 1) présents dans le texte SQL.</summary>
        public static IReadOnlyList<string> IdentifiantsDistincts(string sql)
        {
            List<string> liste = new List<string>();
            HashSet<string> vus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JetonSql jeton in TokeniseurSql.Tokeniser(sql))
            {
                if (jeton.Type == TypeJetonSql.Identifiant && jeton.Texte.Length > 1 && vus.Add(jeton.Texte))
                    liste.Add(jeton.Texte);
            }
            return liste;
        }
    }
}

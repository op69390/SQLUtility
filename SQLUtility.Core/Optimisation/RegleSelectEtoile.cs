using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SQLUtility.Core.Optimisation
{
    /// <summary>
    /// OPT-01 : déconseille l'usage de SELECT * (colonnes implicites, fragilité au
    /// changement de schéma, surcoût de transfert). Règle d'exemple du moteur de lint.
    /// </summary>
    public sealed class RegleSelectEtoile : IRegleAnalyse
    {
        // SELECT éventuellement suivi de ALL / DISTINCT, puis une étoile.
        // Le COUNT(*) n'est pas concerné (l'étoile n'y suit pas directement SELECT).
        private static readonly Regex Motif =
            new Regex(@"\bSELECT\b\s+(?:ALL\s+|DISTINCT\s+)?\*",
                      RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public string Code { get { return "OPT-01"; } }

        public string Libelle { get { return "Éviter SELECT *"; } }

        public Severite SeveriteParDefaut { get { return Severite.Avertissement; } }

        public IEnumerable<ResultatAnalyse> Analyser(string sql)
        {
            var resultats = new List<ResultatAnalyse>();
            if (string.IsNullOrEmpty(sql))
                return resultats;

            foreach (Match m in Motif.Matches(sql))
            {
                var resultat = new ResultatAnalyse(
                    Code,
                    SeveriteParDefaut,
                    "Usage de SELECT * détecté.",
                    "Lister explicitement les colonnes nécessaires.");
                resultat.Position = m.Index;
                resultat.Longueur = m.Length;
                resultats.Add(resultat);
            }
            return resultats;
        }
    }
}

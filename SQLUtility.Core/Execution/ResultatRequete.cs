using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Résultat d'une requête SELECT : noms de colonnes et lignes de valeurs.
    /// Type neutre (sans ADO.NET) pour rester dans la couche Core. (EF-22 à EF-32)
    /// </summary>
    public sealed class ResultatRequete
    {
        public ResultatRequete(IList<string> colonnes, IList<object[]> lignes, bool tronque)
        {
            Colonnes = new ReadOnlyCollection<string>(colonnes ?? new List<string>());
            Lignes = new ReadOnlyCollection<object[]>(lignes ?? new List<object[]>());
            Tronque = tronque;
        }

        public IReadOnlyList<string> Colonnes { get; }

        /// <summary>Lignes ; chaque ligne est un tableau de valeurs aligné sur <see cref="Colonnes"/> (null pour NULL).</summary>
        public IReadOnlyList<object[]> Lignes { get; }

        /// <summary>Vrai si le résultat a été tronqué (limite de lignes atteinte).</summary>
        public bool Tronque { get; }
    }
}

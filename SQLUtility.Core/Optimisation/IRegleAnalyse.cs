using System.Collections.Generic;

namespace SQLUtility.Core.Optimisation
{
    /// <summary>Règle d'analyse statique d'une requête SQL (lint). (OPT-xx)</summary>
    public interface IRegleAnalyse
    {
        /// <summary>Code de la règle, ex. "OPT-01".</summary>
        string Code { get; }

        /// <summary>Libellé court de la règle.</summary>
        string Libelle { get; }

        /// <summary>Sévérité attribuée par défaut aux résultats de la règle.</summary>
        Severite SeveriteParDefaut { get; }

        /// <summary>Analyse le texte SQL et retourne les anomalies détectées.</summary>
        IEnumerable<ResultatAnalyse> Analyser(string sql);
    }
}

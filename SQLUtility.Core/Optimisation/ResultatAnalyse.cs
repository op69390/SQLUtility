namespace SQLUtility.Core.Optimisation
{
    /// <summary>Résultat unitaire d'une règle d'analyse (lint SQL). (OPT-xx)</summary>
    public sealed class ResultatAnalyse
    {
        public ResultatAnalyse(string codeRegle, Severite severite, string message, string suggestion)
        {
            CodeRegle = codeRegle;
            Severite = severite;
            Message = message;
            Suggestion = suggestion;
            Position = -1;
            Longueur = 0;
        }

        /// <summary>Code de la règle d'origine, ex. "OPT-01".</summary>
        public string CodeRegle { get; }

        public Severite Severite { get; }
        public string Message { get; }

        /// <summary>Suggestion de correction présentée à l'utilisateur.</summary>
        public string Suggestion { get; }

        /// <summary>Position (base 0) dans le texte SQL, -1 si non localisée.</summary>
        public int Position { get; set; }

        /// <summary>Longueur du fragment concerné, pour le surlignage dans l'éditeur.</summary>
        public int Longueur { get; set; }
    }
}

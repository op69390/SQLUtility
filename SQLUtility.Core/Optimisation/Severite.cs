namespace SQLUtility.Core.Optimisation
{
    /// <summary>Niveau de sévérité d'un résultat d'analyse (lint SQL).</summary>
    public enum Severite
    {
        /// <summary>Information ou bonne pratique, sans incidence directe.</summary>
        Information = 0,

        /// <summary>Avertissement : risque de performance ou de fragilité.</summary>
        Avertissement = 1,

        /// <summary>Erreur : problème probable à corriger.</summary>
        Erreur = 2
    }
}

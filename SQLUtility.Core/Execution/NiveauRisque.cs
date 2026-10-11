namespace SQLUtility.Core.Execution
{
    /// <summary>Niveau de risque d'une instruction (conditionne la confirmation). (EF-22 à EF-32)</summary>
    public enum NiveauRisque
    {
        /// <summary>Lecture seule (SELECT) : aucun risque de modification.</summary>
        Faible = 0,

        /// <summary>Modification limitée ou type non critique.</summary>
        Modere = 1,

        /// <summary>Modification de données ou DDL : confirmation requise.</summary>
        Eleve = 2,

        /// <summary>Opération massive ou irréversible : confirmation renforcée.</summary>
        Critique = 3
    }
}

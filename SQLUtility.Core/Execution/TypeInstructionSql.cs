namespace SQLUtility.Core.Execution
{
    /// <summary>Type d'une instruction SQL, pour la confirmation graduée à l'exécution. (EF-22)</summary>
    public enum TypeInstructionSql
    {
        Inconnu,
        Select,
        Insert,
        Update,
        Delete,
        /// <summary>CREATE / ALTER / DROP / GRANT / REVOKE / LABEL / COMMENT...</summary>
        Ddl,
        /// <summary>Autre instruction modifiant potentiellement les données (MERGE, CALL...).</summary>
        Autre
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Edition
{
    /// <summary>
    /// Référentiel des mots-clés SQL (courants et DB2 for i), partagé par le
    /// <see cref="TokeniseurSql"/> (coloration) et le <see cref="MoteurCompletion"/> (EF-16).
    /// </summary>
    public static class MotsClesSql
    {
        private static readonly HashSet<string> Ensemble = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "AND", "OR", "NOT", "NULL", "IS", "IN", "LIKE", "BETWEEN", "EXISTS",
            "JOIN", "INNER", "LEFT", "RIGHT", "FULL", "OUTER", "CROSS", "ON", "USING",
            "GROUP", "BY", "HAVING", "ORDER", "ASC", "DESC", "UNION", "ALL", "DISTINCT",
            "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "MERGE",
            "CREATE", "TABLE", "VIEW", "INDEX", "DROP", "ALTER", "ADD", "COLUMN",
            "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "CONSTRAINT", "UNIQUE", "CHECK",
            "AS", "CASE", "WHEN", "THEN", "ELSE", "END", "CAST", "COALESCE", "NULLIF",
            "COUNT", "SUM", "AVG", "MIN", "MAX",
            "FETCH", "FIRST", "NEXT", "ROW", "ROWS", "ONLY", "OFFSET", "WITH", "RECURSIVE",
            "CURRENT", "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP",
            "DATE", "TIME", "TIMESTAMP", "CHAR", "VARCHAR", "DECIMAL", "NUMERIC", "INTEGER", "SMALLINT", "BIGINT",
            "SCHEMA", "LIBRARY", "DEFAULT", "GENERATED", "IDENTITY",
            "BEGIN", "COMMIT", "ROLLBACK", "GRANT", "REVOKE", "LABEL", "COMMENT"
        };

        private static readonly ReadOnlyCollection<string> Liste = ConstruireListeTriee();

        /// <summary>Tous les mots-clés, triés alphabétiquement.</summary>
        public static IReadOnlyList<string> Tous { get { return Liste; } }

        /// <summary>Indique si un mot est un mot-clé SQL (insensible à la casse).</summary>
        public static bool EstMotCle(string mot)
        {
            return !string.IsNullOrEmpty(mot) && Ensemble.Contains(mot);
        }

        private static ReadOnlyCollection<string> ConstruireListeTriee()
        {
            List<string> liste = new List<string>(Ensemble);
            liste.Sort(StringComparer.OrdinalIgnoreCase);
            return new ReadOnlyCollection<string>(liste);
        }
    }
}

using System.Collections.Generic;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Connexions
{
    /// <summary>
    /// Abstraction d'accès à un SGBD, sans dépendance technique (ni ADO.NET, ni IBM, ni fichiers).
    /// Les implémentations concrètes vivent dans la couche SQLUtility.Data.
    /// </summary>
    public interface IConnexionProvider
    {
        /// <summary>Type de SGBD géré par cette implémentation.</summary>
        TypeSgbd Type { get; }

        /// <summary>Teste l'ouverture d'une connexion. (EF-04)</summary>
        ResultatTestConnexion TesterConnexion(ConnexionProfil profil);

        /// <summary>Liste les schémas (bibliothèques) accessibles. (EF-07)</summary>
        IReadOnlyList<string> ListerSchemas(ConnexionProfil profil);

        /// <summary>Liste les tables et vues d'un schéma. (EF-07)</summary>
        IReadOnlyList<TableInfo> ListerTables(ConnexionProfil profil, string schema);

        /// <summary>Liste les colonnes d'une table. (EF-08)</summary>
        IReadOnlyList<ColonneInfo> ListerColonnes(ConnexionProfil profil, string schema, string table);

        /// <summary>
        /// Décrit une table : colonnes annotées, clés (contraintes, clé DDS, index),
        /// clé d'identification et relations déclarées. (EF-08 à EF-10)
        /// </summary>
        StructureTable DecrireTable(ConnexionProfil profil, TableInfo table);

        /// <summary>Noms de colonnes distincts d'un schéma, pour l'auto-complétion. (EF-16)</summary>
        IReadOnlyList<string> ListerNomsColonnes(ConnexionProfil profil, string schema);

        /// <summary>
        /// Indique si une table est journalisée. Détermine l'irréversibilité d'un DML
        /// et conditionne l'avertissement renforcé avant exécution. (EF-30)
        /// </summary>
        bool EstJournalisee(ConnexionProfil profil, string schema, string table);
    }
}

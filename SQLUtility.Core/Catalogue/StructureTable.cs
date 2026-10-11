using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Catalogue
{
    /// <summary>
    /// Description complète d'une table : colonnes annotées, chemins d'accès à clé,
    /// clé d'identification retenue et relations. (EF-08 à EF-10)
    /// </summary>
    public sealed class StructureTable
    {
        public StructureTable(TableInfo table, IList<ColonneInfo> colonnes, IList<CleInfo> cles,
            CleInfo cleIdentification, IList<RelationInfo> relationsVersParents, IList<RelationInfo> relationsDepuisEnfants,
            IList<string> avertissements)
        {
            Avertissements = new ReadOnlyCollection<string>(avertissements ?? new List<string>());
            Table = table;
            Colonnes = new ReadOnlyCollection<ColonneInfo>(colonnes ?? new List<ColonneInfo>());
            Cles = new ReadOnlyCollection<CleInfo>(cles ?? new List<CleInfo>());
            CleIdentification = cleIdentification;
            RelationsVersParents = new ReadOnlyCollection<RelationInfo>(relationsVersParents ?? new List<RelationInfo>());
            RelationsDepuisEnfants = new ReadOnlyCollection<RelationInfo>(relationsDepuisEnfants ?? new List<RelationInfo>());
        }

        public TableInfo Table { get; }

        /// <summary>Colonnes, annotées du rang de clé et de la référence étrangère.</summary>
        public IReadOnlyList<ColonneInfo> Colonnes { get; }

        /// <summary>Tous les chemins d'accès à clé, du plus au moins pertinent.</summary>
        public IReadOnlyList<CleInfo> Cles { get; }

        /// <summary>
        /// Clé unique retenue pour cibler une ligne (UPDATE / DELETE ligne à ligne),
        /// ou null : il faudra alors recourir au numéro relatif d'enregistrement RRN().
        /// </summary>
        public CleInfo CleIdentification { get; }

        /// <summary>Clés étrangères de cette table vers ses tables parentes.</summary>
        public IReadOnlyList<RelationInfo> RelationsVersParents { get; }

        /// <summary>Clés étrangères d'autres tables qui référencent cette table.</summary>
        public IReadOnlyList<RelationInfo> RelationsDepuisEnfants { get; }

        /// <summary>Lectures du catalogue qui ont échoué (droits, vue absente) : la description est alors partielle.</summary>
        public IReadOnlyList<string> Avertissements { get; }
    }
}

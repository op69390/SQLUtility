using System;

namespace SQLUtility.Core.Catalogue
{
    /// <summary>
    /// Index conseillé par l'optimiseur de DB2 for i (QSYS2.SYSIXADV) : à défaut d'EXPLAIN,
    /// c'est la trace des chemins d'accès qui ont manqué aux requêtes réellement exécutées. (OPT-11)
    /// </summary>
    public sealed class IndexConseille
    {
        public string Schema { get; set; }
        public string Table { get; set; }

        /// <summary>Colonnes de clé conseillées, telles que fournies (« COL1, COL2 DESC »).</summary>
        public string Colonnes { get; set; }

        /// <summary>RADIX (index classique) ou EVI (index vectoriel encodé).</summary>
        public string TypeIndex { get; set; }

        /// <summary>Nombre de fois où l'optimiseur a conseillé cet index.</summary>
        public long NombreConseils { get; set; }

        public DateTime? DernierConseil { get; set; }

        /// <summary>Durée de création estimée, en secondes.</summary>
        public long? DureeCreationSecondes { get; set; }

        /// <summary>Raison du conseil (sélection de lignes, tri / regroupement...).</summary>
        public string Raison { get; set; }

        public bool EstEvi
        {
            get { return (TypeIndex ?? string.Empty).Trim().StartsWith("EVI", StringComparison.OrdinalIgnoreCase); }
        }

        /// <summary>
        /// Instruction de création proposée, à faire valider avant exécution (DDL sur la base
        /// de production : place disque, maintenance de l'index à chaque mise à jour du fichier).
        /// </summary>
        public string InstructionCreation(int numero)
        {
            string nom = Table + "_IXC" + numero;
            if (nom.Length > 128) nom = nom.Substring(0, 128);
            return (EstEvi ? "CREATE ENCODED VECTOR INDEX " : "CREATE INDEX ") + Schema + "." + nom
                + " ON " + Schema + "." + Table + " (" + (Colonnes ?? string.Empty).Trim() + ")";
        }
    }
}

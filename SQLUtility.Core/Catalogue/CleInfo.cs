using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SQLUtility.Core.Catalogue
{
    /// <summary>Chemin d'accès à clé d'une table : contrainte, clé DDS ou index. (EF-09)</summary>
    public sealed class CleInfo
    {
        public CleInfo(string schema, string nom, OrigineCle origine, bool estUnique,
            bool estPartielle, bool estComplete, IList<string> colonnes)
        {
            Schema = schema;
            Nom = nom;
            Origine = origine;
            EstUnique = estUnique;
            EstPartielle = estPartielle;
            EstComplete = estComplete;
            Colonnes = new ReadOnlyCollection<string>(colonnes ?? new List<string>());
        }

        /// <summary>Schéma de l'index, du logique ou de la contrainte.</summary>
        public string Schema { get; }

        /// <summary>Nom de l'index, du logique ou de la contrainte.</summary>
        public string Nom { get; }

        public OrigineCle Origine { get; }

        /// <summary>Vrai si deux lignes ne peuvent pas avoir la même valeur de clé.</summary>
        public bool EstUnique { get; }

        /// <summary>Vrai si le chemin d'accès omet des lignes (select/omit, WHERE) ou a une clé dérivée.</summary>
        public bool EstPartielle { get; }

        /// <summary>Faux si la liste des colonnes a été tronquée par le catalogue.</summary>
        public bool EstComplete { get; }

        /// <summary>Colonnes de la clé, dans l'ordre de la clé (noms de la table).</summary>
        public IReadOnlyList<string> Colonnes { get; }

        /// <summary>Vrai si la clé désigne au plus une ligne et couvre toute la table.</summary>
        public bool PeutIdentifierLigne
        {
            get { return EstUnique && !EstPartielle && EstComplete && Colonnes.Count > 0; }
        }

        /// <summary>Libellé court de l'origine, pour l'affichage.</summary>
        public string LibelleOrigine
        {
            get
            {
                switch (Origine)
                {
                    case OrigineCle.ClePrimaire: return "Clé primaire";
                    case OrigineCle.ContrainteUnique: return "Contrainte unique";
                    case OrigineCle.FichierPhysique: return "Clé du physique";
                    case OrigineCle.Index: return "Index";
                    case OrigineCle.FichierLogique: return "Logique";
                    default: return "Autre";
                }
            }
        }

        /// <summary>Colonnes séparées par des virgules, pour l'affichage.</summary>
        public string ListeColonnes
        {
            get { return string.Join(", ", Colonnes); }
        }

        public override string ToString()
        {
            return Nom + " (" + ListeColonnes + ")";
        }
    }
}

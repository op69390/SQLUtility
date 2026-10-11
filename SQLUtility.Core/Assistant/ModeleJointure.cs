using System;
using System.Collections.Generic;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Assistant
{
    /// <summary>Type de jointure proposé par l'assistant. (EF-44)</summary>
    public enum TypeJointure
    {
        /// <summary>INNER JOIN : seulement les lignes présentes des deux côtés.</summary>
        Interne,

        /// <summary>LEFT OUTER JOIN : toutes les lignes de gauche, même sans correspondance.</summary>
        ExterneGauche
    }

    /// <summary>D'où vient une condition de jointure, de la plus à la moins sûre. (EF-45)</summary>
    public enum OrigineJointure
    {
        /// <summary>Clé étrangère déclarée dans le catalogue.</summary>
        CleEtrangereDeclaree = 0,

        /// <summary>Toutes les colonnes d'une clé unique retrouvées, même nom et type compatible.</summary>
        CleCompleteDeduite = 1,

        /// <summary>Une partie seulement d'une clé retrouvée : des lignes peuvent être multipliées.</summary>
        ClePartielleDeduite = 2,

        /// <summary>Conditions saisies par l'utilisateur.</summary>
        Manuelle = 3
    }

    /// <summary>Table d'une requête multi-tables, avec son alias (A, B, C...).</summary>
    public sealed class TableRequete
    {
        public TableRequete(StructureTable structure, string alias)
        {
            if (structure == null) throw new ArgumentNullException(nameof(structure));
            if (string.IsNullOrEmpty(alias)) throw new ArgumentNullException(nameof(alias));
            Structure = structure;
            Alias = alias;
        }

        public StructureTable Structure { get; }

        public TableInfo Table
        {
            get { return Structure.Table; }
        }

        public string Alias { get; }

        /// <summary>Colonne de cette table par son nom (null si absente).</summary>
        public ColonneInfo Colonne(string nom)
        {
            foreach (ColonneInfo colonne in Structure.Colonnes)
                if (string.Equals(colonne.Nom, nom, StringComparison.OrdinalIgnoreCase)) return colonne;
            return null;
        }

        public override string ToString()
        {
            return Alias + " — " + Table.Schema + "." + Table.Nom;
        }
    }

    /// <summary>Égalité « colonne de la table jointe = colonne d'une table déjà présente ».</summary>
    public sealed class ConditionJointure
    {
        public ConditionJointure(ColonneInfo colonneJointe, TableRequete tableLiee, ColonneInfo colonneLiee)
        {
            ColonneJointe = colonneJointe;
            TableLiee = tableLiee;
            ColonneLiee = colonneLiee;
        }

        /// <summary>Colonne de la table qu'on joint.</summary>
        public ColonneInfo ColonneJointe { get; }

        /// <summary>Table déjà présente dans la requête.</summary>
        public TableRequete TableLiee { get; }

        public ColonneInfo ColonneLiee { get; }
    }

    /// <summary>Jointure d'une table à la requête.</summary>
    public sealed class Jointure
    {
        public Jointure(TableRequete table, TypeJointure type, OrigineJointure origine)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            Table = table;
            Type = type;
            Origine = origine;
            Conditions = new List<ConditionJointure>();
        }

        public TableRequete Table { get; }
        public TypeJointure Type { get; set; }
        public OrigineJointure Origine { get; set; }
        public List<ConditionJointure> Conditions { get; private set; }

        /// <summary>Conditions lisibles : « B.FITEM = A.ITNBR ET ... ».</summary>
        public string DescriptionConditions
        {
            get
            {
                List<string> textes = new List<string>();
                foreach (ConditionJointure condition in Conditions)
                {
                    if (condition.ColonneJointe == null || condition.TableLiee == null || condition.ColonneLiee == null) continue;
                    textes.Add(Table.Alias + "." + condition.ColonneJointe.Nom + " = " + condition.TableLiee.Alias + "." + condition.ColonneLiee.Nom);
                }
                return string.Join(" ET ", textes);
            }
        }
    }

    /// <summary>Colonne affichée d'une requête multi-tables.</summary>
    public sealed class ColonneRequete
    {
        public ColonneRequete(TableRequete table, ColonneInfo colonne)
        {
            Table = table;
            Colonne = colonne;
        }

        public TableRequete Table { get; }
        public ColonneInfo Colonne { get; }
    }

    /// <summary>Consultation sur plusieurs tables jointes (niveau 2). (EF-44)</summary>
    public sealed class RequeteJointure
    {
        public RequeteJointure(TableRequete principale)
        {
            if (principale == null) throw new ArgumentNullException(nameof(principale));
            Principale = principale;
            Jointures = new List<Jointure>();
            Colonnes = new List<ColonneRequete>();
            Criteres = new List<CritereFiltre>();
            Tris = new List<CritereTri>();
        }

        /// <summary>Table de départ (FROM).</summary>
        public TableRequete Principale { get; }

        /// <summary>Tables jointes, dans l'ordre : chacune ne peut se lier qu'aux précédentes.</summary>
        public List<Jointure> Jointures { get; private set; }

        /// <summary>Colonnes affichées ; vide = toutes les colonnes de toutes les tables.</summary>
        public List<ColonneRequete> Colonnes { get; private set; }

        /// <summary>Critères (combinés par AND) ; <see cref="CritereFiltre.Table"/> obligatoire.</summary>
        public List<CritereFiltre> Criteres { get; private set; }

        /// <summary>Tris ; <see cref="CritereTri.Table"/> obligatoire.</summary>
        public List<CritereTri> Tris { get; private set; }

        public bool Distinct { get; set; }

        /// <summary>FETCH FIRST n ROWS ONLY ; 0 = sans limite.</summary>
        public int Limite { get; set; }
    }

    /// <summary>
    /// Jointure suggérée entre une table déjà présente et une table à joindre.
    /// Les paires associent une colonne de la table présente à une colonne de la nouvelle.
    /// </summary>
    public sealed class JointureProposee
    {
        public JointureProposee(OrigineJointure origine, string description, bool auPlusUneLigne)
        {
            Origine = origine;
            Description = description;
            AuPlusUneLigne = auPlusUneLigne;
            Paires = new List<KeyValuePair<ColonneInfo, ColonneInfo>>();
        }

        public OrigineJointure Origine { get; }

        public string Description { get; }

        /// <summary>
        /// Vrai si chaque ligne de la table présente trouve au plus une ligne dans la nouvelle
        /// (jointure vers une clé unique complète) : le nombre de lignes n'est pas multiplié.
        /// </summary>
        public bool AuPlusUneLigne { get; }

        /// <summary>Paires (colonne de la table présente, colonne de la nouvelle table).</summary>
        public List<KeyValuePair<ColonneInfo, ColonneInfo>> Paires { get; private set; }

        public string LibelleOrigine
        {
            get
            {
                switch (Origine)
                {
                    case OrigineJointure.CleEtrangereDeclaree: return "Clé étrangère déclarée";
                    case OrigineJointure.CleCompleteDeduite: return "Déduite d'une clé";
                    case OrigineJointure.ClePartielleDeduite: return "Clé partielle (à vérifier)";
                    default: return "Manuelle";
                }
            }
        }

        public override string ToString()
        {
            return LibelleOrigine + " : " + Description;
        }
    }
}

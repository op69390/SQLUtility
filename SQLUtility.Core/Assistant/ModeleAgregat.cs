using System;
using System.Collections.Generic;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Dates;

namespace SQLUtility.Core.Assistant
{
    /// <summary>Rôle d'une colonne dans une requête d'agrégat (niveau 3). (EF-46)</summary>
    public enum FonctionAgregat
    {
        /// <summary>Colonne de regroupement (GROUP BY).</summary>
        Regrouper,
        Somme,
        /// <summary>COUNT(colonne) : valeurs non NULL.</summary>
        Nombre,
        NombreDistinct,
        Minimum,
        Maximum,
        Moyenne,
        /// <summary>COUNT(*) : lignes du groupe (sans colonne).</summary>
        NombreLignes
    }

    /// <summary>Regroupement d'une date par période. (EF-47)</summary>
    public enum PeriodeDate
    {
        Aucune,
        Annee,
        /// <summary>Année et mois, sous la forme AAAAMM (ex. 202610).</summary>
        Mois,
        Jour
    }

    /// <summary>Élément calculé ou regroupé d'une requête d'agrégat.</summary>
    public sealed class ElementAgregat
    {
        public ElementAgregat(TableRequete table, ColonneInfo colonne, FonctionAgregat fonction)
        {
            if (colonne == null && fonction != FonctionAgregat.NombreLignes)
                throw new ArgumentNullException(nameof(colonne));
            Table = table;
            Colonne = colonne;
            Fonction = fonction;
        }

        /// <summary>Table portant la colonne (null pour une table seule ou pour COUNT(*)).</summary>
        public TableRequete Table { get; }

        /// <summary>Colonne (null pour <see cref="FonctionAgregat.NombreLignes"/>).</summary>
        public ColonneInfo Colonne { get; }

        public FonctionAgregat Fonction { get; }

        /// <summary>Période, pour un regroupement sur une date.</summary>
        public PeriodeDate Periode { get; set; }

        /// <summary>Format de stockage d'une date Atlas numérique (période, HAVING sur MIN / MAX).</summary>
        public FormatDate? FormatDate { get; set; }

        public bool EstCalcul
        {
            get { return Fonction != FonctionAgregat.Regrouper; }
        }
    }

    /// <summary>Condition sur un calcul (HAVING), ex. SUM(QTY) &gt; 100.</summary>
    public sealed class CritereAgregat
    {
        public CritereAgregat(ElementAgregat element, OperateurFiltre operateur, string valeur, string valeur2)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            Element = element;
            Operateur = operateur;
            Valeur = valeur;
            Valeur2 = valeur2;
        }

        public ElementAgregat Element { get; }
        public OperateurFiltre Operateur { get; }
        public string Valeur { get; }
        public string Valeur2 { get; }
    }

    /// <summary>Tri sur un élément du résultat.</summary>
    public sealed class TriAgregat
    {
        public TriAgregat(ElementAgregat element, bool descendant)
        {
            Element = element;
            Descendant = descendant;
        }

        public ElementAgregat Element { get; }
        public bool Descendant { get; }
    }

    /// <summary>
    /// Requête d'agrégat (niveau 3) : tables, jointures, filtres sur les lignes (WHERE) et
    /// limite de <see cref="Base"/> ; éléments regroupés ou calculés, filtres sur les calculs
    /// (HAVING) et tris. (EF-46 / EF-47)
    /// </summary>
    public sealed class RequeteAgregat
    {
        public RequeteAgregat(RequeteJointure baseRequete)
        {
            if (baseRequete == null) throw new ArgumentNullException(nameof(baseRequete));
            Base = baseRequete;
            Elements = new List<ElementAgregat>();
            Having = new List<CritereAgregat>();
            Tris = new List<TriAgregat>();
        }

        /// <summary>Tables, jointures, critères WHERE et limite ; colonnes et tris de la base sont ignorés.</summary>
        public RequeteJointure Base { get; }

        public List<ElementAgregat> Elements { get; private set; }
        public List<CritereAgregat> Having { get; private set; }
        public List<TriAgregat> Tris { get; private set; }
    }
}

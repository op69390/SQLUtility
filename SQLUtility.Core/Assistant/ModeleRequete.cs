using System.Collections.Generic;
using System.Collections.ObjectModel;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Dates;

namespace SQLUtility.Core.Assistant
{
    /// <summary>Opérateurs de filtre proposés par l'assistant. (EF-41)</summary>
    public enum OperateurFiltre
    {
        Egal,
        Different,
        Inferieur,
        InferieurOuEgal,
        Superieur,
        SuperieurOuEgal,
        Entre,
        /// <summary>Valeurs séparées par « ; » (la virgule est le séparateur décimal).</summary>
        Dans,
        Commence,
        Contient,
        Finit,
        EstNul,
        NestPasNul
    }

    /// <summary>Critère de filtre d'une consultation : colonne, opérateur, valeur(s).</summary>
    public sealed class CritereFiltre
    {
        public ColonneInfo Colonne { get; set; }
        public OperateurFiltre Operateur { get; set; }
        public string Valeur { get; set; }

        /// <summary>Borne haute de <see cref="OperateurFiltre.Entre"/>.</summary>
        public string Valeur2 { get; set; }

        /// <summary>Format de stockage si la colonne numérique contient une date Atlas.</summary>
        public FormatDate? FormatDate { get; set; }

        /// <summary>Table de la requête portant la colonne (jointures) ; null pour une seule table.</summary>
        public TableRequete Table { get; set; }
    }

    /// <summary>Critère de tri.</summary>
    public sealed class CritereTri
    {
        public CritereTri(ColonneInfo colonne, bool descendant)
        {
            Colonne = colonne;
            Descendant = descendant;
        }

        public ColonneInfo Colonne { get; }
        public bool Descendant { get; }

        /// <summary>Table de la requête portant la colonne (jointures) ; null pour une seule table.</summary>
        public TableRequete Table { get; set; }
    }

    /// <summary>Consultation d'une table (niveau 1). (EF-41)</summary>
    public sealed class RequeteConsultation
    {
        public RequeteConsultation()
        {
            Colonnes = new List<ColonneInfo>();
            Criteres = new List<CritereFiltre>();
            Tris = new List<CritereTri>();
        }

        public TableInfo Table { get; set; }

        /// <summary>Colonnes affichées, dans l'ordre ; vide = toutes les colonnes de <see cref="ToutesColonnes"/>.</summary>
        public List<ColonneInfo> Colonnes { get; private set; }

        /// <summary>Colonnes de la table, utilisées quand aucune n'est cochée (liste explicite plutôt que SELECT *).</summary>
        public IList<ColonneInfo> ToutesColonnes { get; set; }

        /// <summary>Critères combinés par AND.</summary>
        public List<CritereFiltre> Criteres { get; private set; }

        public List<CritereTri> Tris { get; private set; }

        public bool Distinct { get; set; }

        /// <summary>Ajoute le numéro relatif d'enregistrement RRN() en première colonne.</summary>
        public bool AvecRrn { get; set; }

        /// <summary>FETCH FIRST n ROWS ONLY ; 0 = sans limite.</summary>
        public int Limite { get; set; }
    }

    /// <summary>Valeur affectée à une colonne (INSERT, UPDATE ... SET).</summary>
    public sealed class AffectationColonne
    {
        public AffectationColonne(ColonneInfo colonne, string valeur, bool estNull, FormatDate? formatDate)
        {
            Colonne = colonne;
            Valeur = valeur;
            EstNull = estNull;
            FormatDate = formatDate;
        }

        public ColonneInfo Colonne { get; }
        public string Valeur { get; }
        public bool EstNull { get; }
        public FormatDate? FormatDate { get; }
    }

    /// <summary>
    /// Désignation d'une ligne unique : par les valeurs de la clé d'identification, ou à
    /// défaut par son numéro relatif d'enregistrement RRN(). (EF-42)
    /// </summary>
    public sealed class IdentificationLigne
    {
        private IdentificationLigne(IList<AffectationColonne> valeursCle, long? rrn)
        {
            ValeursCle = new ReadOnlyCollection<AffectationColonne>(valeursCle ?? new List<AffectationColonne>());
            Rrn = rrn;
        }

        public static IdentificationLigne ParCle(IList<AffectationColonne> valeursCle)
        {
            return new IdentificationLigne(valeursCle, null);
        }

        public static IdentificationLigne ParRrn(long rrn)
        {
            return new IdentificationLigne(null, rrn);
        }

        public IReadOnlyList<AffectationColonne> ValeursCle { get; }

        /// <summary>Numéro d'enregistrement, ou null si la ligne est désignée par sa clé.</summary>
        public long? Rrn { get; }
    }

    /// <summary>Instruction générée, ou erreurs de saisie qui l'empêchent.</summary>
    public sealed class ResultatGeneration
    {
        public ResultatGeneration(string sql, IList<string> erreurs)
            : this(sql, erreurs, null)
        {
        }

        public ResultatGeneration(string sql, IList<string> erreurs, IList<string> avertissements)
            : this(sql, erreurs, avertissements, null)
        {
        }

        public ResultatGeneration(string sql, IList<string> erreurs, IList<string> avertissements, IList<ColonneInfo> colonnes)
        {
            Erreurs = new ReadOnlyCollection<string>(erreurs ?? new List<string>());
            Avertissements = new ReadOnlyCollection<string>(avertissements ?? new List<string>());
            Colonnes = new ReadOnlyCollection<ColonneInfo>(colonnes ?? new List<ColonneInfo>());
            Sql = Erreurs.Count == 0 ? sql : null;
        }

        /// <summary>
        /// Colonnes du résultat d'une consultation (noms uniques, types, libellés), pour en
        /// faire une étape réutilisable (WITH). Vide pour un ajout, une modification, une suppression.
        /// </summary>
        public IReadOnlyList<ColonneInfo> Colonnes { get; }

        /// <summary>Même résultat, avec d'autres erreurs ou avertissements (colonnes conservées).</summary>
        public ResultatGeneration Completer(IList<string> erreursEnPlus, IList<string> avertissementsEnPlus)
        {
            List<string> erreurs = new List<string>(Erreurs);
            if (erreursEnPlus != null) erreurs.AddRange(erreursEnPlus);
            List<string> avertissements = new List<string>(Avertissements);
            if (avertissementsEnPlus != null) avertissements.AddRange(avertissementsEnPlus);
            return new ResultatGeneration(Sql, erreurs, avertissements, new List<ColonneInfo>(Colonnes));
        }

        /// <summary>Points d'attention qui n'empêchent pas la génération (ex. lignes multipliées).</summary>
        public IReadOnlyList<string> Avertissements { get; }

        /// <summary>Instruction SQL, ou null si la saisie comporte des erreurs.</summary>
        public string Sql { get; }

        public IReadOnlyList<string> Erreurs { get; }

        public bool EstValide
        {
            get { return Erreurs.Count == 0; }
        }
    }
}

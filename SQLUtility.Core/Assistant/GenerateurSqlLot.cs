using System;
using System.Collections.Generic;
using System.Text;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Dates;

namespace SQLUtility.Core.Assistant
{
    /// <summary>Valeur donnée à une colonne par une modification par lot : constante ou colonne de la table source.</summary>
    public sealed class AffectationLot
    {
        /// <summary>Constante (ou NULL).</summary>
        public AffectationLot(ColonneInfo cible, string valeur, bool estNull, FormatDate? formatDate)
        {
            if (cible == null) throw new ArgumentNullException(nameof(cible));
            Cible = cible;
            Valeur = valeur;
            EstNull = estNull;
            FormatDate = formatDate;
        }

        /// <summary>Valeur prise dans la table source (sous-requête corrélée).</summary>
        public AffectationLot(ColonneInfo cible, ColonneInfo source)
        {
            if (cible == null) throw new ArgumentNullException(nameof(cible));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Cible = cible;
            Source = source;
        }

        public ColonneInfo Cible { get; }
        public string Valeur { get; }
        public bool EstNull { get; }
        public FormatDate? FormatDate { get; }

        /// <summary>Colonne de la table source, ou null pour une constante.</summary>
        public ColonneInfo Source { get; }
    }

    /// <summary>
    /// Table source d'une modification par lot : chaque ligne de A prend ses nouvelles valeurs
    /// dans la ligne de la source qui lui correspond. Les conditions relient une colonne de la
    /// source (<see cref="ConditionJointure.ColonneJointe"/>) à une colonne de A.
    /// </summary>
    public sealed class SourceMiseAJour
    {
        public const string AliasSource = "SRC";

        public SourceMiseAJour(StructureTable structure)
        {
            if (structure == null) throw new ArgumentNullException(nameof(structure));
            Table = new TableRequete(structure, AliasSource);
            Conditions = new List<ConditionJointure>();
        }

        public TableRequete Table { get; }
        public List<ConditionJointure> Conditions { get; private set; }
    }

    /// <summary>Alimentation d'une colonne de la table cible d'un INSERT … SELECT.</summary>
    public sealed class CorrespondanceInsert
    {
        /// <summary>Depuis une colonne de la requête source.</summary>
        public CorrespondanceInsert(ColonneInfo cible, ColonneRequete source)
        {
            if (cible == null) throw new ArgumentNullException(nameof(cible));
            if (source == null) throw new ArgumentNullException(nameof(source));
            Cible = cible;
            Source = source;
        }

        /// <summary>Constante (ou NULL) identique pour toutes les lignes copiées.</summary>
        public CorrespondanceInsert(ColonneInfo cible, string valeur, bool estNull, FormatDate? formatDate)
        {
            if (cible == null) throw new ArgumentNullException(nameof(cible));
            Cible = cible;
            Valeur = valeur;
            EstNull = estNull;
            FormatDate = formatDate;
        }

        public ColonneInfo Cible { get; }
        public ColonneRequete Source { get; }
        public string Valeur { get; }
        public bool EstNull { get; }
        public FormatDate? FormatDate { get; }
    }

    /// <summary>
    /// Traitements par lot du niveau 4 (EF-49) : suppression et modification des lignes
    /// filtrées de la table A, éventuellement à partir d'une autre table, et copie des lignes
    /// d'une consultation vers une table (INSERT … SELECT). Le SQL passe ensuite par le
    /// comptage préalable, l'analyse de risque et les confirmations de l'exécution.
    /// </summary>
    public static class GenerateurSqlLot
    {
        private const string Retrait = "       ";

        public static ResultatGeneration ConstruireDelete(RequeteJointure lignes)
        {
            if (lignes == null) throw new ArgumentNullException(nameof(lignes));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();
            RefuserJointures(lignes, "Supprimer", erreurs);

            bool alias = lignes.AvecAlias;
            List<TableRequete> introduites = new List<TableRequete> { lignes.Principale };
            List<string> conditions = GenerateurSqlJointure.ConstruireConditionsWhere(lignes, alias, introduites,
                new HashSet<TableRequete>(), erreurs, avertissements);

            StringBuilder sql = new StringBuilder();
            sql.Append("DELETE FROM ").Append(NomTable(lignes.Principale, alias));
            GenerateurSqlJointure.AjouterWhere(sql, conditions);
            if (conditions.Count == 0)
                avertissements.Add("Aucun filtre : TOUTES les lignes de " + lignes.Principale.Table.Nom + " seront supprimées.");
            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        public static ResultatGeneration ConstruireUpdate(RequeteJointure lignes, IList<AffectationLot> affectations, SourceMiseAJour source)
        {
            if (lignes == null) throw new ArgumentNullException(nameof(lignes));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();
            RefuserJointures(lignes, "Modifier", erreurs);

            bool avecSource = false;
            foreach (AffectationLot affectation in affectations ?? new List<AffectationLot>())
                if (affectation.Source != null) avecSource = true;
            bool alias = lignes.AvecAlias || avecSource;
            TableRequete a = lignes.Principale;
            List<TableRequete> introduites = new List<TableRequete> { a };

            // Lien avec la source : sous-requête corrélée, qui doit désigner au plus une ligne.
            string lienSource = null;
            if (avecSource)
            {
                if (source == null)
                {
                    erreurs.Add("Définissez la table source des valeurs.");
                }
                else
                {
                    lienSource = LienSource(source, a, erreurs);
                    if (lienSource != null && !CouvreCleUnique(source))
                        avertissements.Add("Les conditions vers " + source.Table.Table.Nom + " ne couvrent pas une clé unique : si plusieurs lignes "
                            + "correspondent, l'exécution échouera (SQL0811) sans rien modifier.");
                }
            }

            List<string> affectationsSql = new List<string>();
            foreach (AffectationLot affectation in affectations ?? new List<AffectationLot>())
            {
                string nom = GenerateurSqlSimple.NomSql(affectation.Cible.Nom);
                if (affectation.Source == null)
                {
                    string litteral = Litteral(affectation.Cible, affectation.Valeur, affectation.EstNull, affectation.FormatDate, erreurs);
                    if (litteral != null) affectationsSql.Add(nom + " = " + litteral);
                    continue;
                }
                ControlerCompatibilite(affectation.Source, affectation.Cible, erreurs, avertissements);
                if (source != null && lienSource != null)
                    affectationsSql.Add(nom + " = (SELECT " + GenerateurSqlJointure.Qualifier(source.Table, affectation.Source.Nom)
                        + " FROM " + GenerateurSqlSimple.NomTable(source.Table.Table) + " " + source.Table.Alias + " WHERE " + lienSource + ")");
            }
            if (affectationsSql.Count == 0 && erreurs.Count == 0) erreurs.Add("Indiquez au moins une colonne à modifier.");

            List<string> conditions = GenerateurSqlJointure.ConstruireConditionsWhere(lignes, alias, introduites,
                new HashSet<TableRequete>(), erreurs, avertissements);
            // Sans correspondance dans la source, la sous-requête rendrait NULL : on écarte ces lignes.
            if (lienSource != null)
                conditions.Add("EXISTS (SELECT 1 FROM " + GenerateurSqlSimple.NomTable(source.Table.Table) + " " + source.Table.Alias + " WHERE " + lienSource + ")");

            StringBuilder sql = new StringBuilder();
            sql.Append("UPDATE ").Append(NomTable(a, alias)).AppendLine();
            sql.Append("SET    ").Append(string.Join("," + Environment.NewLine + Retrait, affectationsSql));
            GenerateurSqlJointure.AjouterWhere(sql, conditions);
            if (conditions.Count == 0)
                avertissements.Add("Aucun filtre : TOUTES les lignes de " + a.Table.Nom + " seront modifiées.");
            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        public static ResultatGeneration ConstruireInsertSelect(TableInfo cible, IList<CorrespondanceInsert> correspondances, RequeteJointure source)
        {
            if (cible == null) throw new ArgumentNullException(nameof(cible));
            if (source == null) throw new ArgumentNullException(nameof(source));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();
            List<TableRequete> introduites = new List<TableRequete>();
            HashSet<TableRequete> externes = new HashSet<TableRequete>();
            bool alias = source.AvecAlias;
            string from = GenerateurSqlJointure.ConstruireFrom(source, alias, introduites, externes, erreurs, avertissements);

            List<string> noms = new List<string>();
            List<string> valeurs = new List<string>();
            foreach (CorrespondanceInsert correspondance in correspondances ?? new List<CorrespondanceInsert>())
            {
                string valeur;
                if (correspondance.Source != null)
                {
                    if (!introduites.Contains(correspondance.Source.Table))
                    {
                        erreurs.Add(correspondance.Cible.Nom + " : la colonne source vient d'une table absente de la requête.");
                        continue;
                    }
                    ControlerCompatibilite(correspondance.Source.Colonne, correspondance.Cible, erreurs, avertissements);
                    valeur = GenerateurSqlJointure.Expression(correspondance.Source.Table, correspondance.Source.Colonne.Nom, alias);
                }
                else
                {
                    valeur = Litteral(correspondance.Cible, correspondance.Valeur, correspondance.EstNull, correspondance.FormatDate, erreurs);
                    if (valeur == null) continue;
                }
                noms.Add(GenerateurSqlSimple.NomSql(correspondance.Cible.Nom));
                valeurs.Add(valeur);
            }
            if (noms.Count == 0 && erreurs.Count == 0) erreurs.Add("Associez au moins une colonne de la table cible.");
            if (source.Limite > 0)
                avertissements.Add("La limite de lignes de la consultation n'est pas appliquée à la copie : toutes les lignes filtrées seront copiées.");

            StringBuilder sql = new StringBuilder();
            sql.Append("INSERT INTO ").Append(GenerateurSqlSimple.NomTable(cible)).AppendLine();
            sql.Append("       (").Append(string.Join(", ", noms)).AppendLine(")");
            sql.Append(source.Distinct ? "SELECT DISTINCT " : "SELECT ").Append(string.Join(", ", valeurs)).AppendLine();
            sql.Append(from);
            GenerateurSqlJointure.AjouterWhere(sql,
                GenerateurSqlJointure.ConstruireConditionsWhere(source, alias, introduites, externes, erreurs, avertissements));
            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        /// <summary>
        /// Associe automatiquement les colonnes de la cible aux colonnes de même nom et de type
        /// compatible de la consultation (les noms de zones Atlas sont partagés entre fichiers).
        /// </summary>
        public static Dictionary<ColonneInfo, ColonneRequete> AssocierParNom(IEnumerable<ColonneInfo> cible, IEnumerable<ColonneRequete> sources)
        {
            Dictionary<ColonneInfo, ColonneRequete> associations = new Dictionary<ColonneInfo, ColonneRequete>();
            List<ColonneRequete> liste = new List<ColonneRequete>(sources);
            foreach (ColonneInfo colonne in cible)
            {
                foreach (ColonneRequete source in liste)
                {
                    if (string.Equals(source.Colonne.Nom, colonne.Nom, StringComparison.OrdinalIgnoreCase)
                        && DetecteurJointures.TypesCompatibles(source.Colonne, colonne))
                    {
                        associations[colonne] = source;
                        break;
                    }
                }
            }
            return associations;
        }

        // ------------------------------------------------------------------ Outils

        private static void RefuserJointures(RequeteJointure lignes, string verbe, List<string> erreurs)
        {
            if (lignes.Jointures.Count > 0)
                erreurs.Add(verbe + " ne porte que sur la table A : retirez les tables jointes et utilisez plutôt une condition d'existence.");
        }

        private static string NomTable(TableRequete table, bool alias)
        {
            return GenerateurSqlSimple.NomTable(table.Table) + (alias ? " " + table.Alias : string.Empty);
        }

        private static string LienSource(SourceMiseAJour source, TableRequete a, List<string> erreurs)
        {
            List<string> conditions = new List<string>();
            foreach (ConditionJointure condition in source.Conditions)
            {
                if (condition.ColonneJointe == null || condition.ColonneLiee == null) continue;
                if (!DetecteurJointures.TypesCompatibles(condition.ColonneJointe, condition.ColonneLiee))
                    erreurs.Add(condition.ColonneJointe.Nom + " et " + condition.ColonneLiee.Nom + " ne sont pas de types comparables.");
                conditions.Add(GenerateurSqlJointure.Qualifier(source.Table, condition.ColonneJointe.Nom) + " = "
                    + GenerateurSqlJointure.Qualifier(a, condition.ColonneLiee.Nom));
            }
            if (conditions.Count == 0)
            {
                erreurs.Add("Reliez la table source à A (au moins une condition).");
                return null;
            }
            return string.Join(" AND ", conditions);
        }

        /// <summary>Vrai si les colonnes reliées de la source contiennent une clé unique complète.</summary>
        private static bool CouvreCleUnique(SourceMiseAJour source)
        {
            HashSet<string> reliees = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ConditionJointure condition in source.Conditions)
                if (condition.ColonneJointe != null) reliees.Add(condition.ColonneJointe.Nom);
            foreach (CleInfo cle in source.Table.Structure.Cles)
            {
                if (!cle.PeutIdentifierLigne) continue;
                bool couverte = true;
                foreach (string nom in cle.Colonnes)
                    if (!reliees.Contains(nom)) { couverte = false; break; }
                if (couverte) return true;
            }
            return false;
        }

        private static void ControlerCompatibilite(ColonneInfo source, ColonneInfo cible, List<string> erreurs, List<string> avertissements)
        {
            if (!DetecteurJointures.TypesCompatibles(source, cible))
            {
                erreurs.Add(cible.Nom + " (" + cible.TypeDonnee + ") ne peut pas recevoir " + source.Nom + " (" + source.TypeDonnee + ").");
                return;
            }
            CategorieType categorie = TypesSql.Categorie(cible.TypeDonnee);
            if (categorie == CategorieType.Texte && source.Longueur > cible.Longueur && cible.Longueur > 0)
                avertissements.Add(source.Nom + " (" + source.Longueur + " car.) est plus long que " + cible.Nom + " (" + cible.Longueur
                    + ") : une valeur trop longue fera échouer l'exécution.");
            if (categorie == CategorieType.Numerique && source.Longueur - source.Echelle > cible.Longueur - cible.Echelle
                && cible.Longueur > 0 && source.Longueur > 0)
                avertissements.Add(source.Nom + " peut dépasser la capacité de " + cible.Nom + " : une valeur trop grande fera échouer l'exécution.");
        }

        private static string Litteral(ColonneInfo cible, string valeur, bool estNull, FormatDate? formatDate, List<string> erreurs)
        {
            if (estNull)
            {
                if (cible.AccepteNull) return "NULL";
                erreurs.Add(cible.Nom + " n'accepte pas la valeur NULL.");
                return null;
            }
            string litteral;
            string erreur;
            if (FormateurLitteral.TryFormater(cible, valeur, formatDate, out litteral, out erreur)) return litteral;
            erreurs.Add(erreur);
            return null;
        }
    }
}

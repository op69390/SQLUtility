using System;
using System.Collections.Generic;
using System.Text;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Dates;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Génère les requêtes d'agrégat du niveau 3 : GROUP BY, SUM / COUNT / MIN / MAX / AVG,
    /// HAVING, regroupement des dates par année, mois ou jour (dates Atlas comprises). (EF-46 / EF-47)
    /// Sur une table seule, les colonnes restent non qualifiées ; avec des jointures, elles
    /// sont qualifiées par alias. Le GROUP BY et le HAVING répètent les expressions (DB2
    /// n'y accepte pas les noms de résultat) ; l'ORDER BY utilise les noms de résultat.
    /// </summary>
    public static class GenerateurSqlAgregat
    {
        private const string Retrait = "       ";

        public static ResultatGeneration ConstruireSelect(RequeteAgregat requete)
        {
            if (requete == null) throw new ArgumentNullException(nameof(requete));
            List<string> erreurs = new List<string>();
            List<string> avertissements = new List<string>();
            List<TableRequete> introduites = new List<TableRequete>();
            HashSet<TableRequete> externes = new HashSet<TableRequete>();
            RequeteJointure baseRequete = requete.Base;
            bool qualifier = baseRequete.AvecAlias;

            string from = GenerateurSqlJointure.ConstruireFrom(baseRequete, qualifier, introduites, externes, erreurs, avertissements);
            if (requete.Elements.Count == 0)
                return new ResultatGeneration(null, new[] { "Choisissez au moins une colonne à regrouper ou à calculer." });

            // Éléments du résultat : expression, nom de résultat unique, libellé.
            List<string> selection = new List<string>();
            List<string> libelles = new List<string>();
            List<string> regroupements = new List<string>();
            Dictionary<ElementAgregat, string> noms = new Dictionary<ElementAgregat, string>();
            HashSet<string> nomsPris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ElementAgregat element in requete.Elements)
            {
                string expression = Expression(element, baseRequete, qualifier, introduites, erreurs);
                if (expression == null) continue;

                string nom = NomUnique(NomResultat(element), nomsPris);
                noms[element] = nom;
                bool colonneBrute = element.Fonction == FonctionAgregat.Regrouper && element.Periode == PeriodeDate.Aucune;
                // Une colonne regroupée telle quelle garde son nom, sauf homonyme d'une autre table.
                selection.Add(colonneBrute && string.Equals(nom, element.Colonne.Nom, StringComparison.OrdinalIgnoreCase)
                    ? expression : expression + " AS " + GenerateurSqlSimple.NomSql(nom));
                libelles.Add(LibelleElement(element));
                if (element.Fonction == FonctionAgregat.Regrouper) regroupements.Add(expression);
            }

            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT ");
            for (int i = 0; i < selection.Count; i++)
            {
                if (i > 0) sql.Append(Retrait);
                sql.Append(selection[i]);
                if (i < selection.Count - 1) sql.Append(",");
                if (!string.IsNullOrEmpty(libelles[i])) sql.Append("  -- ").Append(libelles[i]);
                sql.AppendLine();
            }
            sql.Append(from);
            GenerateurSqlJointure.AjouterWhere(sql,
                GenerateurSqlJointure.ConstruireConditionsWhere(baseRequete, qualifier, introduites, externes, erreurs, avertissements));

            if (regroupements.Count > 0) sql.AppendLine().Append("GROUP BY ").Append(string.Join(", ", regroupements));

            List<string> having = new List<string>();
            foreach (CritereAgregat critere in requete.Having)
            {
                string condition = ConditionHaving(critere, baseRequete, qualifier, introduites, erreurs);
                if (condition != null) having.Add(condition);
            }
            for (int i = 0; i < having.Count; i++)
                sql.AppendLine().Append(i == 0 ? "HAVING " : "  AND  ").Append(having[i]);

            List<string> tris = new List<string>();
            foreach (TriAgregat tri in requete.Tris)
            {
                string nom;
                if (tri.Element == null || !noms.TryGetValue(tri.Element, out nom))
                {
                    erreurs.Add("Tri impossible : on ne peut trier que sur une colonne regroupée ou calculée du résultat.");
                    continue;
                }
                tris.Add(GenerateurSqlSimple.NomSql(nom) + (tri.Descendant ? " DESC" : string.Empty));
            }
            if (tris.Count > 0) sql.AppendLine().Append("ORDER BY ").Append(string.Join(", ", tris));
            GenerateurSqlJointure.AjouterLimite(sql, baseRequete.Limite);

            return new ResultatGeneration(sql.ToString(), erreurs, avertissements);
        }

        // ------------------------------------------------------------------ Expressions

        /// <summary>Expression SQL de l'élément (agrégat appliqué), ou null en cas d'erreur.</summary>
        internal static string Expression(ElementAgregat element, RequeteJointure baseRequete, bool qualifier,
            List<TableRequete> introduites, List<string> erreurs)
        {
            if (element.Fonction == FonctionAgregat.NombreLignes) return "COUNT(*)";

            TableRequete table = element.Table ?? baseRequete.Principale;
            if (!introduites.Contains(table))
            {
                erreurs.Add(element.Colonne.Nom + " : table absente de la requête.");
                return null;
            }
            string colonne = GenerateurSqlJointure.Expression(table, element.Colonne.Nom, qualifier);
            CategorieType categorie = TypesSql.Categorie(element.Colonne.TypeDonnee);
            if (categorie == CategorieType.NonPrisEnCharge)
            {
                erreurs.Add(element.Colonne.Nom + " : type " + element.Colonne.TypeDonnee + " non pris en charge.");
                return null;
            }
            if (element.Periode != PeriodeDate.Aucune && element.Fonction != FonctionAgregat.Regrouper)
            {
                erreurs.Add(element.Colonne.Nom + " : le regroupement par période ne s'applique qu'à une colonne regroupée.");
                return null;
            }

            switch (element.Fonction)
            {
                case FonctionAgregat.Regrouper:
                    return element.Periode == PeriodeDate.Aucune ? colonne : ExpressionPeriode(element, colonne, categorie, erreurs);
                case FonctionAgregat.Somme:
                case FonctionAgregat.Moyenne:
                    if (categorie != CategorieType.Numerique)
                    {
                        erreurs.Add(element.Colonne.Nom + " : somme et moyenne ne s'appliquent qu'aux nombres.");
                        return null;
                    }
                    if (element.Fonction == FonctionAgregat.Somme) return "SUM(" + colonne + ")";
                    // La moyenne d'entiers serait tronquée : on la calcule en décimal.
                    return EstEntier(element.Colonne) ? "AVG(DECIMAL(" + colonne + ", 31, 2))" : "AVG(" + colonne + ")";
                case FonctionAgregat.Nombre: return "COUNT(" + colonne + ")";
                case FonctionAgregat.NombreDistinct: return "COUNT(DISTINCT " + colonne + ")";
                case FonctionAgregat.Minimum: return "MIN(" + colonne + ")";
                case FonctionAgregat.Maximum: return "MAX(" + colonne + ")";
                default: return null;
            }
        }

        /// <summary>
        /// YEAR, YEAR*100+MONTH ou la date elle-même. Une date Atlas numérique est d'abord
        /// décodée en DATE ; la valeur 0 (« pas de date ») donne NULL au lieu d'une erreur.
        /// </summary>
        private static string ExpressionPeriode(ElementAgregat element, string colonne, CategorieType categorie, List<string> erreurs)
        {
            string date;
            switch (categorie)
            {
                case CategorieType.Date:
                    date = colonne;
                    break;
                case CategorieType.Horodatage:
                    date = "DATE(" + colonne + ")";
                    break;
                case CategorieType.Numerique:
                    if (element.Colonne.Echelle > 0)
                    {
                        // Un nombre à décimales n'encode jamais une date Atlas.
                        erreurs.Add(element.Colonne.Nom + " : le regroupement par période ne s'applique qu'à une date.");
                        return null;
                    }
                    if (!element.FormatDate.HasValue)
                    {
                        erreurs.Add(element.Colonne.Nom + " : indiquez le format de la date Atlas pour la regrouper par période.");
                        return null;
                    }
                    string calcul = PeriodeArithmetique(colonne, element.FormatDate.Value, element.Periode);
                    if (calcul != null) return calcul;
                    date = "CASE WHEN " + colonne + " = 0 THEN NULL ELSE " + GenerateurExpressionDate.Generer(colonne, element.FormatDate.Value) + " END";
                    break;
                default:
                    erreurs.Add(element.Colonne.Nom + " : le regroupement par période ne s'applique qu'à une date.");
                    return null;
            }

            switch (element.Periode)
            {
                case PeriodeDate.Annee: return "YEAR(" + date + ")";
                case PeriodeDate.Mois: return "YEAR(" + date + ") * 100 + MONTH(" + date + ")";
                default: return categorie == CategorieType.Date ? date : "(" + date + ")";
            }
        }

        /// <summary>
        /// Année ou mois d'une date CYYMMDD ou AAAAMMJJ par simple calcul, sans décodage en DATE :
        /// plus lisible, et bien plus léger sur un gros fichier Atlas. Ex. CYYMMDD 1261015 →
        /// année 1900 + INTEGER(1261015 / 10000) = 2026, mois 190000 + INTEGER(1261015 / 100) = 202610.
        /// Null pour les autres formats ou pour le jour (décodage complet).
        /// </summary>
        private static string PeriodeArithmetique(string colonne, FormatDate format, PeriodeDate periode)
        {
            if (periode != PeriodeDate.Annee && periode != PeriodeDate.Mois) return null;
            string diviseur = periode == PeriodeDate.Annee ? "10000" : "100";
            string calcul;
            switch (format)
            {
                case FormatDate.Cymd:
                    calcul = (periode == PeriodeDate.Annee ? "1900" : "190000") + " + INTEGER(" + colonne + " / " + diviseur + ")";
                    break;
                case FormatDate.Iso:
                    calcul = "INTEGER(" + colonne + " / " + diviseur + ")";
                    break;
                default:
                    return null;
            }
            // 0 = « pas de date » dans Atlas : NULL plutôt qu'une fausse année 1900.
            return "CASE WHEN " + colonne + " = 0 THEN NULL ELSE " + calcul + " END";
        }

        private static string ConditionHaving(CritereAgregat critere, RequeteJointure baseRequete, bool qualifier,
            List<TableRequete> introduites, List<string> erreurs)
        {
            ElementAgregat element = critere.Element;
            if (!element.EstCalcul)
            {
                erreurs.Add("Un filtre sur une colonne regroupée se fait sur les lignes (WHERE), pas sur les calculs.");
                return null;
            }
            switch (critere.Operateur)
            {
                case OperateurFiltre.Commence:
                case OperateurFiltre.Contient:
                case OperateurFiltre.Finit:
                case OperateurFiltre.Dans:
                    erreurs.Add("Filtre sur un calcul : opérateurs de comparaison, « entre » ou NULL seulement.");
                    return null;
            }

            string expression = Expression(element, baseRequete, qualifier, introduites, erreurs);
            if (expression == null) return null;

            // Type de la valeur comparée : entier pour un comptage, décimal pour une somme ou
            // une moyenne, type de la colonne (date Atlas comprise) pour un minimum ou un maximum.
            CritereFiltre filtre = new CritereFiltre();
            filtre.Operateur = critere.Operateur;
            filtre.Valeur = critere.Valeur;
            filtre.Valeur2 = critere.Valeur2;
            switch (element.Fonction)
            {
                case FonctionAgregat.Nombre:
                case FonctionAgregat.NombreDistinct:
                case FonctionAgregat.NombreLignes:
                    filtre.Colonne = new ColonneInfo { Nom = NomResultat(element), TypeDonnee = "BIGINT" };
                    break;
                case FonctionAgregat.Somme:
                case FonctionAgregat.Moyenne:
                    filtre.Colonne = new ColonneInfo { Nom = NomResultat(element), TypeDonnee = "DECIMAL", Longueur = 31, Echelle = 10 };
                    break;
                default:
                    filtre.Colonne = element.Colonne;
                    filtre.FormatDate = element.FormatDate;
                    break;
            }
            return GenerateurSqlSimple.ConstruireCondition(filtre, expression, erreurs);
        }

        // ------------------------------------------------------------------ Noms et libellés

        /// <summary>Nom de résultat proposé : ITNBR, ANNEE_DTDUE, SOMME_QTY, NB_LIGNES...</summary>
        public static string NomResultat(ElementAgregat element)
        {
            if (element.Fonction == FonctionAgregat.NombreLignes) return "NB_LIGNES";
            string nom = element.Colonne.Nom;
            switch (element.Fonction)
            {
                case FonctionAgregat.Regrouper:
                    switch (element.Periode)
                    {
                        case PeriodeDate.Annee: return "ANNEE_" + nom;
                        case PeriodeDate.Mois: return "MOIS_" + nom;
                        case PeriodeDate.Jour: return "JOUR_" + nom;
                        default: return nom;
                    }
                case FonctionAgregat.Somme: return "SOMME_" + nom;
                case FonctionAgregat.Nombre: return "NB_" + nom;
                case FonctionAgregat.NombreDistinct: return "NBDIST_" + nom;
                case FonctionAgregat.Minimum: return "MIN_" + nom;
                case FonctionAgregat.Maximum: return "MAX_" + nom;
                default: return "MOY_" + nom;
            }
        }

        private static string LibelleElement(ElementAgregat element)
        {
            if (element.Fonction == FonctionAgregat.NombreLignes) return "Nombre de lignes";
            string libelle = GenerateurSqlJointure.Commentaire(element.Colonne.Libelle) ?? element.Colonne.Nom;
            switch (element.Fonction)
            {
                case FonctionAgregat.Regrouper:
                    switch (element.Periode)
                    {
                        case PeriodeDate.Annee: return libelle + " (année)";
                        case PeriodeDate.Mois: return libelle + " (mois AAAAMM)";
                        case PeriodeDate.Jour: return libelle + " (jour)";
                        default: return libelle;
                    }
                case FonctionAgregat.Somme: return "Somme : " + libelle;
                case FonctionAgregat.Nombre: return "Nombre : " + libelle;
                case FonctionAgregat.NombreDistinct: return "Nombre distinct : " + libelle;
                case FonctionAgregat.Minimum: return "Minimum : " + libelle;
                case FonctionAgregat.Maximum: return "Maximum : " + libelle;
                default: return "Moyenne : " + libelle;
            }
        }

        private static string NomUnique(string nom, HashSet<string> pris)
        {
            string candidat = nom;
            for (int n = 2; !pris.Add(candidat); n++) candidat = nom + "_" + n;
            return candidat;
        }

        private static bool EstEntier(ColonneInfo colonne)
        {
            string type = (colonne.TypeDonnee ?? string.Empty).Trim().ToUpperInvariant();
            return type == "SMALLINT" || type == "INTEGER" || type == "INT" || type == "BIGINT";
        }
    }
}

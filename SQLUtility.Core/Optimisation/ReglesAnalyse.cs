using System;
using System.Collections.Generic;
using SQLUtility.Core.Assistant;
using SQLUtility.Core.Edition;
using SQLUtility.Core.Execution;

namespace SQLUtility.Core.Optimisation
{
    /// <summary>
    /// Socle des règles OPT-02 à OPT-10 : chaque règle examine une instruction à la fois,
    /// sur les jetons situés par <see cref="AnalyseLexicale"/> (chaînes et commentaires ignorés).
    /// </summary>
    public abstract class RegleInstruction : IRegleAnalyse
    {
        public abstract string Code { get; }
        public abstract string Libelle { get; }
        public abstract Severite SeveriteParDefaut { get; }

        public IEnumerable<ResultatAnalyse> Analyser(string sql)
        {
            List<ResultatAnalyse> resultats = new List<ResultatAnalyse>();
            if (string.IsNullOrWhiteSpace(sql)) return resultats;
            List<JetonAnalyse> jetons = AnalyseLexicale.Jetons(sql);
            foreach (Plage plage in AnalyseLexicale.Instructions(jetons))
                AnalyserInstruction(sql, jetons, plage, resultats);
            return resultats;
        }

        protected abstract void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats);

        protected ResultatAnalyse Signaler(List<ResultatAnalyse> resultats, int debut, int fin, string message, string suggestion)
        {
            ResultatAnalyse resultat = new ResultatAnalyse(Code, SeveriteParDefaut, message, suggestion);
            resultat.Position = debut;
            resultat.Longueur = Math.Max(0, fin - debut);
            resultats.Add(resultat);
            return resultat;
        }

        /// <summary>Texte de l'instruction (du premier au dernier jeton).</summary>
        protected static string Texte(string sql, List<JetonAnalyse> jetons, Plage plage)
        {
            int debut = jetons[plage.Debut].Position;
            return sql.Substring(debut, jetons[plage.Fin - 1].Fin - debut);
        }

        /// <summary>Vrai dans une condition : WHERE, ON (jointure) — HAVING porte sur des calculs, il n'est pas visé.</summary>
        protected static bool DansCondition(JetonAnalyse jeton)
        {
            return jeton.Clause == "WHERE" || jeton.Clause == "ON";
        }

        protected static bool EstComparaison(JetonAnalyse jeton)
        {
            if (jeton.Type != TypeJetonSql.Operateur) return false;
            string t = jeton.Jeton.Texte;
            return t == "=" || t == "<>" || t == "!=" || t == "<" || t == "<=" || t == ">" || t == ">=";
        }

        /// <summary>Vrai pour un nom de colonne : identifiant qui n'est pas un appel de fonction.</summary>
        protected static bool EstColonne(List<JetonAnalyse> jetons, int i, Plage plage)
        {
            return jetons[i].Type == TypeJetonSql.Identifiant && !(i + 1 < plage.Fin && jetons[i + 1].EstPonctuation("("));
        }
    }

    /// <summary>
    /// OPT-02 : fonction ou calcul appliqué à une colonne dans un WHERE ou un ON. DB2 for i ne
    /// peut plus utiliser la clé ni les logiques sur cette colonne : le fichier est lu en entier.
    /// </summary>
    public sealed class RegleFonctionSurColonne : RegleInstruction
    {
        private static readonly HashSet<string> Fonctions = new HashSet<string>(StringComparer.Ordinal)
        {
            "SUBSTR", "SUBSTRING", "LEFT", "RIGHT", "UPPER", "LOWER", "UCASE", "LCASE", "TRIM", "RTRIM", "LTRIM", "STRIP",
            "YEAR", "MONTH", "DAY", "DAYS", "DATE", "TIME", "TIMESTAMP", "WEEK", "QUARTER", "DAYOFWEEK", "HOUR", "MINUTE",
            "DIGITS", "CHAR", "VARCHAR", "DECIMAL", "DEC", "INTEGER", "INT", "BIGINT", "SMALLINT", "ZONED", "CAST",
            "COALESCE", "IFNULL", "VALUE", "NULLIF", "CONCAT", "REPLACE", "TRANSLATE", "ABS", "ROUND", "TRUNCATE", "TRUNC",
            "MOD", "LENGTH", "VARCHAR_FORMAT", "TO_CHAR", "TO_DATE", "LPAD", "RPAD"
        };

        private static readonly HashSet<string> OperateursCalcul = new HashSet<string>(StringComparer.Ordinal) { "+", "-", "*", "/", "||" };

        public override string Code { get { return "OPT-02"; } }
        public override string Libelle { get { return "Pas de fonction ni de calcul sur une colonne filtrée"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Avertissement; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            for (int i = plage.Debut; i < plage.Fin; i++)
            {
                JetonAnalyse jeton = jetons[i];
                if (!DansCondition(jeton)) continue;

                // Appel de fonction contenant une colonne (hors sous-requête).
                if (Fonctions.Contains(jeton.Majuscules) && (jeton.Type == TypeJetonSql.Identifiant || jeton.Type == TypeJetonSql.MotCle)
                    && i + 1 < plage.Fin && jetons[i + 1].EstPonctuation("("))
                {
                    int fermante = AnalyseLexicale.Fermante(jetons, i + 1);
                    if (fermante < 0 || fermante >= plage.Fin) continue;
                    bool colonne = false, sousRequete = false;
                    for (int k = i + 2; k < fermante; k++)
                    {
                        if (jetons[k].EstMot("SELECT")) sousRequete = true;
                        if (EstColonne(jetons, k, plage)) colonne = true;
                    }
                    if (colonne && !sousRequete)
                        Signaler(resultats, jeton.Position, jetons[fermante].Fin,
                            "Fonction " + jeton.Majuscules + " appliquée à une colonne dans le " + jeton.Clause + " : la clé et les logiques ne servent plus, le fichier est lu en entier.",
                            Suggestion(jeton.Majuscules));
                    i = fermante;
                    continue;
                }

                // Calcul sur une colonne en début d'opérande : COL / 100 = 12610, COL + 1 > X.
                if (EstColonne(jetons, i, plage) && i + 1 < plage.Fin && jetons[i + 1].Type == TypeJetonSql.Operateur
                    && OperateursCalcul.Contains(jetons[i + 1].Jeton.Texte) && DebutOperande(jetons, i, plage))
                {
                    Signaler(resultats, jeton.Position, jetons[i + 1].Fin,
                        "Calcul sur la colonne " + jeton.Majuscules + " dans le " + jeton.Clause + " : la clé et les logiques ne servent plus.",
                        "Déplacer le calcul du côté de la constante (ex. COL / 100 = 12610 → COL BETWEEN 1261000 AND 1261099).");
                }
            }
        }

        /// <summary>Vrai si la colonne (éventuellement qualifiée) commence un opérande.</summary>
        private static bool DebutOperande(List<JetonAnalyse> jetons, int i, Plage plage)
        {
            int debut = i;
            if (debut - 2 >= plage.Debut && jetons[debut - 1].EstPonctuation(".")) debut -= 2;
            if (debut - 1 < plage.Debut) return false;
            JetonAnalyse avant = jetons[debut - 1];
            return EstComparaison(avant) || avant.EstPonctuation("(") || avant.EstMot("WHERE") || avant.EstMot("AND")
                || avant.EstMot("OR") || avant.EstMot("ON") || avant.EstMot("NOT");
        }

        private static string Suggestion(string fonction)
        {
            switch (fonction)
            {
                case "SUBSTR":
                case "SUBSTRING":
                case "LEFT":
                    return "Pour un début de code : COL LIKE 'ABC%' (la clé reste utilisable).";
                case "YEAR":
                case "MONTH":
                case "DAY":
                case "DAYS":
                case "DATE":
                case "TIMESTAMP":
                case "WEEK":
                case "QUARTER":
                case "DIGITS":
                case "DECIMAL":
                case "DEC":
                case "INTEGER":
                case "INT":
                case "CHAR":
                    return "Comparer la colonne brute à des bornes (BETWEEN) ; pour une date Atlas, à sa valeur stockée (ex. DTDUE BETWEEN 1261001 AND 1261031).";
                case "UPPER":
                case "LOWER":
                case "UCASE":
                case "LCASE":
                case "TRIM":
                case "RTRIM":
                case "LTRIM":
                case "STRIP":
                    return "Comparer la colonne telle quelle : les codes Atlas sont en majuscules et les blancs de fin sont ignorés par « = ».";
                case "COALESCE":
                case "IFNULL":
                case "VALUE":
                    return "Écrire (COL = valeur OR COL IS NULL) plutôt que COALESCE(COL, ...) = valeur.";
                default:
                    return "Comparer la colonne brute à une valeur calculée à l'avance.";
            }
        }
    }

    /// <summary>OPT-03 : LIKE commençant par % ou _ : aucune clé ne peut servir, lecture complète.</summary>
    public sealed class RegleLikeJokerInitial : RegleInstruction
    {
        public override string Code { get { return "OPT-03"; } }
        public override string Libelle { get { return "LIKE sans joker en tête"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Avertissement; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            for (int i = plage.Debut; i + 1 < plage.Fin; i++)
            {
                if (!jetons[i].EstMot("LIKE")) continue;
                JetonAnalyse motif = jetons[i + 1];
                string texte = motif.Jeton.Texte ?? string.Empty;
                if (motif.Type == TypeJetonSql.Chaine && (texte.StartsWith("'%", StringComparison.Ordinal) || texte.StartsWith("'_", StringComparison.Ordinal)))
                    Signaler(resultats, jetons[i].Position, motif.Fin,
                        "LIKE " + texte + " commence par un joker : le fichier est lu en entier.",
                        "Rechercher plutôt un début de valeur (LIKE 'ABC%') quand c'est possible, ou restreindre d'abord par un autre filtre.");
            }
        }
    }

    /// <summary>
    /// OPT-04 : NOT IN (sous-requête). Si la sous-requête ramène un seul NULL, aucune ligne ne
    /// ressort : le résultat est faux, pas seulement lent.
    /// </summary>
    public sealed class RegleNotInSousRequete : RegleInstruction
    {
        public override string Code { get { return "OPT-04"; } }
        public override string Libelle { get { return "NOT IN (sous-requête) remplacé par NOT EXISTS"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Erreur; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            for (int i = plage.Debut; i + 3 < plage.Fin; i++)
            {
                if (jetons[i].EstMot("NOT") && jetons[i + 1].EstMot("IN") && jetons[i + 2].EstPonctuation("(")
                    && (jetons[i + 3].EstMot("SELECT") || jetons[i + 3].EstMot("WITH")))
                    Signaler(resultats, jetons[i].Position, jetons[i + 1].Fin,
                        "NOT IN (sous-requête) : si la sous-requête ramène un NULL, AUCUNE ligne ne ressort.",
                        "Écrire NOT EXISTS (SELECT 1 FROM ... WHERE colonne = colonne_extérieure).");
            }
        }
    }

    /// <summary>OPT-05 : UPDATE ou DELETE sans WHERE principal : toute la table est touchée.</summary>
    public sealed class RegleModificationSansWhere : RegleInstruction
    {
        public override string Code { get { return "OPT-05"; } }
        public override string Libelle { get { return "UPDATE / DELETE avec WHERE"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Erreur; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            string texte = Texte(sql, jetons, plage);
            TypeInstructionSql type = AnalyseurInstruction.DetecterType(texte);
            if ((type == TypeInstructionSql.Update || type == TypeInstructionSql.Delete) && !AnalyseurInstruction.ContientWhere(texte))
                Signaler(resultats, jetons[plage.Debut].Position, jetons[plage.Debut].Fin,
                    jetons[plage.Debut].Majuscules + " sans WHERE principal : TOUTES les lignes de la table seront touchées.",
                    "Ajouter une clause WHERE (un WHERE placé dans une sous-requête ne limite pas les lignes modifiées).");
        }
    }

    /// <summary>OPT-06 : produit cartésien (CROSS JOIN, ON 1 = 1, FROM A, B sans condition qui les relie).</summary>
    public sealed class RegleProduitCartesien : RegleInstruction
    {
        public override string Code { get { return "OPT-06"; } }
        public override string Libelle { get { return "Pas de produit cartésien"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Erreur; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            for (int i = plage.Debut; i < plage.Fin; i++)
            {
                JetonAnalyse jeton = jetons[i];
                if (jeton.EstMot("CROSS") && i + 1 < plage.Fin && jetons[i + 1].EstMot("JOIN"))
                    Signaler(resultats, jeton.Position, jetons[i + 1].Fin, "CROSS JOIN : chaque ligne est associée à toutes les lignes de l'autre table.",
                        "Remplacer par INNER JOIN ... ON avec la condition qui relie les tables.");

                // ON 1 = 1 (condition toujours vraie)
                if (jeton.EstMot("ON") && i + 3 < plage.Fin && jetons[i + 1].Type == TypeJetonSql.Nombre && jetons[i + 2].Jeton.Texte == "="
                    && jetons[i + 3].Type == TypeJetonSql.Nombre && jetons[i + 1].Jeton.Texte == jetons[i + 3].Jeton.Texte)
                    Signaler(resultats, jeton.Position, jetons[i + 3].Fin, "Condition de jointure toujours vraie : produit cartésien.",
                        "Relier les tables par leurs colonnes communes (ex. B.ITNBR = A.ITNBR).");

                // FROM A, B : tables séparées par des virgules sans égalité qui relie deux tables.
                if (jeton.EstMot("FROM") && !(jeton.Ouvrante >= 1 && EstFonctionAvecFrom(jetons[jeton.Ouvrante - 1])))
                    ControlerListe(jetons, i, plage, resultats);
            }
        }

        private static bool EstFonctionAvecFrom(JetonAnalyse jeton)
        {
            return jeton.EstMot("EXTRACT") || jeton.EstMot("TRIM") || jeton.EstMot("SUBSTRING") || jeton.EstMot("POSITION") || jeton.EstMot("OVERLAY");
        }

        private void ControlerListe(List<JetonAnalyse> jetons, int from, Plage plage, List<ResultatAnalyse> resultats)
        {
            int profondeur = jetons[from].Profondeur;
            int virgules = 0;
            bool lien = false;
            for (int k = from + 1; k < plage.Fin; k++)
            {
                JetonAnalyse jeton = jetons[k];
                if (jeton.Profondeur < profondeur) break;
                if (jeton.Profondeur == profondeur && (jeton.EstMot("UNION") || jeton.EstMot("EXCEPT") || jeton.EstMot("INTERSECT"))) break;
                if (jeton.Profondeur == profondeur && jeton.Clause == "FROM" && jeton.EstPonctuation(",")) virgules++;
                // A.X = B.Y : égalité entre deux colonnes qualifiées par des tables différentes.
                if (jeton.Jeton.Texte == "=" && jeton.Type == TypeJetonSql.Operateur && k - 3 > from && k + 3 < plage.Fin
                    && jetons[k - 2].EstPonctuation(".") && jetons[k + 2].EstPonctuation(".")
                    && !string.Equals(jetons[k - 3].Majuscules, jetons[k + 1].Majuscules, StringComparison.Ordinal))
                    lien = true;
            }
            if (virgules > 0 && !lien)
                Signaler(resultats, jetons[from].Position, jetons[from].Fin,
                    "FROM liste " + (virgules + 1) + " tables séparées par des virgules sans condition qui les relie : produit cartésien probable.",
                    "Écrire INNER JOIN ... ON (ou ajouter dans le WHERE l'égalité qui relie les tables, ex. B.ITNBR = A.ITNBR).");
        }
    }

    /// <summary>OPT-07 : consultation sans filtre ni limite : peut ramener tout un fichier volumineux.</summary>
    public sealed class RegleConsultationSansLimite : RegleInstruction
    {
        public override string Code { get { return "OPT-07"; } }
        public override string Libelle { get { return "Consultation filtrée ou limitée"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Information; } }

        private static readonly HashSet<string> Agregats = new HashSet<string>(StringComparer.Ordinal) { "COUNT", "SUM", "MIN", "MAX", "AVG", "COUNT_BIG" };

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            if (AnalyseurInstruction.DetecterType(Texte(sql, jetons, plage)) != TypeInstructionSql.Select) return;

            JetonAnalyse selection = null;
            bool from = false, filtre = false, agregat = false;
            for (int i = plage.Debut; i < plage.Fin; i++)
            {
                JetonAnalyse jeton = jetons[i];
                if (jeton.Profondeur != 0) continue;
                if (jeton.EstMot("SELECT") && selection == null) selection = jeton;
                if (jeton.EstMot("FROM")) from = true;
                if (jeton.EstMot("WHERE") || jeton.EstMot("FETCH") || jeton.EstMot("LIMIT") || jeton.EstMot("GROUP") || jeton.EstMot("OPTIMIZE")) filtre = true;
                if (jeton.Clause == "SELECT" && Agregats.Contains(jeton.Majuscules) && i + 1 < plage.Fin && jetons[i + 1].EstPonctuation("(")) agregat = true;
                if (jeton.EstMot("SYSDUMMY1")) filtre = true;
            }
            if (selection == null || !from || filtre || agregat) return;
            Signaler(resultats, selection.Position, selection.Fin,
                "Consultation sans filtre ni limite : sur un fichier Atlas, elle peut ramener des centaines de milliers de lignes.",
                "Ajouter un WHERE ou FETCH FIRST n ROWS ONLY.");
        }
    }

    /// <summary>
    /// OPT-08 : table non qualifiée. Elle est lue dans la bibliothèque par défaut du profil :
    /// dangereux quand une bibliothèque de test côtoie celle de production.
    /// </summary>
    public sealed class RegleTableNonQualifiee : RegleInstruction
    {
        public override string Code { get { return "OPT-08"; } }
        public override string Libelle { get { return "Tables qualifiées par leur bibliothèque"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Information; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            foreach (TableCitee table in AnalyseLexicale.TablesCitees(jetons, plage))
            {
                if (table.Schema != null) continue;
                Signaler(resultats, jetons[table.Indice].Position, table.Fin,
                    "Table « " + table.Nom + " » non qualifiée : elle sera lue dans la bibliothèque par défaut du profil de connexion.",
                    "Écrire BIBLIOTHEQUE." + table.Nom + " (ex. AMFLIB." + table.Nom + ") pour viser sans ambiguïté la bonne bibliothèque.");
            }
        }
    }

    /// <summary>
    /// OPT-09 : texte comparé à un nombre (ou l'inverse), d'après les types du catalogue chargé.
    /// Conversion implicite : index inutilisable, et pour un code CHAR, zéros de tête perdus.
    /// Sans catalogue (résolveur absent ou colonne ambiguë), la règle se tait.
    /// </summary>
    public sealed class RegleTypesMelanges : RegleInstruction
    {
        private readonly Func<string, CategorieType?> pTypeColonne;

        /// <param name="typeColonne">Famille de type d'une colonne par son nom, null si inconnue ou ambiguë.</param>
        public RegleTypesMelanges(Func<string, CategorieType?> typeColonne)
        {
            pTypeColonne = typeColonne;
        }

        public override string Code { get { return "OPT-09"; } }
        public override string Libelle { get { return "Constantes du type de la colonne"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Avertissement; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            if (pTypeColonne == null) return;
            for (int i = plage.Debut; i < plage.Fin; i++)
            {
                if (!DansCondition(jetons[i]) && jetons[i].Clause != "HAVING") continue;
                if (!EstColonne(jetons, i, plage)) continue;
                CategorieType? categorie = pTypeColonne(AnalyseLexicale.Nom(jetons[i]));
                if (!categorie.HasValue) continue;

                // COL op constante, constante op COL, COL IN (constantes), COL BETWEEN c1 AND c2.
                List<JetonAnalyse> constantes = new List<JetonAnalyse>();
                if (i + 2 < plage.Fin && EstComparaison(jetons[i + 1])) constantes.Add(jetons[i + 2]);
                if (i - 2 >= plage.Debut && EstComparaison(jetons[i - 1])) constantes.Add(jetons[i - 2]);
                if (i + 2 < plage.Fin && jetons[i + 1].EstMot("IN") && jetons[i + 2].EstPonctuation("("))
                {
                    int fermante = AnalyseLexicale.Fermante(jetons, i + 2);
                    for (int k = i + 3; k > 0 && k < fermante; k++) constantes.Add(jetons[k]);
                }
                if (i + 4 < plage.Fin && jetons[i + 1].EstMot("BETWEEN") && jetons[i + 3].EstMot("AND"))
                {
                    constantes.Add(jetons[i + 2]);
                    constantes.Add(jetons[i + 4]);
                }

                foreach (JetonAnalyse constante in constantes)
                {
                    string probleme = Incompatibilite(categorie.Value, constante);
                    if (probleme == null) continue;
                    Signaler(resultats, constante.Position, constante.Fin,
                        jetons[i].Majuscules + " " + probleme + " (" + constante.Jeton.Texte + ") : conversion implicite, index inutilisable.",
                        categorie.Value == CategorieType.Texte
                            ? "Écrire la constante entre apostrophes, zéros de tête compris (ex. '000123')."
                            : "Écrire la constante sans apostrophes (ex. 1261001 pour une date Atlas).");
                    break;
                }
            }
        }

        private static string Incompatibilite(CategorieType categorie, JetonAnalyse constante)
        {
            if (constante.Type == TypeJetonSql.Chaine && categorie == CategorieType.Numerique) return "est numérique mais comparée à un texte";
            if (constante.Type == TypeJetonSql.Nombre && categorie == CategorieType.Texte) return "est un texte mais comparée à un nombre";
            if (constante.Type == TypeJetonSql.Nombre && (categorie == CategorieType.Date || categorie == CategorieType.Horodatage))
                return "est une date mais comparée à un nombre";
            return null;
        }
    }

    /// <summary>
    /// OPT-10 : OR dans une condition. Entre colonnes différentes, il empêche souvent l'usage
    /// d'un index ; sur une même colonne, IN est plus lisible et mieux optimisé.
    /// </summary>
    public sealed class RegleOrColonnes : RegleInstruction
    {
        public override string Code { get { return "OPT-10"; } }
        public override string Libelle { get { return "OR entre colonnes"; } }
        public override Severite SeveriteParDefaut { get { return Severite.Information; } }

        protected override void AnalyserInstruction(string sql, List<JetonAnalyse> jetons, Plage plage, List<ResultatAnalyse> resultats)
        {
            for (int i = plage.Debut; i < plage.Fin; i++)
            {
                JetonAnalyse ou = jetons[i];
                if (!ou.EstMot("OR") || !DansCondition(ou)) continue;
                string avant = ColonneComparee(jetons, i, -1, plage);
                string apres = ColonneComparee(jetons, i, +1, plage);
                if (avant == null || apres == null) continue;
                if (avant == apres)
                    Signaler(resultats, ou.Position, ou.Fin, "OR répété sur " + avant + ".",
                        "Écrire " + avant + " IN (valeur1, valeur2) : plus lisible et mieux optimisé.");
                else
                    Signaler(resultats, ou.Position, ou.Fin, "OR entre " + avant + " et " + apres + " : l'optimiseur ne peut souvent utiliser aucun index.",
                        "Si c'est lent : deux requêtes réunies par UNION, chacune filtrant sur sa colonne.");
            }
        }

        /// <summary>Colonne de la comparaison qui précède (sens -1) ou suit (sens +1) le OR, au même niveau.</summary>
        private static string ColonneComparee(List<JetonAnalyse> jetons, int ou, int sens, Plage plage)
        {
            int profondeur = jetons[ou].Profondeur;
            for (int k = ou + sens; k >= plage.Debut && k < plage.Fin; k += sens)
            {
                JetonAnalyse jeton = jetons[k];
                if (jeton.Profondeur < profondeur) return null;
                if (jeton.Profondeur == profondeur && (jeton.EstMot("AND") || jeton.EstMot("OR") || jeton.EstMot("WHERE") || jeton.EstMot("ON"))) return null;
                if (jeton.Profondeur != profondeur || !EstComparaison(jeton)) continue;
                // La colonne est à gauche de la comparaison, éventuellement dans un appel de fonction : SUBSTR(ITNBR, 1, 3) = ...
                if (k - 1 >= plage.Debut && jetons[k - 1].EstPonctuation(")"))
                {
                    for (int o = k - 2; o >= plage.Debut; o--)
                    {
                        if (!jetons[o].EstPonctuation("(") || jetons[o].Profondeur != jetons[k - 1].Profondeur) continue;
                        for (int c = o + 1; c < k - 1; c++)
                            if (EstColonne(jetons, c, plage)) return AnalyseLexicale.Nom(jetons[c]);
                        return null;
                    }
                    return null;
                }
                for (int c = k - 1; c >= plage.Debut && c > k - 4; c--)
                    if (EstColonne(jetons, c, plage)) return AnalyseLexicale.Nom(jetons[c]);
                return null;
            }
            return null;
        }
    }

    /// <summary>Jeu de règles d'analyse de l'éditeur, dans l'ordre des codes. (OPT-01 à OPT-10)</summary>
    public static class CatalogueRegles
    {
        /// <param name="typeColonne">Types du catalogue chargé pour OPT-09 (null : règle inactive).</param>
        public static List<IRegleAnalyse> Toutes(Func<string, CategorieType?> typeColonne)
        {
            return new List<IRegleAnalyse>
            {
                new RegleSelectEtoile(),
                new RegleFonctionSurColonne(),
                new RegleLikeJokerInitial(),
                new RegleNotInSousRequete(),
                new RegleModificationSansWhere(),
                new RegleProduitCartesien(),
                new RegleConsultationSansLimite(),
                new RegleTableNonQualifiee(),
                new RegleTypesMelanges(typeColonne),
                new RegleOrColonnes()
            };
        }
    }
}

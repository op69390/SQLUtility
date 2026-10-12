using System;
using System.Collections.Generic;
using SQLUtility.Core.Edition;

namespace SQLUtility.Core.Optimisation
{
    /// <summary>
    /// Jeton significatif (ni espace ni commentaire) d'une instruction, situé dans sa
    /// structure : profondeur de parenthèses et clause en cours (WHERE, ON, FROM...).
    /// </summary>
    public sealed class JetonAnalyse
    {
        public JetonAnalyse(JetonSql jeton, int profondeur, string clause, int ouvrante)
        {
            Jeton = jeton;
            Profondeur = profondeur;
            Clause = clause;
            Ouvrante = ouvrante;
            Majuscules = (jeton.Texte ?? string.Empty).ToUpperInvariant();
        }

        public JetonSql Jeton { get; }

        /// <summary>Nombre de parenthèses ouvertes autour du jeton.</summary>
        public int Profondeur { get; }

        /// <summary>Clause en cours : SELECT, FROM, JOIN, ON, WHERE, GROUP, HAVING, ORDER, SET, INTO, FETCH, WITH...</summary>
        public string Clause { get; }

        /// <summary>Indice (dans la liste) de la parenthèse ouvrante qui contient le jeton, -1 au niveau principal.</summary>
        public int Ouvrante { get; }

        public string Majuscules { get; }

        public TypeJetonSql Type
        {
            get { return Jeton.Type; }
        }

        public int Position
        {
            get { return Jeton.Position; }
        }

        public int Fin
        {
            get { return Jeton.Position + Jeton.Longueur; }
        }

        /// <summary>Vrai pour un mot (mot-clé ou identifiant) égal à <paramref name="mot"/>.</summary>
        public bool EstMot(string mot)
        {
            return (Type == TypeJetonSql.MotCle || Type == TypeJetonSql.Identifiant) && Majuscules == mot;
        }

        public bool EstPonctuation(string signe)
        {
            return Type == TypeJetonSql.Ponctuation && Jeton.Texte == signe;
        }

        public override string ToString()
        {
            return Jeton.Texte + " [" + Profondeur + "," + Clause + "]";
        }
    }

    /// <summary>
    /// Lecture structurée du SQL pour les règles d'analyse (OPT-02 à OPT-10) et la recherche
    /// des tables citées : sans évaluer la grammaire complète, elle situe chaque jeton dans
    /// sa clause et ses parenthèses, en ignorant chaînes et commentaires.
    /// </summary>
    public static class AnalyseLexicale
    {
        private static readonly HashSet<string> MotsClause = new HashSet<string>(StringComparer.Ordinal)
        {
            "SELECT", "FROM", "WHERE", "GROUP", "HAVING", "ORDER", "SET", "VALUES", "ON", "INTO", "FETCH",
            "WITH", "UPDATE", "DELETE", "INSERT", "MERGE", "USING", "UNION", "EXCEPT", "INTERSECT", "LIMIT"
        };

        /// <summary>Fonctions dont un FROM interne n'introduit pas de table : EXTRACT(YEAR FROM d)...</summary>
        private static readonly HashSet<string> FonctionsAvecFrom = new HashSet<string>(StringComparer.Ordinal)
        {
            "EXTRACT", "TRIM", "SUBSTRING", "POSITION", "OVERLAY"
        };

        /// <summary>Jetons significatifs du texte, situés dans leur clause et leurs parenthèses.</summary>
        public static List<JetonAnalyse> Jetons(string sql)
        {
            List<JetonAnalyse> jetons = new List<JetonAnalyse>();
            Stack<string> clauses = new Stack<string>();
            Stack<int> ouvrantes = new Stack<int>();
            string clause = string.Empty;
            foreach (JetonSql jeton in TokeniseurSql.Tokeniser(sql))
            {
                if (jeton.Type == TypeJetonSql.Espace || jeton.Type == TypeJetonSql.Commentaire) continue;
                int ouvrante = ouvrantes.Count > 0 ? ouvrantes.Peek() : -1;

                if (jeton.Type == TypeJetonSql.Ponctuation && jeton.Texte == "(")
                {
                    jetons.Add(new JetonAnalyse(jeton, ouvrantes.Count, clause, ouvrante));
                    // Une parenthèse hérite de la clause (appel de fonction) ; une sous-requête la redéfinit.
                    clauses.Push(clause);
                    ouvrantes.Push(jetons.Count - 1);
                    continue;
                }
                if (jeton.Type == TypeJetonSql.Ponctuation && jeton.Texte == ")" && ouvrantes.Count > 0)
                {
                    ouvrantes.Pop();
                    clause = clauses.Pop();
                    jetons.Add(new JetonAnalyse(jeton, ouvrantes.Count, clause, ouvrantes.Count > 0 ? ouvrantes.Peek() : -1));
                    continue;
                }

                string mot = (jeton.Texte ?? string.Empty).ToUpperInvariant();
                if (jeton.Type == TypeJetonSql.MotCle || jeton.Type == TypeJetonSql.Identifiant)
                {
                    if (MotsClause.Contains(mot)) clause = mot;
                    else if (mot == "JOIN") clause = "JOIN";
                }
                jetons.Add(new JetonAnalyse(jeton, ouvrantes.Count, clause, ouvrante));
            }
            return jetons;
        }

        /// <summary>
        /// Instructions du texte : plages [début, fin[ d'indices dans <paramref name="jetons"/>,
        /// séparées par les « ; » du niveau principal (les « ; » sont exclus).
        /// </summary>
        public static List<Plage> Instructions(List<JetonAnalyse> jetons)
        {
            List<Plage> plages = new List<Plage>();
            int debut = 0;
            for (int i = 0; i <= jetons.Count; i++)
            {
                bool separateur = i < jetons.Count && jetons[i].Profondeur == 0 && jetons[i].EstPonctuation(";");
                if (i < jetons.Count && !separateur) continue;
                if (i > debut) plages.Add(new Plage(debut, i));
                debut = i + 1;
            }
            return plages;
        }

        /// <summary>Indice de la parenthèse fermante associée à l'ouvrante d'indice <paramref name="ouvrante"/>, -1 si absente.</summary>
        public static int Fermante(List<JetonAnalyse> jetons, int ouvrante)
        {
            int profondeur = jetons[ouvrante].Profondeur;
            for (int i = ouvrante + 1; i < jetons.Count; i++)
                if (jetons[i].EstPonctuation(")") && jetons[i].Profondeur == profondeur) return i;
            return -1;
        }

        /// <summary>Noms des étapes définies par WITH (nom AS (...) ou nom (colonnes) AS (...)).</summary>
        public static HashSet<string> NomsEtapes(List<JetonAnalyse> jetons, Plage plage)
        {
            HashSet<string> noms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = plage.Debut; i + 1 < plage.Fin; i++)
            {
                JetonAnalyse jeton = jetons[i];
                if (jeton.Profondeur != 0 || jeton.Clause != "WITH" || jeton.Type != TypeJetonSql.Identifiant) continue;
                int suivant = i + 1;
                if (jetons[suivant].EstPonctuation("("))
                {
                    int fermante = Fermante(jetons, suivant);
                    if (fermante < 0) continue;
                    suivant = fermante + 1;
                }
                if (suivant < plage.Fin && jetons[suivant].EstMot("AS")) noms.Add(jeton.Jeton.Texte);
            }
            return noms;
        }

        /// <summary>
        /// Tables citées après FROM, JOIN, UPDATE et INTO (hors étapes WITH, sous-requêtes et
        /// fonctions table) : indice du premier jeton, schéma (null si non qualifié) et nom.
        /// </summary>
        public static List<TableCitee> TablesCitees(List<JetonAnalyse> jetons, Plage plage)
        {
            List<TableCitee> tables = new List<TableCitee>();
            HashSet<string> etapes = NomsEtapes(jetons, plage);
            for (int i = plage.Debut + 1; i < plage.Fin; i++)
            {
                JetonAnalyse precedent = jetons[i - 1];
                if (!(precedent.EstMot("FROM") || precedent.EstMot("JOIN") || precedent.EstMot("UPDATE") || precedent.EstMot("INTO"))) continue;
                if (precedent.EstMot("FROM") && precedent.Ouvrante >= 1 && FonctionsAvecFrom.Contains(jetons[precedent.Ouvrante - 1].Majuscules)) continue;

                JetonAnalyse jeton = jetons[i];
                if (jeton.Type != TypeJetonSql.Identifiant && jeton.Type != TypeJetonSql.MotCle) continue;
                // Nom suivi de « ( » : fonction table (TABLE(...), UDTF), sauf après INTO où c'est la liste de colonnes.
                if (i + 1 < plage.Fin && jetons[i + 1].EstPonctuation("(") && !precedent.EstMot("INTO")) continue;
                if (jeton.EstMot("TABLE") || jeton.EstMot("LATERAL") || jeton.EstMot("FINAL") || jeton.EstMot("NEW")) continue;

                bool qualifiee = i + 2 < plage.Fin && (jetons[i + 1].EstPonctuation(".") || jetons[i + 1].Jeton.Texte == "/")
                    && (jetons[i + 2].Type == TypeJetonSql.Identifiant || jetons[i + 2].Type == TypeJetonSql.MotCle);
                if (qualifiee)
                    tables.Add(new TableCitee(i, Nom(jeton), Nom(jetons[i + 2]), jetons[i + 2].Fin));
                else if (!etapes.Contains(jeton.Jeton.Texte))
                    tables.Add(new TableCitee(i, null, Nom(jeton), jeton.Fin));
            }
            return tables;
        }

        /// <summary>Nom tel qu'écrit : délimité (casse conservée) ou en majuscules.</summary>
        public static string Nom(JetonAnalyse jeton)
        {
            string texte = jeton.Jeton.Texte ?? string.Empty;
            if (texte.Length >= 2 && texte[0] == '"' && texte[texte.Length - 1] == '"') return texte.Substring(1, texte.Length - 2);
            return texte.ToUpperInvariant();
        }
    }

    /// <summary>Plage [Debut, Fin[ d'indices de jetons (une instruction).</summary>
    public struct Plage
    {
        public Plage(int debut, int fin)
        {
            Debut = debut;
            Fin = fin;
        }

        public int Debut { get; }
        public int Fin { get; }
    }

    /// <summary>Table citée dans une instruction.</summary>
    public sealed class TableCitee
    {
        public TableCitee(int indice, string schema, string nom, int fin)
        {
            Indice = indice;
            Schema = schema;
            Nom = nom;
            Fin = fin;
        }

        /// <summary>Indice du premier jeton du nom dans la liste de l'instruction.</summary>
        public int Indice { get; }

        /// <summary>Schéma écrit, ou null si le nom n'est pas qualifié.</summary>
        public string Schema { get; }

        public string Nom { get; }

        /// <summary>Position de fin du nom dans le texte.</summary>
        public int Fin { get; }
    }
}

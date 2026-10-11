using System;
using System.Collections.Generic;

namespace SQLUtility.Core.Edition
{
    /// <summary>
    /// Découpe un texte SQL en jetons (mots-clés, identifiants, chaînes, commentaires,
    /// nombres, opérateurs, ponctuation). Logique pure, réutilisable par la coloration
    /// de l'éditeur (EF-15) et par le moteur de lint (OPT-xx).
    /// </summary>
    public static class TokeniseurSql
    {
        private const string CaracteresOperateur = "+-*/<>=!|&%~^";

        // Les mots-clés sont centralisés dans MotsClesSql (partagés avec l'auto-complétion).

        public static IReadOnlyList<JetonSql> Tokeniser(string sql)
        {
            List<JetonSql> jetons = new List<JetonSql>();
            if (string.IsNullOrEmpty(sql)) return jetons;

            int i = 0;
            int n = sql.Length;
            while (i < n)
            {
                char c = sql[i];

                // Espaces
                if (char.IsWhiteSpace(c))
                {
                    int debut = i;
                    while (i < n && char.IsWhiteSpace(sql[i])) i++;
                    jetons.Add(Creer(sql, TypeJetonSql.Espace, debut, i - debut));
                    continue;
                }

                // Commentaire ligne : -- jusqu'à la fin de ligne
                if (c == '-' && i + 1 < n && sql[i + 1] == '-')
                {
                    int debut = i;
                    i += 2;
                    while (i < n && sql[i] != '\n' && sql[i] != '\r') i++;
                    jetons.Add(Creer(sql, TypeJetonSql.Commentaire, debut, i - debut));
                    continue;
                }

                // Commentaire bloc : /* ... */
                if (c == '/' && i + 1 < n && sql[i + 1] == '*')
                {
                    int debut = i;
                    i += 2;
                    while (i < n && !(sql[i] == '*' && i + 1 < n && sql[i + 1] == '/')) i++;
                    i = (i < n) ? i + 2 : n; // Consomme le */ final, ou va jusqu'à la fin si non terminé.
                    jetons.Add(Creer(sql, TypeJetonSql.Commentaire, debut, i - debut));
                    continue;
                }

                // Chaîne : '...' avec '' comme échappement
                if (c == '\'')
                {
                    int debut = i;
                    i++;
                    while (i < n)
                    {
                        if (sql[i] == '\'')
                        {
                            if (i + 1 < n && sql[i + 1] == '\'') { i += 2; continue; }
                            i++;
                            break;
                        }
                        i++;
                    }
                    jetons.Add(Creer(sql, TypeJetonSql.Chaine, debut, i - debut));
                    continue;
                }

                // Identifiant délimité : "..."
                if (c == '"')
                {
                    int debut = i;
                    i++;
                    while (i < n && sql[i] != '"') i++;
                    if (i < n) i++;
                    jetons.Add(Creer(sql, TypeJetonSql.Identifiant, debut, i - debut));
                    continue;
                }

                // Nombre
                if (char.IsDigit(c) || (c == '.' && i + 1 < n && char.IsDigit(sql[i + 1])))
                {
                    int debut = i;
                    while (i < n && (char.IsDigit(sql[i]) || sql[i] == '.')) i++;
                    jetons.Add(Creer(sql, TypeJetonSql.Nombre, debut, i - debut));
                    continue;
                }

                // Mot : mot-clé ou identifiant
                if (char.IsLetter(c) || c == '_')
                {
                    int debut = i;
                    while (i < n && (char.IsLetterOrDigit(sql[i]) || sql[i] == '_' || sql[i] == '@' || sql[i] == '#' || sql[i] == '$')) i++;
                    string mot = sql.Substring(debut, i - debut);
                    TypeJetonSql type = MotsClesSql.EstMotCle(mot) ? TypeJetonSql.MotCle : TypeJetonSql.Identifiant;
                    jetons.Add(new JetonSql(type, debut, i - debut, mot));
                    continue;
                }

                // Ponctuation
                if (c == '(' || c == ')' || c == ',' || c == ';' || c == '.')
                {
                    jetons.Add(Creer(sql, TypeJetonSql.Ponctuation, i, 1));
                    i++;
                    continue;
                }

                // Opérateurs
                if (CaracteresOperateur.IndexOf(c) >= 0)
                {
                    int debut = i;
                    while (i < n && CaracteresOperateur.IndexOf(sql[i]) >= 0) i++;
                    jetons.Add(Creer(sql, TypeJetonSql.Operateur, debut, i - debut));
                    continue;
                }

                // Caractère inconnu isolé
                jetons.Add(Creer(sql, TypeJetonSql.Inconnu, i, 1));
                i++;
            }
            return jetons;
        }

        private static JetonSql Creer(string sql, TypeJetonSql type, int position, int longueur)
        {
            return new JetonSql(type, position, longueur, sql.Substring(position, longueur));
        }
    }
}

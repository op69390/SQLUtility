using System;
using System.Collections.Generic;
using SQLUtility.Core.Edition;

namespace SQLUtility.Core.Execution
{
    /// <summary>
    /// Table modifiée par un INSERT / UPDATE / DELETE / MERGE, pour la détection de
    /// journalisation (EF-30) et le choix du contrôle de validation. Logique pure.
    /// </summary>
    public sealed class TableCible
    {
        public TableCible(string schema, string nom, bool schemaExplicite)
        {
            Schema = schema;
            Nom = nom;
            SchemaExplicite = schemaExplicite;
        }

        /// <summary>Schéma (bibliothèque) ; celui par défaut du profil si l'instruction ne le précise pas.</summary>
        public string Schema { get; }

        public string Nom { get; }

        /// <summary>Vrai si le schéma figure dans l'instruction (LIB.TABLE ou LIB/TABLE).</summary>
        public bool SchemaExplicite { get; }

        public override string ToString()
        {
            return Schema + "." + Nom;
        }

        // Mots qui ne peuvent pas être un nom de table juste après INTO / UPDATE / FROM.
        private static readonly HashSet<string> MotsInterdits = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SET", "WHERE", "VALUES", "SELECT", "WITH", "USING", "ON", "AS"
        };

        /// <summary>
        /// Extrait la table cible. Retourne null si l'instruction n'est pas un DML reconnu,
        /// si le nom est illisible, ou s'il n'est pas qualifié et qu'aucun schéma par défaut
        /// n'est connu. Le schéma par défaut est le premier de la liste du profil
        /// (« LIB1,LIB2 » ou « LIB1 LIB2 »), qui est le schéma courant en nommage SQL.
        /// </summary>
        public static TableCible Extraire(string sql, string schemasParDefaut)
        {
            List<JetonSql> jetons = new List<JetonSql>();
            foreach (JetonSql jeton in TokeniseurSql.Tokeniser(sql))
            {
                if (jeton.Type != TypeJetonSql.Commentaire) jetons.Add(jeton);
            }

            int i = SauterEspaces(jetons, 0);
            if (i >= jetons.Count) return null;
            string verbe = (jetons[i].Texte ?? string.Empty).ToUpperInvariant();
            i = SauterEspaces(jetons, i + 1);

            switch (verbe)
            {
                case "INSERT":
                case "MERGE":
                    if (!EstMot(jetons, i, "INTO")) return null;
                    i = SauterEspaces(jetons, i + 1);
                    break;
                case "DELETE":
                    if (EstMot(jetons, i, "FROM")) i = SauterEspaces(jetons, i + 1);
                    break;
                case "UPDATE":
                    break;
                default:
                    return null;
            }

            string premier = LireNom(jetons, i);
            if (premier == null) return null;
            i++;

            // Qualification : LIB.TABLE (nommage SQL) ou LIB/TABLE (nommage système), sans espace.
            if (i + 1 < jetons.Count && (jetons[i].Texte == "." || jetons[i].Texte == "/"))
            {
                string second = LireNom(jetons, i + 1);
                if (second == null) return null;
                return new TableCible(premier, second, true);
            }

            string schema = PremierSchema(schemasParDefaut);
            if (schema == null) return null;
            return new TableCible(schema, premier, false);
        }

        private static int SauterEspaces(List<JetonSql> jetons, int i)
        {
            while (i < jetons.Count && jetons[i].Type == TypeJetonSql.Espace) i++;
            return i;
        }

        private static bool EstMot(List<JetonSql> jetons, int i, string mot)
        {
            return i < jetons.Count && string.Equals(jetons[i].Texte, mot, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Lit un nom (identifiant ou mot non réservé) ; les noms non délimités passent en majuscules.</summary>
        private static string LireNom(List<JetonSql> jetons, int i)
        {
            if (i >= jetons.Count) return null;
            JetonSql jeton = jetons[i];
            string texte = jeton.Texte ?? string.Empty;

            if (jeton.Type == TypeJetonSql.Identifiant && texte.Length >= 2 && texte[0] == '"' && texte[texte.Length - 1] == '"')
                return texte.Substring(1, texte.Length - 2); // Identifiant délimité : casse conservée.

            if ((jeton.Type == TypeJetonSql.Identifiant || jeton.Type == TypeJetonSql.MotCle)
                && texte.Length > 0 && !MotsInterdits.Contains(texte))
                return texte.ToUpperInvariant();

            return null;
        }

        private static string PremierSchema(string schemasParDefaut)
        {
            if (string.IsNullOrWhiteSpace(schemasParDefaut)) return null;
            string[] morceaux = schemasParDefaut.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            return morceaux.Length == 0 ? null : morceaux[0].Trim().ToUpperInvariant();
        }
    }
}

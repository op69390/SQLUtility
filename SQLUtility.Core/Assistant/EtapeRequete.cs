using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Étape d'une requête en plusieurs temps (niveau 4b) : une consultation nommée
    /// (ETAPE1, ETAPE2...) dont le résultat se réutilise comme une table, puis assemblée en
    /// WITH ETAPE1 (colonnes) AS (...). La liste de colonnes explicite garantit des noms
    /// uniques, même quand deux tables jointes ont une colonne homonyme. (EF-50)
    /// </summary>
    public sealed class EtapeRequete
    {
        public EtapeRequete(string nom, string libelle, string sql, IEnumerable<ColonneInfo> colonnes, IEnumerable<string> dependances)
        {
            if (string.IsNullOrEmpty(nom)) throw new ArgumentNullException(nameof(nom));
            if (string.IsNullOrWhiteSpace(sql)) throw new ArgumentNullException(nameof(sql));
            List<ColonneInfo> liste = new List<ColonneInfo>(colonnes ?? new List<ColonneInfo>());
            if (liste.Count == 0) throw new ArgumentException("Une étape doit avoir au moins une colonne.", nameof(colonnes));
            Nom = nom;
            Libelle = libelle;
            Sql = sql.Trim().TrimEnd(';').TrimEnd();
            Colonnes = new ReadOnlyCollection<ColonneInfo>(liste);
            Dependances = new ReadOnlyCollection<string>(new List<string>(dependances ?? new List<string>()));
        }

        /// <summary>Nom de l'étape, utilisé comme nom de table (ETAPE1...).</summary>
        public string Nom { get; }

        /// <summary>Description lisible (affichée dans la liste des tables).</summary>
        public string Libelle { get; }

        /// <summary>Consultation de l'étape (sans « ; » final).</summary>
        public string Sql { get; }

        /// <summary>Colonnes du résultat, aux noms uniques.</summary>
        public IReadOnlyList<ColonneInfo> Colonnes { get; }

        /// <summary>Étapes utilisées par cette étape (elles doivent la précéder dans le WITH).</summary>
        public IReadOnlyList<string> Dependances { get; }

        /// <summary>Description de l'étape comme une table sans schéma ni clé, utilisable par l'assistant.</summary>
        public StructureTable Structure()
        {
            List<ColonneInfo> colonnes = new List<ColonneInfo>();
            int position = 1;
            foreach (ColonneInfo colonne in Colonnes)
            {
                ColonneInfo copie = ComposeurEtapes.Copier(colonne, colonne.Nom);
                copie.Position = position++;
                colonnes.Add(copie);
            }
            TableInfo table = new TableInfo { Schema = string.Empty, Nom = Nom, NomSysteme = Nom, Libelle = Libelle, Type = "Étape" };
            return new ConstructeurStructureTable().Construire(table, colonnes);
        }
    }

    /// <summary>Assemble les étapes utiles devant une requête (WITH ... AS (...) SELECT ...). (EF-50)</summary>
    public static class ComposeurEtapes
    {
        /// <summary>Vrai si la table désigne une étape (pas de schéma, nom d'une étape connue).</summary>
        public static bool EstEtape(TableInfo table, IEnumerable<EtapeRequete> etapes)
        {
            return Trouver(table, etapes) != null;
        }

        public static EtapeRequete Trouver(TableInfo table, IEnumerable<EtapeRequete> etapes)
        {
            if (table == null || !string.IsNullOrEmpty(table.Schema)) return null;
            foreach (EtapeRequete etape in etapes)
                if (string.Equals(etape.Nom, table.Nom, StringComparison.OrdinalIgnoreCase)) return etape;
            return null;
        }

        /// <summary>
        /// Étapes nécessaires à une requête qui utilise <paramref name="utilisees"/>, dépendances
        /// comprises, dans l'ordre de création (une étape ne voit que les précédentes).
        /// </summary>
        public static List<EtapeRequete> Necessaires(IEnumerable<string> utilisees, IList<EtapeRequete> etapes)
        {
            HashSet<string> requises = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Stack<string> aVoir = new Stack<string>(utilisees);
            while (aVoir.Count > 0)
            {
                string nom = aVoir.Pop();
                if (!requises.Add(nom)) continue;
                foreach (EtapeRequete etape in etapes)
                    if (string.Equals(etape.Nom, nom, StringComparison.OrdinalIgnoreCase))
                        foreach (string dependance in etape.Dependances) aVoir.Push(dependance);
            }
            List<EtapeRequete> resultat = new List<EtapeRequete>();
            foreach (EtapeRequete etape in etapes)
                if (requises.Contains(etape.Nom)) resultat.Add(etape);
            return resultat;
        }

        /// <summary>WITH ETAPE1 (C1, C2) AS (...), ETAPE2 (...) AS (...) puis la requête ; inchangée sans étape.</summary>
        public static string Composer(IList<EtapeRequete> etapes, string sql)
        {
            if (etapes == null || etapes.Count == 0) return sql;
            StringBuilder texte = new StringBuilder();
            for (int i = 0; i < etapes.Count; i++)
            {
                EtapeRequete etape = etapes[i];
                List<string> noms = new List<string>();
                foreach (ColonneInfo colonne in etape.Colonnes) noms.Add(GenerateurSqlSimple.NomSql(colonne.Nom));
                texte.Append(i == 0 ? "WITH " : "," + Environment.NewLine + "     ");
                texte.Append(GenerateurSqlSimple.NomSql(etape.Nom)).Append(" (").Append(string.Join(", ", noms)).Append(") AS (").AppendLine();
                foreach (string ligne in etape.Sql.Replace("\r\n", "\n").Split('\n'))
                    texte.Append("    ").AppendLine(ligne);
                // Parenthèse sur sa propre ligne : un commentaire « -- » de la dernière ligne ne l'avale pas.
                texte.Append(")");
            }
            texte.AppendLine();
            texte.Append(sql);
            return texte.ToString();
        }

        /// <summary>Copie d'une colonne sous un autre nom (description d'un résultat).</summary>
        public static ColonneInfo Copier(ColonneInfo colonne, string nom)
        {
            return new ColonneInfo
            {
                Nom = nom,
                NomSysteme = nom,
                Libelle = colonne.Libelle,
                TypeDonnee = colonne.TypeDonnee,
                Longueur = colonne.Longueur,
                Echelle = colonne.Echelle,
                AccepteNull = colonne.AccepteNull
            };
        }

        /// <summary>Nom unique dans <paramref name="pris"/> : ITNBR, puis ITNBR_2, ITNBR_3...</summary>
        public static string NomUnique(string nom, HashSet<string> pris)
        {
            string candidat = nom;
            for (int n = 2; !pris.Add(candidat); n++) candidat = nom + "_" + n;
            return candidat;
        }
    }
}

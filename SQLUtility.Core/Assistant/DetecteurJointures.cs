using System;
using System.Collections.Generic;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.Core.Assistant
{
    /// <summary>
    /// Propose les conditions de jointure entre une table présente et une table à joindre. (EF-45)
    /// Atlas / MAPICS déclare rarement des clés étrangères ; ses fichiers partagent en revanche
    /// les mêmes noms de zones (ITNBR, HOUSE, ORDNO...). D'où trois sources, de la plus sûre
    /// à la moins sûre : clé étrangère déclarée, clé unique retrouvée en entier par les noms,
    /// clé retrouvée en partie (lignes possiblement multipliées).
    /// </summary>
    public static class DetecteurJointures
    {
        public static IReadOnlyList<JointureProposee> Proposer(StructureTable presente, StructureTable nouvelle)
        {
            if (presente == null) throw new ArgumentNullException(nameof(presente));
            if (nouvelle == null) throw new ArgumentNullException(nameof(nouvelle));

            List<JointureProposee> propositions = new List<JointureProposee>();
            HashSet<string> signatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Clés étrangères déclarées, dans les deux sens (vues depuis l'une ou l'autre table).
            List<RelationInfo> relations = new List<RelationInfo>();
            relations.AddRange(presente.RelationsVersParents);
            relations.AddRange(presente.RelationsDepuisEnfants);
            relations.AddRange(nouvelle.RelationsVersParents);
            relations.AddRange(nouvelle.RelationsDepuisEnfants);
            foreach (RelationInfo relation in relations)
            {
                bool presenteEnfant = MemeTable(relation.SchemaEnfant, relation.TableEnfant, presente.Table)
                    && MemeTable(relation.SchemaParent, relation.TableParent, nouvelle.Table);
                bool presenteParent = MemeTable(relation.SchemaParent, relation.TableParent, presente.Table)
                    && MemeTable(relation.SchemaEnfant, relation.TableEnfant, nouvelle.Table);
                if (!presenteEnfant && !presenteParent) continue;

                List<string> colonnesPresente = presenteEnfant ? relation.ColonnesEnfant : relation.ColonnesParent;
                List<string> colonnesNouvelle = presenteEnfant ? relation.ColonnesParent : relation.ColonnesEnfant;
                JointureProposee proposition = new JointureProposee(OrigineJointure.CleEtrangereDeclaree,
                    relation.Nom + " (" + relation.Correspondance + ")", presenteEnfant);
                if (Apparier(proposition, presente, colonnesPresente, nouvelle, colonnesNouvelle))
                    Ajouter(propositions, signatures, proposition);
            }

            // 2. Clé unique de la nouvelle table retrouvée en entier dans la présente : au plus une ligne.
            foreach (CleInfo cle in nouvelle.Cles)
            {
                if (!cle.PeutIdentifierLigne) continue;
                List<string> noms = new List<string>(cle.Colonnes);
                JointureProposee proposition = new JointureProposee(OrigineJointure.CleCompleteDeduite,
                    "clé de " + nouvelle.Table.Nom + " (" + cle.ListeColonnes + ") retrouvée dans " + presente.Table.Nom
                    + " : au plus une ligne " + nouvelle.Table.Nom + " par ligne " + presente.Table.Nom, true);
                if (Apparier(proposition, presente, noms, nouvelle, noms)) Ajouter(propositions, signatures, proposition);
            }

            // 2 bis. Clé unique de la présente retrouvée dans la nouvelle : relation un-à-plusieurs.
            foreach (CleInfo cle in presente.Cles)
            {
                if (!cle.PeutIdentifierLigne) continue;
                List<string> noms = new List<string>(cle.Colonnes);
                JointureProposee proposition = new JointureProposee(OrigineJointure.CleCompleteDeduite,
                    "clé de " + presente.Table.Nom + " (" + cle.ListeColonnes + ") retrouvée dans " + nouvelle.Table.Nom
                    + " : plusieurs lignes " + nouvelle.Table.Nom + " possibles par ligne " + presente.Table.Nom, false);
                if (Apparier(proposition, presente, noms, nouvelle, noms)) Ajouter(propositions, signatures, proposition);
            }

            // 3. Clé d'identification retrouvée en partie seulement (ex. ITNBR de HOUSE + ITNBR).
            AjouterPartielle(propositions, signatures, presente, nouvelle, nouvelle.CleIdentification, nouvelle.Table.Nom);
            AjouterPartielle(propositions, signatures, presente, nouvelle, presente.CleIdentification, presente.Table.Nom);

            return propositions;
        }

        /// <summary>Vrai si les deux colonnes peuvent être comparées (même famille de type).</summary>
        public static bool TypesCompatibles(ColonneInfo a, ColonneInfo b)
        {
            if (a == null || b == null) return false;
            CategorieType categorie = TypesSql.Categorie(a.TypeDonnee);
            return categorie != CategorieType.NonPrisEnCharge && categorie == TypesSql.Categorie(b.TypeDonnee);
        }

        private static void AjouterPartielle(List<JointureProposee> propositions, HashSet<string> signatures,
            StructureTable presente, StructureTable nouvelle, CleInfo cle, string nomTableCle)
        {
            if (cle == null) return;
            List<string> communs = new List<string>();
            foreach (string nom in cle.Colonnes)
            {
                if (TypesCompatibles(Trouver(presente, nom), Trouver(nouvelle, nom))) communs.Add(nom);
            }
            if (communs.Count == 0 || communs.Count == cle.Colonnes.Count) return;

            JointureProposee proposition = new JointureProposee(OrigineJointure.ClePartielleDeduite,
                string.Join(", ", communs) + " seulement, sur la clé de " + nomTableCle + " (" + cle.ListeColonnes
                + ") : des lignes peuvent être multipliées", false);
            if (Apparier(proposition, presente, communs, nouvelle, communs)) Ajouter(propositions, signatures, proposition);
        }

        /// <summary>Remplit les paires ; faux si une colonne manque ou si les types sont incompatibles.</summary>
        private static bool Apparier(JointureProposee proposition, StructureTable presente, IList<string> nomsPresente,
            StructureTable nouvelle, IList<string> nomsNouvelle)
        {
            if (nomsPresente.Count == 0 || nomsPresente.Count != nomsNouvelle.Count) return false;
            for (int i = 0; i < nomsPresente.Count; i++)
            {
                ColonneInfo a = Trouver(presente, nomsPresente[i]);
                ColonneInfo b = Trouver(nouvelle, nomsNouvelle[i]);
                if (!TypesCompatibles(a, b)) return false;
                proposition.Paires.Add(new KeyValuePair<ColonneInfo, ColonneInfo>(a, b));
            }
            return true;
        }

        /// <summary>Écarte une proposition dont les paires figurent déjà (dans n'importe quel ordre).</summary>
        private static void Ajouter(List<JointureProposee> propositions, HashSet<string> signatures, JointureProposee proposition)
        {
            List<string> paires = new List<string>();
            foreach (KeyValuePair<ColonneInfo, ColonneInfo> paire in proposition.Paires)
                paires.Add(paire.Key.Nom + "=" + paire.Value.Nom);
            paires.Sort(StringComparer.OrdinalIgnoreCase);
            if (signatures.Add(string.Join("|", paires))) propositions.Add(proposition);
        }

        private static ColonneInfo Trouver(StructureTable structure, string nom)
        {
            foreach (ColonneInfo colonne in structure.Colonnes)
            {
                if (string.Equals(colonne.Nom, nom, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(colonne.NomSysteme, nom, StringComparison.OrdinalIgnoreCase))
                    return colonne;
            }
            return null;
        }

        private static bool MemeTable(string schema, string nom, TableInfo table)
        {
            return string.Equals(schema, table.Schema, StringComparison.OrdinalIgnoreCase)
                && string.Equals(nom, table.Nom, StringComparison.OrdinalIgnoreCase);
        }
    }
}

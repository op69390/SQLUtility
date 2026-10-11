using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Data.Parametres;
using SQLUtility.Data.Providers;

namespace SQLUtility.App
{
    /// <summary>
    /// Explorateur de catalogue (EF-07 / EF-08) : arbre schémas → tables, et grille
    /// des colonnes de la table sélectionnée. Accès en lecture seule via le provider.
    /// </summary>
    public partial class FormExplorateurCatalogue : Form
    {
        private readonly ParametresApplication pParametres;

        // Texte du nœud fictif marquant un schéma non encore développé (chargement paresseux).
        private const string MarqueurChargement = "(chargement…)";

        private List<string> pSchemas = new List<string>();
        private ConnexionProfil pProfilActif;
        private IConnexionProvider pProvider;

        public FormExplorateurCatalogue(ParametresApplication parametres)
        {
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));
            pParametres = parametres;
            InitializeComponent();
            PeuplerConnexions();
        }

        // ----------------------------------------------------------------- Connexions

        private void PeuplerConnexions()
        {
            listeConnexions.Items.Clear();
            if (pParametres.Profils != null)
            {
                foreach (ConnexionProfil profil in pParametres.Profils)
                    listeConnexions.Items.Add(profil);
            }

            if (!string.IsNullOrEmpty(pParametres.ProfilParDefaut))
            {
                for (int i = 0; i < listeConnexions.Items.Count; i++)
                {
                    ConnexionProfil profil = listeConnexions.Items[i] as ConnexionProfil;
                    if (profil != null && string.Equals(profil.Nom, pParametres.ProfilParDefaut, StringComparison.OrdinalIgnoreCase))
                    {
                        listeConnexions.SelectedIndex = i;
                        return;
                    }
                }
            }
            if (listeConnexions.Items.Count > 0)
                listeConnexions.SelectedIndex = 0;
        }

        // ----------------------------------------------------------------- Schémas

        private void boutonRafraichir_Click(object sender, EventArgs e)
        {
            ConnexionProfil profil = listeConnexions.SelectedItem as ConnexionProfil;
            if (profil == null)
            {
                Statut("Sélectionnez d'abord une connexion (menu Connexions pour en créer une).");
                return;
            }

            pProfilActif = profil;
            pProvider = ConnexionProviderFactory.Creer(profil);

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                IReadOnlyList<string> schemas = pProvider.ListerSchemas(profil);
                pSchemas = new List<string>(schemas);
                ConstruireArbre();
                Statut(pSchemas.Count + " schéma(s) chargé(s).");
            }
            catch (Exception ex)
            {
                arbreCatalogue.Nodes.Clear();
                grilleColonnes.DataSource = null;
                Statut("Échec du chargement des schémas : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void ConstruireArbre()
        {
            string filtre = (champFiltre.Text ?? string.Empty).Trim();
            arbreCatalogue.BeginUpdate();
            try
            {
                arbreCatalogue.Nodes.Clear();
                foreach (string schema in pSchemas)
                {
                    if (filtre.Length > 0 && schema.IndexOf(filtre, StringComparison.OrdinalIgnoreCase) < 0)
                        continue;

                    TreeNode noeud = new TreeNode(schema);
                    noeud.Tag = schema; // Tag de type string => nœud « schéma ».
                    noeud.Nodes.Add(new TreeNode(MarqueurChargement)); // Nœud fictif pour le « + » d'expansion.
                    arbreCatalogue.Nodes.Add(noeud);
                }
            }
            finally
            {
                arbreCatalogue.EndUpdate();
            }
        }

        private void champFiltre_TextChanged(object sender, EventArgs e)
        {
            if (pSchemas.Count > 0) ConstruireArbre();
        }

        // ----------------------------------------------------------------- Tables / colonnes

        private void arbreCatalogue_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            string schema = e.Node.Tag as string;
            if (schema == null) return;

            // Charge les tables au premier développement (le nœud ne contient alors que le marqueur).
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == MarqueurChargement)
                ChargerTables(e.Node, schema);
        }

        private void ChargerTables(TreeNode noeudSchema, string schema)
        {
            if (pProvider == null || pProfilActif == null) return;

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                IReadOnlyList<TableInfo> tables = pProvider.ListerTables(pProfilActif, schema);
                noeudSchema.Nodes.Clear();
                foreach (TableInfo table in tables)
                {
                    string texte = string.IsNullOrEmpty(table.Type) ? table.Nom : table.Nom + "  (" + table.Type + ")";
                    TreeNode noeudTable = new TreeNode(texte);
                    noeudTable.Tag = table; // Tag de type TableInfo => nœud « table ».
                    noeudSchema.Nodes.Add(noeudTable);
                }
                Statut(tables.Count + " table(s) dans " + schema + ".");
            }
            catch (Exception ex)
            {
                noeudSchema.Nodes.Clear();
                Statut("Échec du chargement des tables : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void arbreCatalogue_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TableInfo table = e.Node.Tag as TableInfo;
            if (table == null) return; // Nœud « schéma » : les grilles restent inchangées.
            ChargerStructure(table);
        }

        private void ChargerStructure(TableInfo table)
        {
            // EF-08 à EF-10 : colonnes, clés et relations de la table sélectionnée.
            if (pProvider == null || pProfilActif == null) return;

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                StructureTable structure = pProvider.DecrireTable(pProfilActif, table);
                grilleColonnes.DataSource = new List<ColonneInfo>(structure.Colonnes);
                grilleCles.DataSource = new List<CleInfo>(structure.Cles);
                grilleRelations.DataSource = LignesRelations(structure);
                etiquetteResume.Text = Resume(structure);

                string message = table.Schema + "." + table.Nom + " : " + structure.Colonnes.Count + " colonne(s), "
                    + structure.Cles.Count + " clé(s), "
                    + (structure.RelationsVersParents.Count + structure.RelationsDepuisEnfants.Count) + " relation(s).";
                if (structure.Avertissements.Count > 0)
                    message += " Description partielle : " + string.Join(" | ", structure.Avertissements);
                Statut(message);
            }
            catch (Exception ex)
            {
                grilleColonnes.DataSource = null;
                grilleCles.DataSource = null;
                grilleRelations.DataSource = null;
                etiquetteResume.Text = string.Empty;
                Statut("Échec du chargement de la table : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        /// <summary>Synthèse affichée au-dessus des onglets : libellé et clé d'identification.</summary>
        private static string Resume(StructureTable structure)
        {
            TableInfo table = structure.Table;
            string entete = table.Schema + "." + table.Nom;
            if (!string.IsNullOrEmpty(table.Libelle)) entete += " — " + table.Libelle;

            string cle;
            if (structure.CleIdentification != null)
                cle = "Clé d'identification : " + structure.CleIdentification.ListeColonnes
                    + " (" + structure.CleIdentification.LibelleOrigine.ToLowerInvariant() + " " + structure.CleIdentification.Nom + ")";
            else
                cle = "Aucune clé unique : les modifications ligne à ligne passeront par le numéro d'enregistrement RRN().";
            return entete + Environment.NewLine + cle;
        }

        /// <summary>Lignes de la grille des relations : d'abord vers les parents, puis depuis les enfants.</summary>
        private static List<LigneRelation> LignesRelations(StructureTable structure)
        {
            List<LigneRelation> lignes = new List<LigneRelation>();
            foreach (RelationInfo relation in structure.RelationsVersParents)
                lignes.Add(new LigneRelation("→ parent", relation.Nom, relation.Parent, relation.Correspondance, relation.RegleSuppression));
            foreach (RelationInfo relation in structure.RelationsDepuisEnfants)
                lignes.Add(new LigneRelation("← enfant", relation.Nom, relation.Enfant, relation.Correspondance, relation.RegleSuppression));
            return lignes;
        }

        /// <summary>Ligne d'affichage d'une relation, vue depuis la table sélectionnée.</summary>
        internal sealed class LigneRelation
        {
            public LigneRelation(string sens, string nom, string autreTable, string correspondance, string regleSuppression)
            {
                Sens = sens;
                Nom = nom;
                AutreTable = autreTable;
                Correspondance = correspondance;
                RegleSuppression = regleSuppression;
            }

            public string Sens { get; }
            public string Nom { get; }
            public string AutreTable { get; }
            public string Correspondance { get; }
            public string RegleSuppression { get; }
        }

        // ----------------------------------------------------------------- Statut

        private void Statut(string message)
        {
            etiquetteStatut.Text = message;
        }
    }
}

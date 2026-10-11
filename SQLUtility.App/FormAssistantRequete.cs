using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using SQLUtility.Core.Assistant;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Core.Dates;
using SQLUtility.Core.Execution;

namespace SQLUtility.App
{
    /// <summary>
    /// Assistant de requête. Niveau 1 (une table) : consultation guidée et ajout /
    /// modification / suppression d'une ligne désignée par sa clé ou son RRN (EF-40 à EF-43).
    /// Niveau 2 : consultation sur plusieurs tables jointes, A étant la table de départ (EF-44 / EF-45).
    /// Le SQL produit est rendu à l'éditeur (<see cref="SqlGenere"/>), jamais exécuté ici,
    /// sauf la relecture d'une ligne, faite en lecture seule.
    /// </summary>
    public partial class FormAssistantRequete : Form
    {
        private const string SansFormat = "(non)";

        private readonly ConnexionProfil pProfil;
        private readonly IConnexionProvider pProvider;
        private readonly string pSchemaInitial;

        /// <summary>Table principale (A) : celle de l'onglet Ligne et du FROM.</summary>
        private StructureTable pStructure;

        // Niveau 2 (EF-44) : tables de la consultation (A, B, C...) et jointures de B, C...
        private readonly List<TableRequete> pTables = new List<TableRequete>();
        private readonly List<Jointure> pJointures = new List<Jointure>();

        // Niveau 4 (EF-48 / EF-49) : conditions d'existence et traitement par lot.
        private readonly List<CritereExistence> pExistences = new List<CritereExistence>();
        private SourceMiseAJour pSourceLot;
        private StructureTable pCibleLot;
        private readonly Dictionary<string, ColonneInfo> pColonnesSourceLot = new Dictionary<string, ColonneInfo>();
        private readonly Dictionary<string, ColonneRequete> pColonnesCopieLot = new Dictionary<string, ColonneRequete>();

        private readonly Dictionary<string, ColonneRequete> pColonnesParLibelle = new Dictionary<string, ColonneRequete>();
        private readonly Dictionary<string, OperateurFiltre> pOperateurs = new Dictionary<string, OperateurFiltre>();
        private readonly Dictionary<string, FormatDate> pFormats = new Dictionary<string, FormatDate>();

        // Niveau 3 (EF-46 / EF-47) : calculs de la grille des colonnes et portée des filtres.
        private const string PorteeLignes = "les lignes (WHERE)";
        private readonly Dictionary<string, KeyValuePair<FonctionAgregat, PeriodeDate>> pCalculs = new Dictionary<string, KeyValuePair<FonctionAgregat, PeriodeDate>>();
        private readonly Dictionary<string, FonctionAgregat> pPortees = new Dictionary<string, FonctionAgregat>();
        private bool pRemplissage;

        public FormAssistantRequete(ConnexionProfil profil, IConnexionProvider provider, string schemaInitial)
        {
            if (profil == null) throw new ArgumentNullException(nameof(profil));
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            pProfil = profil;
            pProvider = provider;
            pSchemaInitial = schemaInitial;
            InitializeComponent();
            InitialiserListes();
            AppliquerMode();
            RemplirLot();
        }

        /// <summary>Instruction à insérer dans l'éditeur (renseignée quand la fenêtre se ferme par OK).</summary>
        public string SqlGenere { get; private set; }

        private void InitialiserListes()
        {
            AjouterOperateur("=", OperateurFiltre.Egal);
            AjouterOperateur("≠ (différent)", OperateurFiltre.Different);
            AjouterOperateur("<", OperateurFiltre.Inferieur);
            AjouterOperateur("≤", OperateurFiltre.InferieurOuEgal);
            AjouterOperateur(">", OperateurFiltre.Superieur);
            AjouterOperateur("≥", OperateurFiltre.SuperieurOuEgal);
            AjouterOperateur("entre", OperateurFiltre.Entre);
            AjouterOperateur("dans la liste", OperateurFiltre.Dans);
            AjouterOperateur("commence par", OperateurFiltre.Commence);
            AjouterOperateur("contient", OperateurFiltre.Contient);
            AjouterOperateur("finit par", OperateurFiltre.Finit);
            AjouterOperateur("est NULL", OperateurFiltre.EstNul);
            AjouterOperateur("n'est pas NULL", OperateurFiltre.NestPasNul);

            colFiltreFormat.Items.Add(SansFormat);
            colVFormat.Items.Add(SansFormat);
            colFormatC.Items.Add(SansFormat);
            colLFormat.Items.Add(SansFormat);
            foreach (DescriptionFormatDate description in ReferentielFormatsDate.Tous)
            {
                if (!description.Numerique || !description.Implemente) continue;
                pFormats[description.Libelle] = description.Format;
                colFiltreFormat.Items.Add(description.Libelle);
                colVFormat.Items.Add(description.Libelle);
                colFormatC.Items.Add(description.Libelle);
                colLFormat.Items.Add(description.Libelle);
            }
            // ISO stocké en numérique (AAAAMMJJ) : format DECIMAL(8,0) fréquent hors Atlas.
            string libelleIso = "AAAAMMJJ (numérique)";
            pFormats[libelleIso] = FormatDate.Iso;
            colFiltreFormat.Items.Add(libelleIso);
            colVFormat.Items.Add(libelleIso);
            colFormatC.Items.Add(libelleIso);
            colLFormat.Items.Add(libelleIso);

            colCalcul.Items.Add(string.Empty);
            AjouterCalcul("Regrouper", FonctionAgregat.Regrouper, PeriodeDate.Aucune);
            AjouterCalcul("Regrouper par année", FonctionAgregat.Regrouper, PeriodeDate.Annee);
            AjouterCalcul("Regrouper par mois", FonctionAgregat.Regrouper, PeriodeDate.Mois);
            AjouterCalcul("Regrouper par jour", FonctionAgregat.Regrouper, PeriodeDate.Jour);
            AjouterCalcul("Somme", FonctionAgregat.Somme, PeriodeDate.Aucune);
            AjouterCalcul("Nombre", FonctionAgregat.Nombre, PeriodeDate.Aucune);
            AjouterCalcul("Nombre distinct", FonctionAgregat.NombreDistinct, PeriodeDate.Aucune);
            AjouterCalcul("Minimum", FonctionAgregat.Minimum, PeriodeDate.Aucune);
            AjouterCalcul("Maximum", FonctionAgregat.Maximum, PeriodeDate.Aucune);
            AjouterCalcul("Moyenne", FonctionAgregat.Moyenne, PeriodeDate.Aucune);

            colFiltrePorte.Items.Add(PorteeLignes);
            AjouterPortee("la somme (HAVING)", FonctionAgregat.Somme);
            AjouterPortee("le nombre (HAVING)", FonctionAgregat.Nombre);
            AjouterPortee("le nombre distinct (HAVING)", FonctionAgregat.NombreDistinct);
            AjouterPortee("le minimum (HAVING)", FonctionAgregat.Minimum);
            AjouterPortee("le maximum (HAVING)", FonctionAgregat.Maximum);
            AjouterPortee("la moyenne (HAVING)", FonctionAgregat.Moyenne);
            AjouterPortee("le nombre de lignes du groupe (HAVING)", FonctionAgregat.NombreLignes);
        }

        private void AjouterCalcul(string libelle, FonctionAgregat fonction, PeriodeDate periode)
        {
            pCalculs[libelle] = new KeyValuePair<FonctionAgregat, PeriodeDate>(fonction, periode);
            colCalcul.Items.Add(libelle);
        }

        private void AjouterPortee(string libelle, FonctionAgregat fonction)
        {
            pPortees[libelle] = fonction;
            colFiltrePorte.Items.Add(libelle);
        }

        private void AjouterOperateur(string libelle, OperateurFiltre operateur)
        {
            pOperateurs[libelle] = operateur;
            colFiltreOperateur.Items.Add(libelle);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ChargerSchemas();
        }

        // ----------------------------------------------------------------- Schéma et table

        private void ChargerSchemas()
        {
            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                listeSchemas.Items.Clear();
                foreach (string schema in pProvider.ListerSchemas(pProfil)) listeSchemas.Items.Add(schema);
                if (!string.IsNullOrEmpty(pSchemaInitial))
                {
                    int index = listeSchemas.FindStringExact(pSchemaInitial);
                    if (index >= 0)
                    {
                        listeSchemas.SelectedIndex = index;
                        ChargerTables();
                    }
                }
                listeTables.Focus();
            }
            catch (Exception ex)
            {
                Info("Schémas indisponibles : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void listeSchemas_SelectionChangeCommitted(object sender, EventArgs e)
        {
            BeginInvoke((Action)ChargerTables); // Après la mise à jour du texte de la liste.
        }

        private void listeSchemas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            ChargerTables();
            e.SuppressKeyPress = true;
        }

        private void ChargerTables()
        {
            string schema = (listeSchemas.Text ?? string.Empty).Trim().ToUpperInvariant();
            listeTables.Items.Clear();
            if (schema.Length == 0) return;

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                IReadOnlyList<TableInfo> tables = pProvider.ListerTables(pProfil, schema);
                listeTables.BeginUpdate();
                foreach (TableInfo table in tables) listeTables.Items.Add(new ElementTable(table));
                listeTables.EndUpdate();
                Info(tables.Count + " table(s) dans " + schema + " : tapez le début du nom.");
            }
            catch (Exception ex)
            {
                Info("Tables indisponibles : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void listeTables_SelectionChangeCommitted(object sender, EventArgs e)
        {
            BeginInvoke((Action)ChargerTable);
        }

        private void listeTables_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            ChargerTable();
            e.SuppressKeyPress = true;
        }

        private void boutonChargerTable_Click(object sender, EventArgs e)
        {
            ChargerTable();
        }

        private void ChargerTable()
        {
            TableInfo table = TableSaisie();
            if (table == null)
            {
                Info("Table introuvable dans la liste : vérifiez le schéma et le nom.");
                return;
            }

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                pStructure = pProvider.DecrireTable(pProfil, table);
                RemplirGrilles();
                Info(DescriptionTable(pStructure));
            }
            catch (Exception ex)
            {
                pStructure = null;
                Info("Échec du chargement de " + table.Nom + " : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
            AppliquerMode();
            Regenerer();
        }

        /// <summary>Table choisie ou tapée (le nom seul suffit, sans le libellé).</summary>
        private TableInfo TableSaisie()
        {
            ElementTable choisi = listeTables.SelectedItem as ElementTable;
            string texte = (listeTables.Text ?? string.Empty).Trim();
            if (choisi != null && texte == choisi.ToString()) return choisi.Table;

            string nom = texte;
            int separateur = nom.IndexOf(ElementTable.Separateur, StringComparison.Ordinal);
            if (separateur > 0) nom = nom.Substring(0, separateur).Trim();
            foreach (object element in listeTables.Items)
            {
                ElementTable candidat = (ElementTable)element;
                if (string.Equals(candidat.Table.Nom, nom, StringComparison.OrdinalIgnoreCase)) return candidat.Table;
            }
            return null;
        }

        private static string DescriptionTable(StructureTable structure)
        {
            TableInfo table = structure.Table;
            string texte = table.Schema + "." + table.Nom;
            if (!string.IsNullOrEmpty(table.Libelle)) texte += " — " + table.Libelle;
            texte += " — " + structure.Colonnes.Count + " colonne(s) — ";
            texte += structure.CleIdentification != null
                ? "clé : " + structure.CleIdentification.ListeColonnes
                : "aucune clé unique (lignes désignées par RRN)";
            if (structure.Avertissements.Count > 0) texte += " — description partielle";
            return texte;
        }

        private void RemplirGrilles()
        {
            pRemplissage = true;
            try
            {
                grilleChoixColonnes.Rows.Clear();
                grilleFiltres.Rows.Clear();
                grilleValeurs.Rows.Clear();
                colFiltreColonne.Items.Clear();
                pColonnesParLibelle.Clear();
                champRrn.Text = string.Empty;

                // Nouvelle table principale : la consultation repart d'une seule table.
                pTables.Clear();
                pJointures.Clear();
                pExistences.Clear();
                pSourceLot = null;
                pCibleLot = null;
                pTables.Add(new TableRequete(pStructure, "A"));
                AjouterColonnesConsultation(pTables[0]);
                RafraichirTables();
                RemplirLot();

                foreach (ColonneInfo colonne in pStructure.Colonnes)
                {
                    string type = TypesSql.Libelle(colonne.TypeDonnee, colonne.Longueur, colonne.Echelle);
                    int ligne = grilleValeurs.Rows.Add(colonne.Nom, colonne.Libelle, type, colonne.RangCleTexte, "", "", false, FormatPropose(colonne));
                    DataGridViewRow rangee = grilleValeurs.Rows[ligne];
                    rangee.Tag = colonne;
                    if (!colonne.AccepteNull) rangee.Cells[colVNull.Index].ReadOnly = true;
                    if (TypesSql.Categorie(colonne.TypeDonnee) == CategorieType.NonPrisEnCharge)
                    {
                        rangee.ReadOnly = true;
                        rangee.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.GrayText;
                    }
                }
            }
            finally
            {
                pRemplissage = false;
            }
        }

        /// <summary>Lignes de la grille des colonnes et entrées de la liste des filtres pour une table.</summary>
        private void AjouterColonnesConsultation(TableRequete table)
        {
            bool etat = pRemplissage;
            pRemplissage = true;
            try
            {
                foreach (ColonneInfo colonne in table.Structure.Colonnes)
                {
                    string type = TypesSql.Libelle(colonne.TypeDonnee, colonne.Longueur, colonne.Echelle);
                    int ligne = grilleChoixColonnes.Rows.Add(false, table.Alias, colonne.Nom, colonne.Libelle, type, colonne.RangCleTexte, "", "",
                        string.Empty, FormatPropose(colonne));
                    grilleChoixColonnes.Rows[ligne].Tag = new ColonneRequete(table, colonne);

                    string libelle = LibelleColonne(table, colonne);
                    pColonnesParLibelle[libelle] = new ColonneRequete(table, colonne);
                    colFiltreColonne.Items.Add(libelle);
                }
            }
            finally
            {
                pRemplissage = etat;
            }
        }

        /// <summary>
        /// Libellé d'une colonne dans la liste des filtres : sans préfixe pour la table A (le
        /// niveau 1 reste inchangé), préfixé par l'alias pour les tables jointes.
        /// </summary>
        private string LibelleColonne(TableRequete table, ColonneInfo colonne)
        {
            string libelle = string.IsNullOrEmpty(colonne.Libelle) ? colonne.Nom : colonne.Nom + " — " + colonne.Libelle;
            return pTables.Count > 0 && table == pTables[0] ? libelle : table.Alias + "." + libelle;
        }

        // ----------------------------------------------------------------- Jointures (niveau 2, EF-44 / EF-45)

        private void boutonJoindre_Click(object sender, EventArgs e)
        {
            if (pStructure == null)
            {
                Info("Chargez d'abord la table principale (A), puis choisissez la table à joindre.");
                return;
            }
            TableInfo table = TableSaisie();
            if (table == null)
            {
                Info("Choisissez dans la liste la table à joindre.");
                return;
            }

            StructureTable structure;
            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                structure = pProvider.DecrireTable(pProfil, table);
            }
            catch (Exception ex)
            {
                Info("Échec du chargement de " + table.Nom + " : " + ex.Message);
                return;
            }
            finally
            {
                Cursor = curseur;
            }

            TableRequete nouvelle = new TableRequete(structure, AliasLibre());
            using (FormJointure dialogue = new FormJointure(pTables, nouvelle, null))
            {
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                pTables.Add(nouvelle);
                pJointures.Add(dialogue.Jointure);
            }
            AjouterColonnesConsultation(nouvelle);
            RafraichirTables();
            onglets.SelectedTab = ongletConsulter;
            Info(DescriptionTable(pStructure) + " — " + pTables.Count + " tables jointes.");
            Regenerer();
        }

        private void grilleTables_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0) ModifierLiaison(e.RowIndex);
        }

        private void boutonModifierJointure_Click(object sender, EventArgs e)
        {
            int index = grilleTables.CurrentRow != null ? grilleTables.CurrentRow.Index : -1;
            if (index < 1)
            {
                Info("Sélectionnez une table jointe (B, C...) ou une condition d'existence (S1...) : la table A n'a pas de liaison.");
                return;
            }
            ModifierLiaison(index);
        }

        /// <summary>Lignes de la liste : tables (A, jointures), puis conditions d'existence.</summary>
        private void ModifierLiaison(int index)
        {
            if (index < pTables.Count) ModifierJointure(index);
            else ModifierExistence(index - pTables.Count);
        }

        // ----------------------------------------------------------------- Conditions d'existence (niveau 4, EF-48)

        private void boutonExistence_Click(object sender, EventArgs e)
        {
            if (pStructure == null)
            {
                Info("Chargez d'abord la table principale (A), puis choisissez la table de la condition d'existence.");
                return;
            }
            StructureTable structure = DecrireTableSaisie("de la condition d'existence");
            if (structure == null) return;

            TableRequete table = new TableRequete(structure, AliasExistence());
            using (FormJointure dialogue = new FormJointure(pTables, table, null, ModeLiaison.Existence))
            {
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                pExistences.Add(ExistenceDepuis(table, dialogue.Jointure));
            }
            AjouterColonnesExistence(table);
            RafraichirTables();
            onglets.SelectedTab = ongletConsulter;
            Info("Condition d'existence " + table.Alias + " ajoutée : ses colonnes sont proposées dans les filtres (préfixe " + table.Alias + ".).");
            Regenerer();
        }

        private void ModifierExistence(int index)
        {
            CritereExistence actuelle = pExistences[index];
            Jointure transport = new Jointure(actuelle.Table, actuelle.Negation ? TypeJointure.ExterneGauche : TypeJointure.Interne, OrigineJointure.Manuelle);
            transport.Conditions.AddRange(actuelle.Conditions);
            using (FormJointure dialogue = new FormJointure(pTables, actuelle.Table, transport, ModeLiaison.Existence))
            {
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                pExistences[index] = ExistenceDepuis(actuelle.Table, dialogue.Jointure);
            }
            RafraichirTables();
            Regenerer();
        }

        /// <summary>Le type de la liaison porte la négation : LEFT (« sans correspondance ») = NOT EXISTS.</summary>
        private static CritereExistence ExistenceDepuis(TableRequete table, Jointure liaison)
        {
            CritereExistence existence = new CritereExistence(table, liaison.Type == TypeJointure.ExterneGauche);
            existence.Conditions.AddRange(liaison.Conditions);
            return existence;
        }

        /// <summary>Colonnes de la sous-requête : proposées dans les filtres seulement (jamais affichées).</summary>
        private void AjouterColonnesExistence(TableRequete table)
        {
            foreach (ColonneInfo colonne in table.Structure.Colonnes)
            {
                string libelle = LibelleColonne(table, colonne);
                pColonnesParLibelle[libelle] = new ColonneRequete(table, colonne);
                colFiltreColonne.Items.Add(libelle);
            }
        }

        private void RetirerExistence(int index)
        {
            TableRequete table = pExistences[index].Table;
            pRemplissage = true;
            try
            {
                RetirerFiltresEtLibelles(table);
                pExistences.RemoveAt(index);
            }
            finally
            {
                pRemplissage = false;
            }
            RafraichirTables();
            Regenerer();
        }

        /// <summary>Enlève les filtres portant sur la table et ses entrées dans la liste des filtres.</summary>
        private void RetirerFiltresEtLibelles(TableRequete table)
        {
            for (int i = grilleFiltres.Rows.Count - 1; i >= 0; i--)
            {
                ColonneRequete colonne = ColonneDuFiltre(grilleFiltres.Rows[i]);
                if (!grilleFiltres.Rows[i].IsNewRow && colonne != null && colonne.Table == table) grilleFiltres.Rows.RemoveAt(i);
            }
            foreach (ColonneInfo colonne in table.Structure.Colonnes)
            {
                string libelle = LibelleColonne(table, colonne);
                pColonnesParLibelle.Remove(libelle);
                colFiltreColonne.Items.Remove(libelle);
            }
        }

        private bool EstTableExistence(TableRequete table)
        {
            foreach (CritereExistence existence in pExistences)
                if (existence.Table == table) return true;
            return false;
        }

        /// <summary>
        /// Conditions d'existence prêtes à générer : les filtres saisis sur les colonnes S1, S2...
        /// sont placés dans leur sous-requête.
        /// </summary>
        private List<CritereExistence> ExistencesSaisies()
        {
            List<CritereExistence> existences = new List<CritereExistence>();
            foreach (CritereExistence modele in pExistences)
            {
                CritereExistence existence = new CritereExistence(modele.Table, modele.Negation);
                existence.Conditions.AddRange(modele.Conditions);
                foreach (CritereFiltre critere in CriteresDesFiltres(true))
                    if (critere.Table == modele.Table) existence.Criteres.Add(critere);
                existences.Add(existence);
            }
            return existences;
        }

        /// <summary>Premier alias libre pour une sous-requête : S1, S2...</summary>
        private string AliasExistence()
        {
            for (int i = 1; ; i++)
            {
                string alias = "S" + i;
                if (!pExistences.Exists(x => string.Equals(x.Table.Alias, alias, StringComparison.OrdinalIgnoreCase))) return alias;
            }
        }

        /// <summary>Table choisie dans la liste du haut, décrite par le catalogue ; null (et message) sinon.</summary>
        private StructureTable DecrireTableSaisie(string role)
        {
            TableInfo table = TableSaisie();
            if (table == null)
            {
                Info("Choisissez dans la liste « Table » en haut la table " + role + ".");
                return null;
            }
            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                return pProvider.DecrireTable(pProfil, table);
            }
            catch (Exception ex)
            {
                Info("Échec du chargement de " + table.Nom + " : " + ex.Message);
                return null;
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void ModifierJointure(int index)
        {
            Jointure actuelle = pJointures[index - 1];
            using (FormJointure dialogue = new FormJointure(pTables.GetRange(0, index), actuelle.Table, actuelle))
            {
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                pJointures[index - 1] = dialogue.Jointure;
            }
            RafraichirTables();
            Regenerer();
        }

        private void boutonRetirerTable_Click(object sender, EventArgs e)
        {
            int index = grilleTables.CurrentRow != null ? grilleTables.CurrentRow.Index : -1;
            if (index < 1)
            {
                Info("Sélectionnez une table jointe ou une condition d'existence à retirer (la table A se change avec « Charger »).");
                return;
            }
            if (index >= pTables.Count)
            {
                RetirerExistence(index - pTables.Count);
                return;
            }
            TableRequete table = pTables[index];
            List<string> dependants = new List<string>();
            foreach (Jointure autre in pJointures)
            {
                if (autre.Table == table) continue;
                foreach (ConditionJointure condition in autre.Conditions)
                    if (condition.TableLiee == table && !dependants.Contains(autre.Table.Alias)) dependants.Add(autre.Table.Alias);
            }
            foreach (CritereExistence existence in pExistences)
            {
                foreach (ConditionJointure condition in existence.Conditions)
                    if (condition.TableLiee == table && !dependants.Contains(existence.Table.Alias)) dependants.Add(existence.Table.Alias);
            }
            if (dependants.Count > 0)
            {
                Info("Impossible de retirer " + table.Alias + " : " + string.Join(", ", dependants) + " s'appuie dessus.");
                return;
            }

            pRemplissage = true;
            try
            {
                for (int i = grilleChoixColonnes.Rows.Count - 1; i >= 0; i--)
                    if (((ColonneRequete)grilleChoixColonnes.Rows[i].Tag).Table == table) grilleChoixColonnes.Rows.RemoveAt(i);
                RetirerFiltresEtLibelles(table);
                pJointures.RemoveAt(index - 1);
                pTables.RemoveAt(index);
            }
            finally
            {
                pRemplissage = false;
            }
            RafraichirTables();
            Regenerer();
        }

        /// <summary>Premier alias libre : A, B, C... puis T27, T28...</summary>
        private string AliasLibre()
        {
            for (int i = 0; ; i++)
            {
                string alias = i < 26 ? ((char)('A' + i)).ToString() : "T" + (i + 1);
                if (!pTables.Exists(t => string.Equals(t.Alias, alias, StringComparison.OrdinalIgnoreCase))) return alias;
            }
        }

        private void RafraichirTables()
        {
            grilleTables.Rows.Clear();
            for (int i = 0; i < pTables.Count; i++)
            {
                TableRequete table = pTables[i];
                string nom = table.Table.Schema + "." + table.Table.Nom + (string.IsNullOrEmpty(table.Table.Libelle) ? string.Empty : " — " + table.Table.Libelle);
                if (i == 0)
                {
                    grilleTables.Rows.Add(table.Alias, nom, "table de départ", string.Empty);
                    continue;
                }
                Jointure jointure = pJointures[i - 1];
                string type = jointure.Type == TypeJointure.ExterneGauche ? "LEFT OUTER" : "INNER";
                if (jointure.Origine == OrigineJointure.ClePartielleDeduite) type += " (clé partielle)";
                else if (jointure.Origine == OrigineJointure.Manuelle) type += " (manuelle)";
                grilleTables.Rows.Add(table.Alias, nom, type, jointure.DescriptionConditions);
            }
            foreach (CritereExistence existence in pExistences)
            {
                TableInfo info = existence.Table.Table;
                string nom = info.Schema + "." + info.Nom + (string.IsNullOrEmpty(info.Libelle) ? string.Empty : " — " + info.Libelle);
                grilleTables.Rows.Add(existence.Table.Alias, nom, existence.Negation ? "NOT EXISTS (aucune ligne liée)" : "EXISTS (au moins une ligne liée)",
                    existence.DescriptionConditions);
            }
            boutonModifierJointure.Enabled = boutonRetirerTable.Enabled = pTables.Count > 1 || pExistences.Count > 0;
            // Les colonnes copiables dépendent des tables de la consultation.
            if (radioLotCopier.Checked) RemplirLot();
        }

        /// <summary>
        /// Format de date proposé : seulement pour un numérique de longueur compatible dont le
        /// nom ou le libellé évoque une date (une quantité DECIMAL(7,0) n'en est pas une).
        /// </summary>
        private string FormatPropose(ColonneInfo colonne)
        {
            ResultatDetectionDate detection = DetecteurFormatDate.Detecter(colonne);
            if (!detection.EstProbablementDate) return SansFormat;
            string indices = ((colonne.Nom ?? string.Empty) + " " + (colonne.Libelle ?? string.Empty)).ToUpperInvariant();
            if (indices.IndexOf("DAT", StringComparison.Ordinal) < 0 && indices.IndexOf("DTE", StringComparison.Ordinal) < 0)
                return SansFormat;
            if (detection.FormatParDefaut == FormatDate.Iso) return "AAAAMMJJ (numérique)";
            return ReferentielFormatsDate.Obtenir(detection.FormatParDefaut).Libelle;
        }

        // ----------------------------------------------------------------- Événements de saisie

        private void Saisie_Changee(object sender, EventArgs e)
        {
            Regenerer();
        }

        private void Grille_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // Cases à cocher et listes : valider tout de suite pour régénérer en direct.
            DataGridView grille = (DataGridView)sender;
            if (grille.IsCurrentCellDirty && !(grille.CurrentCell is DataGridViewTextBoxCell))
                grille.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void Grille_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Regenerer();
        }

        private void grilleFiltres_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (pRemplissage || e.RowIndex < 0) return;
            DataGridViewRow ligne = grilleFiltres.Rows[e.RowIndex];

            // Colonne choisie : opérateur « = » et format de date proposés par défaut.
            if (e.ColumnIndex == colFiltreColonne.Index)
            {
                ColonneRequete colonne = ColonneDuFiltre(ligne);
                pRemplissage = true;
                try
                {
                    if (ligne.Cells[colFiltreOperateur.Index].Value == null) ligne.Cells[colFiltreOperateur.Index].Value = "=";
                    if (ligne.Cells[colFiltrePorte.Index].Value == null) ligne.Cells[colFiltrePorte.Index].Value = PorteeLignes;
                    if (colonne != null) ligne.Cells[colFiltreFormat.Index].Value = FormatPropose(colonne.Colonne);
                }
                finally
                {
                    pRemplissage = false;
                }
            }
            Regenerer();
        }

        private void grilleFiltres_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            Regenerer();
        }

        private void Grille_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false; // Valeur de liste obsolète après changement de table.
        }

        private void boutonToutCocher_Click(object sender, EventArgs e)
        {
            CocherTout(true);
        }

        private void boutonToutDecocher_Click(object sender, EventArgs e)
        {
            CocherTout(false);
        }

        private void CocherTout(bool valeur)
        {
            pRemplissage = true;
            try
            {
                foreach (DataGridViewRow ligne in grilleChoixColonnes.Rows) ligne.Cells[colAfficher.Index].Value = valeur;
            }
            finally
            {
                pRemplissage = false;
            }
            Regenerer();
        }

        // ----------------------------------------------------------------- Mode ligne

        private void Mode_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (!radio.Checked) return; // Un seul passage par changement (le bouton qui s'active).
            AppliquerMode();
            Regenerer();
        }

        private void AppliquerMode()
        {
            bool ajout = radioAjouter.Checked;
            bool suppression = radioSupprimer.Checked;

            colVActuelle.Visible = !ajout;
            colVNouvelle.Visible = !suppression;
            colVNull.Visible = !suppression;
            colVNouvelle.HeaderText = ajout ? "Valeur" : "Nouvelle valeur";
            etiquetteRrn.Visible = champRrn.Visible = boutonChargerLigne.Visible = !ajout;

            CleInfo cle = pStructure != null ? pStructure.CleIdentification : null;
            foreach (DataGridViewRow ligne in grilleValeurs.Rows)
            {
                ColonneInfo colonne = (ColonneInfo)ligne.Tag;
                if (ligne.ReadOnly) continue;
                // « Valeur actuelle » : saisissable pour les colonnes de clé (désignation), lue sinon.
                ligne.Cells[colVActuelle.Index].ReadOnly = colonne.RangCle == 0;
                ligne.Cells[colVActuelle.Index].Style.BackColor = colonne.RangCle > 0 && !ajout
                    ? System.Drawing.Color.FromArgb(255, 250, 225) : System.Drawing.Color.Empty;
            }

            if (ajout)
                etiquetteAideLigne.Text = "Saisissez les valeurs ; une colonne laissée vide prend sa valeur par défaut. Dates : jj/mm/aaaa.";
            else if (cle != null)
                etiquetteAideLigne.Text = "Désignez la ligne par sa clé (" + cle.ListeColonnes + ") dans « Valeur actuelle / clé », ou par son RRN, puis « Charger la ligne »."
                    + (suppression ? string.Empty : " Seules les valeurs changées sont modifiées (vide = inchangé, un espace = blanc).");
            else
                etiquetteAideLigne.Text = "Pas de clé unique : désignez la ligne par son RRN (affichez-le avec « Inclure le n° d'enregistrement » dans l'onglet Consulter).";
        }

        private void boutonChargerLigne_Click(object sender, EventArgs e)
        {
            if (pStructure == null) return;
            List<string> erreurs = new List<string>();
            IdentificationLigne identification = Identification(erreurs);
            if (identification == null || erreurs.Count > 0)
            {
                Erreurs(erreurs);
                return;
            }

            List<ColonneInfo> lisibles = new List<ColonneInfo>();
            foreach (ColonneInfo colonne in pStructure.Colonnes)
                if (TypesSql.Categorie(colonne.TypeDonnee) != CategorieType.NonPrisEnCharge) lisibles.Add(colonne);

            ResultatGeneration lecture = GenerateurSqlSimple.ConstruireLectureLigne(pStructure.Table, lisibles, identification);
            if (!lecture.EstValide)
            {
                Erreurs(lecture.Erreurs);
                return;
            }

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                ResultatRequete resultat = pProvider.ExecuterSelect(pProfil, lecture.Sql, 2);
                if (resultat.Lignes.Count == 0)
                {
                    Erreurs(new[] { "Aucune ligne ne correspond à cette désignation." });
                    return;
                }
                RemplirValeursActuelles(lisibles, resultat.Lignes[0]);
                if (resultat.Lignes.Count > 1)
                    Erreurs(new[] { "ATTENTION : plusieurs lignes correspondent — la désignation n'est pas unique." });
                else
                    Regenerer();
            }
            catch (Exception ex)
            {
                Erreurs(new[] { "Lecture impossible : " + ex.Message });
            }
            finally
            {
                Cursor = curseur;
            }
        }

        /// <summary>Valeurs lues : affichées en « actuelle » et recopiées en « nouvelle », à modifier sur place.</summary>
        private void RemplirValeursActuelles(List<ColonneInfo> colonnes, object[] valeurs)
        {
            pRemplissage = true;
            try
            {
                foreach (DataGridViewRow ligne in grilleValeurs.Rows)
                {
                    int index = colonnes.IndexOf((ColonneInfo)ligne.Tag);
                    if (index < 0) continue;
                    string texte = TexteValeur(valeurs[index]);
                    ligne.Cells[colVActuelle.Index].Value = texte;
                    ligne.Cells[colVNouvelle.Index].Value = texte;
                    ligne.Cells[colVNull.Index].Value = false;
                }
            }
            finally
            {
                pRemplissage = false;
            }
        }

        /// <summary>Valeur lue, dans la forme attendue en saisie (dates jj/mm/aaaa, nombres en culture locale).</summary>
        internal static string TexteValeur(object valeur)
        {
            if (valeur == null || valeur is DBNull) return string.Empty;
            if (valeur is DateTime)
            {
                DateTime instant = (DateTime)valeur;
                return instant.TimeOfDay == TimeSpan.Zero
                    ? instant.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
                    : instant.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            if (valeur is TimeSpan) return ((TimeSpan)valeur).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            if (valeur is string) return ((string)valeur).TrimEnd(); // CHAR complété d'espaces.
            IFormattable formattable = valeur as IFormattable;
            return formattable != null ? formattable.ToString(null, CultureInfo.CurrentCulture) : valeur.ToString();
        }

        // ----------------------------------------------------------------- Génération

        private void Regenerer()
        {
            if (pRemplissage) return;
            if (pStructure == null)
            {
                apercuSql.Text = string.Empty;
                Erreurs(new string[0]);
                return;
            }

            ResultatGeneration resultat;
            if (onglets.SelectedTab == ongletConsulter) resultat = GenererConsultation();
            else if (onglets.SelectedTab == ongletLot) resultat = GenererLot();
            else resultat = GenererLigne();
            apercuSql.Text = resultat.Sql ?? string.Empty;
            Erreurs(resultat.Erreurs, resultat.Avertissements);
        }

        /// <summary>
        /// Consultation : une seule table = SQL du niveau 1 (colonnes non qualifiées) ;
        /// dès qu'une table est jointe = SQL du niveau 2 (colonnes qualifiées par alias).
        /// </summary>
        private ResultatGeneration GenererConsultation()
        {
            if (ModeAgregat()) return GenererAgregat();

            List<ColonneRequete> colonnes = new List<ColonneRequete>();
            List<CritereTri> tris = TrisSaisis(colonnes);
            List<CritereFiltre> criteres = CriteresSaisis();

            if (pJointures.Count == 0 && pExistences.Count == 0)
            {
                RequeteConsultation simple = new RequeteConsultation();
                simple.Table = pStructure.Table;
                simple.ToutesColonnes = new List<ColonneInfo>(pStructure.Colonnes);
                simple.Distinct = caseDistinct.Checked;
                simple.AvecRrn = caseRrn.Checked;
                simple.Limite = (int)champLimite.Value;
                foreach (ColonneRequete colonne in colonnes) simple.Colonnes.Add(colonne.Colonne);
                simple.Tris.AddRange(tris);
                simple.Criteres.AddRange(criteres);
                return GenerateurSqlSimple.ConstruireSelect(simple);
            }

            RequeteJointure requete = new RequeteJointure(pTables[0]);
            requete.Jointures.AddRange(pJointures);
            requete.Existences.AddRange(ExistencesSaisies());
            requete.Colonnes.AddRange(colonnes);
            requete.Tris.AddRange(tris);
            requete.Criteres.AddRange(criteres);
            requete.Distinct = caseDistinct.Checked;
            requete.Limite = (int)champLimite.Value;
            ResultatGeneration resultat = GenerateurSqlJointure.ConstruireSelect(requete);
            if (!caseRrn.Checked) return resultat;

            // RRN() n'a de sens que pour une table seule : on le signale sans bloquer.
            List<string> avertissements = new List<string>(resultat.Avertissements);
            avertissements.Add("Le n° d'enregistrement (RRN) n'est proposé que pour une table seule : option ignorée.");
            return new ResultatGeneration(resultat.Sql, new List<string>(resultat.Erreurs), avertissements);
        }

        /// <summary>Colonnes cochées (remplit <paramref name="colonnes"/>) et tris ordonnés par rang.</summary>
        private List<CritereTri> TrisSaisis(List<ColonneRequete> colonnes)
        {
            List<KeyValuePair<int, CritereTri>> tris = new List<KeyValuePair<int, CritereTri>>();
            foreach (DataGridViewRow ligne in grilleChoixColonnes.Rows)
            {
                ColonneRequete colonne = (ColonneRequete)ligne.Tag;
                if (true.Equals(ligne.Cells[colAfficher.Index].Value)) colonnes.Add(colonne);

                string sens = ligne.Cells[colTri.Index].Value as string;
                if (string.IsNullOrEmpty(sens)) continue;
                int rang;
                if (!int.TryParse(Convert.ToString(ligne.Cells[colRangTri.Index].Value), out rang)) rang = 1000 + ligne.Index;
                CritereTri tri = new CritereTri(colonne.Colonne, sens == "Décroissant");
                tri.Table = colonne.Table;
                tris.Add(new KeyValuePair<int, CritereTri>(rang, tri));
            }
            // Tri stable par rang (List.Sort ne l'est pas : on départage par l'ordre d'origine).
            List<KeyValuePair<int, KeyValuePair<int, CritereTri>>> ordonnes = new List<KeyValuePair<int, KeyValuePair<int, CritereTri>>>();
            for (int i = 0; i < tris.Count; i++) ordonnes.Add(new KeyValuePair<int, KeyValuePair<int, CritereTri>>(i, tris[i]));
            ordonnes.Sort(delegate (KeyValuePair<int, KeyValuePair<int, CritereTri>> a, KeyValuePair<int, KeyValuePair<int, CritereTri>> b)
            {
                int comparaison = a.Value.Key.CompareTo(b.Value.Key);
                return comparaison != 0 ? comparaison : a.Key.CompareTo(b.Key);
            });
            List<CritereTri> resultat = new List<CritereTri>();
            foreach (KeyValuePair<int, KeyValuePair<int, CritereTri>> tri in ordonnes) resultat.Add(tri.Value.Value);
            return resultat;
        }

        // ----------------------------------------------------------------- Agrégats (niveau 3, EF-46 / EF-47)

        /// <summary>Mode agrégat : un calcul choisi, le nombre de lignes demandé, ou un filtre sur un calcul.</summary>
        private bool ModeAgregat()
        {
            if (caseCompter.Checked) return true;
            foreach (DataGridViewRow ligne in grilleChoixColonnes.Rows)
                if (!string.IsNullOrEmpty(ligne.Cells[colCalcul.Index].Value as string)) return true;
            foreach (DataGridViewRow ligne in grilleFiltres.Rows)
                if (!ligne.IsNewRow && PorteeDuFiltre(ligne).HasValue) return true;
            return false;
        }

        /// <summary>
        /// Requête GROUP BY : les colonnes avec un calcul, plus les colonnes cochées sans calcul
        /// (regroupées). Les filtres « sur les lignes » vont au WHERE, ceux sur un calcul au HAVING.
        /// </summary>
        private ResultatGeneration GenererAgregat()
        {
            RequeteJointure baseRequete = new RequeteJointure(pTables[0]);
            baseRequete.Jointures.AddRange(pJointures);
            baseRequete.Existences.AddRange(ExistencesSaisies());
            baseRequete.Criteres.AddRange(CriteresSaisis());
            baseRequete.Limite = (int)champLimite.Value;
            RequeteAgregat requete = new RequeteAgregat(baseRequete);

            List<KeyValuePair<int, TriAgregat>> tris = new List<KeyValuePair<int, TriAgregat>>();
            foreach (DataGridViewRow ligne in grilleChoixColonnes.Rows)
            {
                ColonneRequete colonne = (ColonneRequete)ligne.Tag;
                string calcul = ligne.Cells[colCalcul.Index].Value as string;
                ElementAgregat element = null;
                KeyValuePair<FonctionAgregat, PeriodeDate> choix;
                if (!string.IsNullOrEmpty(calcul) && pCalculs.TryGetValue(calcul, out choix))
                {
                    element = new ElementAgregat(colonne.Table, colonne.Colonne, choix.Key);
                    element.Periode = choix.Value;
                }
                else if (true.Equals(ligne.Cells[colAfficher.Index].Value))
                {
                    element = new ElementAgregat(colonne.Table, colonne.Colonne, FonctionAgregat.Regrouper);
                }
                if (element != null)
                {
                    element.FormatDate = FormatChoisi(ligne.Cells[colFormatC.Index].Value);
                    requete.Elements.Add(element);
                }

                string sens = ligne.Cells[colTri.Index].Value as string;
                if (string.IsNullOrEmpty(sens)) continue;
                int rang;
                if (!int.TryParse(Convert.ToString(ligne.Cells[colRangTri.Index].Value), out rang)) rang = 1000 + ligne.Index;
                // Sans élément, le générateur signale qu'on ne trie que sur le résultat.
                tris.Add(new KeyValuePair<int, TriAgregat>(rang, new TriAgregat(element, sens == "Décroissant")));
            }
            if (caseCompter.Checked) requete.Elements.Add(new ElementAgregat(null, null, FonctionAgregat.NombreLignes));

            List<KeyValuePair<int, KeyValuePair<int, TriAgregat>>> ordonnes = new List<KeyValuePair<int, KeyValuePair<int, TriAgregat>>>();
            for (int i = 0; i < tris.Count; i++) ordonnes.Add(new KeyValuePair<int, KeyValuePair<int, TriAgregat>>(i, tris[i]));
            ordonnes.Sort(delegate (KeyValuePair<int, KeyValuePair<int, TriAgregat>> a, KeyValuePair<int, KeyValuePair<int, TriAgregat>> b)
            {
                int comparaison = a.Value.Key.CompareTo(b.Value.Key);
                return comparaison != 0 ? comparaison : a.Key.CompareTo(b.Key);
            });
            foreach (KeyValuePair<int, KeyValuePair<int, TriAgregat>> tri in ordonnes) requete.Tris.Add(tri.Value.Value);

            foreach (DataGridViewRow ligne in grilleFiltres.Rows)
            {
                if (ligne.IsNewRow) continue;
                FonctionAgregat? portee = PorteeDuFiltre(ligne);
                string operateur = ligne.Cells[colFiltreOperateur.Index].Value as string;
                if (!portee.HasValue || operateur == null || !pOperateurs.ContainsKey(operateur)) continue;
                ColonneRequete colonne = ColonneDuFiltre(ligne);
                if (colonne == null && portee.Value != FonctionAgregat.NombreLignes) continue;

                ElementAgregat element = portee.Value == FonctionAgregat.NombreLignes
                    ? new ElementAgregat(null, null, FonctionAgregat.NombreLignes)
                    : new ElementAgregat(colonne.Table, colonne.Colonne, portee.Value);
                element.FormatDate = FormatChoisi(ligne.Cells[colFiltreFormat.Index].Value);
                requete.Having.Add(new CritereAgregat(element, pOperateurs[operateur],
                    Convert.ToString(ligne.Cells[colFiltreValeur.Index].Value), Convert.ToString(ligne.Cells[colFiltreValeur2.Index].Value)));
            }

            ResultatGeneration resultat = GenerateurSqlAgregat.ConstruireSelect(requete);
            List<string> avertissements = new List<string>(resultat.Avertissements);
            if (caseDistinct.Checked) avertissements.Add("DISTINCT est sans objet avec un regroupement : option ignorée.");
            if (caseRrn.Checked) avertissements.Add("Le n° d'enregistrement (RRN) n'a pas de sens par groupe : option ignorée.");
            return new ResultatGeneration(resultat.Sql, new List<string>(resultat.Erreurs), avertissements);
        }

        /// <summary>Calcul visé par un filtre (HAVING), ou null pour un filtre sur les lignes (WHERE).</summary>
        private FonctionAgregat? PorteeDuFiltre(DataGridViewRow ligne)
        {
            string libelle = ligne.Cells[colFiltrePorte.Index].Value as string;
            FonctionAgregat fonction;
            if (libelle != null && pPortees.TryGetValue(libelle, out fonction)) return fonction;
            return null;
        }

        /// <summary>Filtres sur les lignes (WHERE) ; ceux qui portent sur un calcul vont au HAVING.</summary>
        private List<CritereFiltre> CriteresSaisis()
        {
            return CriteresDesFiltres(false);
        }

        /// <summary>
        /// Filtres sur les lignes : ceux des tables de la requête (A, B...), ou, avec
        /// <paramref name="sousRequetes"/>, ceux des tables des conditions d'existence (S1...).
        /// </summary>
        private List<CritereFiltre> CriteresDesFiltres(bool sousRequetes)
        {
            List<CritereFiltre> criteres = new List<CritereFiltre>();
            foreach (DataGridViewRow ligne in grilleFiltres.Rows)
            {
                if (ligne.IsNewRow || PorteeDuFiltre(ligne).HasValue) continue;
                ColonneRequete colonne = ColonneDuFiltre(ligne);
                string operateur = ligne.Cells[colFiltreOperateur.Index].Value as string;
                if (colonne == null || operateur == null || !pOperateurs.ContainsKey(operateur)) continue;
                if (EstTableExistence(colonne.Table) != sousRequetes) continue;

                CritereFiltre critere = new CritereFiltre();
                critere.Table = colonne.Table;
                critere.Colonne = colonne.Colonne;
                critere.Operateur = pOperateurs[operateur];
                critere.Valeur = Convert.ToString(ligne.Cells[colFiltreValeur.Index].Value);
                critere.Valeur2 = Convert.ToString(ligne.Cells[colFiltreValeur2.Index].Value);
                critere.FormatDate = FormatChoisi(ligne.Cells[colFiltreFormat.Index].Value);
                criteres.Add(critere);
            }
            return criteres;
        }

        private ResultatGeneration GenererLigne()
        {
            List<string> erreurs = new List<string>();
            if (radioAjouter.Checked)
            {
                List<AffectationColonne> valeurs = new List<AffectationColonne>();
                foreach (DataGridViewRow ligne in grilleValeurs.Rows)
                {
                    string nouvelle = Convert.ToString(ligne.Cells[colVNouvelle.Index].Value);
                    bool estNull = true.Equals(ligne.Cells[colVNull.Index].Value);
                    if (nouvelle.Length == 0 && !estNull) continue;
                    valeurs.Add(Affectation(ligne, nouvelle, estNull));
                }
                return GenerateurSqlSimple.ConstruireInsert(pStructure.Table, valeurs);
            }

            IdentificationLigne identification = Identification(erreurs);
            if (identification == null) return new ResultatGeneration(null, erreurs);

            ResultatGeneration resultat;
            if (radioSupprimer.Checked)
            {
                resultat = GenerateurSqlSimple.ConstruireDelete(pStructure.Table, identification);
            }
            else
            {
                List<AffectationColonne> modifications = new List<AffectationColonne>();
                foreach (DataGridViewRow ligne in grilleValeurs.Rows)
                {
                    string actuelle = Convert.ToString(ligne.Cells[colVActuelle.Index].Value);
                    string nouvelle = Convert.ToString(ligne.Cells[colVNouvelle.Index].Value);
                    bool estNull = true.Equals(ligne.Cells[colVNull.Index].Value);
                    // Modifiée si elle passe à NULL, ou si une valeur saisie diffère de celle lue.
                    // Vide = inchangée (un espace met un code texte à blanc).
                    if (!estNull && (nouvelle.Length == 0 || string.Equals(actuelle, nouvelle, StringComparison.Ordinal))) continue;
                    modifications.Add(Affectation(ligne, nouvelle, estNull));
                }
                resultat = GenerateurSqlSimple.ConstruireUpdate(pStructure.Table, modifications, identification);
            }

            if (erreurs.Count == 0) return resultat;
            erreurs.AddRange(resultat.Erreurs);
            return new ResultatGeneration(null, erreurs);
        }

        /// <summary>Désignation de la ligne : RRN s'il est saisi, sinon valeurs de la clé d'identification.</summary>
        private IdentificationLigne Identification(List<string> erreurs)
        {
            string rrnTexte = (champRrn.Text ?? string.Empty).Trim();
            if (rrnTexte.Length > 0)
            {
                long rrn;
                if (long.TryParse(rrnTexte, NumberStyles.Integer, CultureInfo.InvariantCulture, out rrn)) return IdentificationLigne.ParRrn(rrn);
                erreurs.Add("Numéro d'enregistrement (RRN) invalide : « " + rrnTexte + " ».");
                return null;
            }

            CleInfo cle = pStructure.CleIdentification;
            if (cle == null)
            {
                erreurs.Add("Pas de clé unique : saisissez le numéro d'enregistrement (RRN) de la ligne.");
                return null;
            }

            List<AffectationColonne> valeurs = new List<AffectationColonne>();
            bool auMoinsUne = false;
            foreach (string nom in cle.Colonnes)
            {
                DataGridViewRow ligne = LigneValeur(nom);
                if (ligne == null) continue;
                string valeur = Convert.ToString(ligne.Cells[colVActuelle.Index].Value);
                ColonneInfo colonne = (ColonneInfo)ligne.Tag;
                if (valeur.Length > 0) auMoinsUne = true;
                else if (TypesSql.Categorie(colonne.TypeDonnee) != CategorieType.Texte)
                    erreurs.Add("Saisissez la valeur de la clé " + colonne.Nom + " (colonne « Valeur actuelle / clé »).");
                valeurs.Add(new AffectationColonne(colonne, valeur, false, FormatChoisi(ligne.Cells[colVFormat.Index].Value)));
            }
            // Une clé texte vide est permise (code blanc), mais pas une clé entièrement vide.
            if (!auMoinsUne)
            {
                erreurs.Add("Saisissez la clé de la ligne (" + cle.ListeColonnes + ") dans « Valeur actuelle / clé », ou son RRN.");
                return null;
            }
            return IdentificationLigne.ParCle(valeurs);
        }

        private AffectationColonne Affectation(DataGridViewRow ligne, string valeur, bool estNull)
        {
            return new AffectationColonne((ColonneInfo)ligne.Tag, valeur, estNull, FormatChoisi(ligne.Cells[colVFormat.Index].Value));
        }

        private DataGridViewRow LigneValeur(string nomColonne)
        {
            foreach (DataGridViewRow ligne in grilleValeurs.Rows)
                if (string.Equals(((ColonneInfo)ligne.Tag).Nom, nomColonne, StringComparison.OrdinalIgnoreCase)) return ligne;
            return null;
        }

        private ColonneRequete ColonneDuFiltre(DataGridViewRow ligne)
        {
            string libelle = ligne.Cells[colFiltreColonne.Index].Value as string;
            ColonneRequete colonne;
            return libelle != null && pColonnesParLibelle.TryGetValue(libelle, out colonne) ? colonne : null;
        }

        private FormatDate? FormatChoisi(object valeurCellule)
        {
            string libelle = valeurCellule as string;
            FormatDate format;
            if (libelle != null && pFormats.TryGetValue(libelle, out format)) return format;
            return null;
        }

        // ----------------------------------------------------------------- Traitement par lot (niveau 4, EF-49)

        private void ModeLot_CheckedChanged(object sender, EventArgs e)
        {
            if (!((RadioButton)sender).Checked) return; // Un seul passage : le bouton qui s'active.
            RemplirLot();
            Regenerer();
        }

        private void boutonSourceLot_Click(object sender, EventArgs e)
        {
            if (pStructure == null)
            {
                Info("Chargez d'abord la table à modifier (A).");
                return;
            }
            StructureTable structure = DecrireTableSaisie("où prendre les valeurs");
            if (structure == null) return;

            SourceMiseAJour source = new SourceMiseAJour(structure);
            using (FormJointure dialogue = new FormJointure(new List<TableRequete> { pTables[0] }, source.Table, null, ModeLiaison.Source))
            {
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                source.Conditions.AddRange(dialogue.Jointure.Conditions);
            }
            pSourceLot = source;
            RemplirLot();
            Regenerer();
        }

        private void boutonCibleLot_Click(object sender, EventArgs e)
        {
            if (pStructure == null)
            {
                Info("Chargez d'abord la table d'où viennent les lignes (A) et filtrez-la dans l'onglet Consulter.");
                return;
            }
            StructureTable structure = DecrireTableSaisie("où copier les lignes");
            if (structure == null) return;
            pCibleLot = structure;
            RemplirLot();
            Regenerer();
        }

        /// <summary>
        /// Grille du lot. Modifier : colonnes de A, valeur fixe ou colonne de la source.
        /// Copier : colonnes de la cible, associées aux colonnes de la consultation de même nom.
        /// Supprimer : pas de saisie.
        /// </summary>
        private void RemplirLot()
        {
            bool modifier = radioLotModifier.Checked;
            bool copier = radioLotCopier.Checked;
            boutonSourceLot.Visible = modifier;
            boutonCibleLot.Visible = copier;
            grilleLot.Visible = modifier || copier;
            colLSource.HeaderText = copier ? "Prendre la valeur de (consultation)" : "Prendre la valeur de (table source)";

            bool etat = pRemplissage;
            pRemplissage = true;
            try
            {
                grilleLot.Rows.Clear();
                colLSource.Items.Clear();
                colLSource.Items.Add(string.Empty);
                pColonnesSourceLot.Clear();
                pColonnesCopieLot.Clear();

                IEnumerable<ColonneInfo> cibles = null;
                Dictionary<ColonneInfo, string> associations = new Dictionary<ColonneInfo, string>();
                if (modifier && pStructure != null)
                {
                    cibles = pStructure.Colonnes;
                    if (pSourceLot != null)
                    {
                        foreach (ColonneInfo colonne in pSourceLot.Table.Structure.Colonnes)
                        {
                            string libelle = pSourceLot.Table.Alias + "." + (string.IsNullOrEmpty(colonne.Libelle) ? colonne.Nom : colonne.Nom + " — " + colonne.Libelle);
                            pColonnesSourceLot[libelle] = colonne;
                            colLSource.Items.Add(libelle);
                        }
                    }
                }
                else if (copier && pCibleLot != null)
                {
                    cibles = pCibleLot.Colonnes;
                    List<ColonneRequete> sources = new List<ColonneRequete>();
                    foreach (TableRequete table in pTables)
                    {
                        foreach (ColonneInfo colonne in table.Structure.Colonnes)
                        {
                            string libelle = table.Alias + "." + (string.IsNullOrEmpty(colonne.Libelle) ? colonne.Nom : colonne.Nom + " — " + colonne.Libelle);
                            ColonneRequete source = new ColonneRequete(table, colonne);
                            pColonnesCopieLot[libelle] = source;
                            colLSource.Items.Add(libelle);
                            sources.Add(source);
                        }
                    }
                    // Zones Atlas de même nom : association proposée d'office.
                    foreach (KeyValuePair<ColonneInfo, ColonneRequete> paire in GenerateurSqlLot.AssocierParNom(cibles, sources))
                    {
                        foreach (KeyValuePair<string, ColonneRequete> entree in pColonnesCopieLot)
                            if (entree.Value.Table == paire.Value.Table && entree.Value.Colonne == paire.Value.Colonne) associations[paire.Key] = entree.Key;
                    }
                }

                if (cibles != null)
                {
                    foreach (ColonneInfo colonne in cibles)
                    {
                        string source;
                        if (!associations.TryGetValue(colonne, out source)) source = string.Empty;
                        int index = grilleLot.Rows.Add(colonne.Nom, colonne.Libelle, TypesSql.Libelle(colonne.TypeDonnee, colonne.Longueur, colonne.Echelle),
                            source, string.Empty, false, FormatPropose(colonne));
                        DataGridViewRow ligne = grilleLot.Rows[index];
                        ligne.Tag = colonne;
                        if (!colonne.AccepteNull) ligne.Cells[colLNull.Index].ReadOnly = true;
                        if (TypesSql.Categorie(colonne.TypeDonnee) == CategorieType.NonPrisEnCharge) ligne.ReadOnly = true;
                    }
                }
            }
            finally
            {
                pRemplissage = etat;
            }

            if (radioLotSupprimer.Checked)
            {
                etiquetteSourceLot.Text = string.Empty;
                etiquetteAideLot.Text = "Supprime TOUTES les lignes de la table A retenues par les filtres et les conditions d'existence de l'onglet "
                    + "Consulter. Le nombre de lignes est compté et confirmé avant l'exécution.";
            }
            else if (modifier)
            {
                etiquetteSourceLot.Text = pSourceLot == null
                    ? "Table source : aucune (valeurs fixes seulement)."
                    : "Table source : " + pSourceLot.Table.Table.Nom + " (" + SourceMiseAJour.AliasSource + "), reliée par "
                      + DescriptionLiens(pSourceLot.Conditions) + ".";
                etiquetteAideLot.Text = "Modifie les lignes de A retenues dans l'onglet Consulter. Pour chaque colonne : une valeur fixe, NULL, "
                    + "ou une colonne de la table source (choisie dans la liste « Table » en haut). Colonne laissée vide = inchangée.";
            }
            else
            {
                etiquetteSourceLot.Text = pCibleLot == null
                    ? "Table cible : choisissez-la dans la liste « Table » en haut, puis cliquez sur le bouton."
                    : "Table cible : " + pCibleLot.Table.Schema + "." + pCibleLot.Table.Nom + " — colonnes de même nom associées d'office.";
                etiquetteAideLot.Text = "Copie dans la table cible les lignes de la consultation (tables, filtres et conditions de l'onglet Consulter). "
                    + "Pour chaque colonne cible : une colonne de la consultation, une valeur fixe ou NULL ; vide = valeur par défaut.";
            }
        }

        private static string DescriptionLiens(List<ConditionJointure> conditions)
        {
            List<string> textes = new List<string>();
            foreach (ConditionJointure condition in conditions)
                textes.Add(SourceMiseAJour.AliasSource + "." + condition.ColonneJointe.Nom + " = A." + condition.ColonneLiee.Nom);
            return string.Join(" ET ", textes);
        }

        private ResultatGeneration GenererLot()
        {
            RequeteJointure lignes = new RequeteJointure(pTables[0]);
            lignes.Jointures.AddRange(pJointures);
            lignes.Existences.AddRange(ExistencesSaisies());
            lignes.Criteres.AddRange(CriteresSaisis());
            lignes.Distinct = caseDistinct.Checked;
            lignes.Limite = (int)champLimite.Value;

            ResultatGeneration resultat;
            if (radioLotSupprimer.Checked)
            {
                resultat = GenerateurSqlLot.ConstruireDelete(lignes);
            }
            else if (radioLotModifier.Checked)
            {
                List<AffectationLot> affectations = new List<AffectationLot>();
                foreach (DataGridViewRow ligne in grilleLot.Rows)
                {
                    ColonneInfo cible = (ColonneInfo)ligne.Tag;
                    string source = ligne.Cells[colLSource.Index].Value as string;
                    string valeur = Convert.ToString(ligne.Cells[colLValeur.Index].Value);
                    bool estNull = true.Equals(ligne.Cells[colLNull.Index].Value);
                    ColonneInfo colonneSource;
                    if (!string.IsNullOrEmpty(source) && pColonnesSourceLot.TryGetValue(source, out colonneSource))
                        affectations.Add(new AffectationLot(cible, colonneSource));
                    else if (estNull || valeur.Length > 0)
                        affectations.Add(new AffectationLot(cible, valeur, estNull, FormatChoisi(ligne.Cells[colLFormat.Index].Value)));
                }
                resultat = GenerateurSqlLot.ConstruireUpdate(lignes, affectations, pSourceLot);
            }
            else
            {
                if (pCibleLot == null)
                    return new ResultatGeneration(null, new[] { "Choisissez la table cible (liste « Table » en haut, puis « Copier vers la table choisie en haut… »)." });
                List<CorrespondanceInsert> correspondances = new List<CorrespondanceInsert>();
                foreach (DataGridViewRow ligne in grilleLot.Rows)
                {
                    ColonneInfo cible = (ColonneInfo)ligne.Tag;
                    string source = ligne.Cells[colLSource.Index].Value as string;
                    string valeur = Convert.ToString(ligne.Cells[colLValeur.Index].Value);
                    bool estNull = true.Equals(ligne.Cells[colLNull.Index].Value);
                    ColonneRequete colonneSource;
                    if (!string.IsNullOrEmpty(source) && pColonnesCopieLot.TryGetValue(source, out colonneSource))
                        correspondances.Add(new CorrespondanceInsert(cible, colonneSource));
                    else if (estNull || valeur.Length > 0)
                        correspondances.Add(new CorrespondanceInsert(cible, valeur, estNull, FormatChoisi(ligne.Cells[colLFormat.Index].Value)));
                }
                resultat = GenerateurSqlLot.ConstruireInsertSelect(pCibleLot.Table, correspondances, lignes);
            }

            if (!ModeAgregat()) return resultat;
            List<string> avertissements = new List<string>(resultat.Avertissements);
            avertissements.Add("Les calculs et regroupements de l'onglet Consulter ne s'appliquent pas au traitement par lot.");
            return new ResultatGeneration(resultat.Sql, new List<string>(resultat.Erreurs), avertissements);
        }

        // ----------------------------------------------------------------- Sortie

        private void boutonInserer_Click(object sender, EventArgs e)
        {
            Regenerer();
            if (string.IsNullOrEmpty(apercuSql.Text)) return;
            SqlGenere = apercuSql.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Erreurs(IEnumerable<string> erreurs)
        {
            Erreurs(erreurs, null);
        }

        /// <summary>Erreurs en rouge (insertion bloquée) ; sinon avertissements en orange (insertion permise).</summary>
        private void Erreurs(IEnumerable<string> erreurs, IEnumerable<string> avertissements)
        {
            List<string> liste = new List<string>(erreurs);
            List<string> attention = avertissements != null ? new List<string>(avertissements) : new List<string>();
            if (liste.Count > 0)
            {
                etiquetteErreurs.ForeColor = System.Drawing.Color.DarkRed;
                etiquetteErreurs.Text = string.Join(Environment.NewLine, liste);
            }
            else
            {
                etiquetteErreurs.ForeColor = System.Drawing.Color.FromArgb(160, 82, 0);
                etiquetteErreurs.Text = attention.Count == 0 ? string.Empty : "Attention : " + string.Join(Environment.NewLine, attention);
            }
            boutonInserer.Enabled = liste.Count == 0 && !string.IsNullOrEmpty(apercuSql.Text);
        }

        private void Info(string message)
        {
            etiquetteInfoTable.Text = message;
        }

        /// <summary>Élément de la liste des tables : « NOM — libellé », pour la saisie semi-automatique.</summary>
        private sealed class ElementTable
        {
            public const string Separateur = "  —  ";

            public ElementTable(TableInfo table)
            {
                Table = table;
            }

            public TableInfo Table { get; }

            public override string ToString()
            {
                return string.IsNullOrEmpty(Table.Libelle) ? Table.Nom : Table.Nom + Separateur + Table.Libelle;
            }
        }
    }
}

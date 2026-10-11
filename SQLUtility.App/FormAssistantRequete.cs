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

        private readonly Dictionary<string, ColonneRequete> pColonnesParLibelle = new Dictionary<string, ColonneRequete>();
        private readonly Dictionary<string, OperateurFiltre> pOperateurs = new Dictionary<string, OperateurFiltre>();
        private readonly Dictionary<string, FormatDate> pFormats = new Dictionary<string, FormatDate>();
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
            foreach (DescriptionFormatDate description in ReferentielFormatsDate.Tous)
            {
                if (!description.Numerique || !description.Implemente) continue;
                pFormats[description.Libelle] = description.Format;
                colFiltreFormat.Items.Add(description.Libelle);
                colVFormat.Items.Add(description.Libelle);
            }
            // ISO stocké en numérique (AAAAMMJJ) : format DECIMAL(8,0) fréquent hors Atlas.
            string libelleIso = "AAAAMMJJ (numérique)";
            pFormats[libelleIso] = FormatDate.Iso;
            colFiltreFormat.Items.Add(libelleIso);
            colVFormat.Items.Add(libelleIso);
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
                pTables.Add(new TableRequete(pStructure, "A"));
                AjouterColonnesConsultation(pTables[0]);
                RafraichirTables();

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
                    int ligne = grilleChoixColonnes.Rows.Add(false, table.Alias, colonne.Nom, colonne.Libelle, type, colonne.RangCleTexte, "", "");
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
            if (e.RowIndex > 0) ModifierJointure(e.RowIndex);
        }

        private void boutonModifierJointure_Click(object sender, EventArgs e)
        {
            int index = grilleTables.CurrentRow != null ? grilleTables.CurrentRow.Index : -1;
            if (index < 1)
            {
                Info("Sélectionnez une table jointe (B, C...) : la table A n'a pas de jointure.");
                return;
            }
            ModifierJointure(index);
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
                Info("Sélectionnez une table jointe à retirer (la table A se change avec « Charger »).");
                return;
            }
            TableRequete table = pTables[index];
            foreach (Jointure autre in pJointures)
            {
                if (autre.Table == table) continue;
                foreach (ConditionJointure condition in autre.Conditions)
                {
                    if (condition.TableLiee != table) continue;
                    Info("Impossible de retirer " + table.Alias + " : la jointure de " + autre.Table.Alias + " s'appuie dessus.");
                    return;
                }
            }

            pRemplissage = true;
            try
            {
                for (int i = grilleChoixColonnes.Rows.Count - 1; i >= 0; i--)
                    if (((ColonneRequete)grilleChoixColonnes.Rows[i].Tag).Table == table) grilleChoixColonnes.Rows.RemoveAt(i);
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
            boutonModifierJointure.Enabled = boutonRetirerTable.Enabled = pTables.Count > 1;
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

            ResultatGeneration resultat = onglets.SelectedTab == ongletConsulter ? GenererConsultation() : GenererLigne();
            apercuSql.Text = resultat.Sql ?? string.Empty;
            Erreurs(resultat.Erreurs, resultat.Avertissements);
        }

        /// <summary>
        /// Consultation : une seule table = SQL du niveau 1 (colonnes non qualifiées) ;
        /// dès qu'une table est jointe = SQL du niveau 2 (colonnes qualifiées par alias).
        /// </summary>
        private ResultatGeneration GenererConsultation()
        {
            List<ColonneRequete> colonnes = new List<ColonneRequete>();
            List<CritereTri> tris = TrisSaisis(colonnes);
            List<CritereFiltre> criteres = CriteresSaisis();

            if (pJointures.Count == 0)
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

        private List<CritereFiltre> CriteresSaisis()
        {
            List<CritereFiltre> criteres = new List<CritereFiltre>();
            foreach (DataGridViewRow ligne in grilleFiltres.Rows)
            {
                if (ligne.IsNewRow) continue;
                ColonneRequete colonne = ColonneDuFiltre(ligne);
                string operateur = ligne.Cells[colFiltreOperateur.Index].Value as string;
                if (colonne == null || operateur == null || !pOperateurs.ContainsKey(operateur)) continue;

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

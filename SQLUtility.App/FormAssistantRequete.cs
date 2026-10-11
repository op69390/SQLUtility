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
    /// Assistant de requête, niveau 1 (une table) : consultation guidée et ajout /
    /// modification / suppression d'une ligne désignée par sa clé ou son RRN. (EF-40 à EF-43)
    /// Le SQL produit est rendu à l'éditeur (<see cref="SqlGenere"/>), jamais exécuté ici,
    /// sauf la relecture d'une ligne, faite en lecture seule.
    /// </summary>
    public partial class FormAssistantRequete : Form
    {
        private const string SansFormat = "(non)";

        private readonly ConnexionProfil pProfil;
        private readonly IConnexionProvider pProvider;
        private readonly string pSchemaInitial;

        private StructureTable pStructure;
        private readonly Dictionary<string, ColonneInfo> pColonnesParLibelle = new Dictionary<string, ColonneInfo>();
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

                foreach (ColonneInfo colonne in pStructure.Colonnes)
                {
                    string type = TypesSql.Libelle(colonne.TypeDonnee, colonne.Longueur, colonne.Echelle);

                    int ligne = grilleChoixColonnes.Rows.Add(false, colonne.Nom, colonne.Libelle, type, colonne.RangCleTexte, "", "");
                    grilleChoixColonnes.Rows[ligne].Tag = colonne;

                    string libelle = LibelleColonne(colonne);
                    pColonnesParLibelle[libelle] = colonne;
                    colFiltreColonne.Items.Add(libelle);

                    ligne = grilleValeurs.Rows.Add(colonne.Nom, colonne.Libelle, type, colonne.RangCleTexte, "", "", false, FormatPropose(colonne));
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

        private static string LibelleColonne(ColonneInfo colonne)
        {
            return string.IsNullOrEmpty(colonne.Libelle) ? colonne.Nom : colonne.Nom + " — " + colonne.Libelle;
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
                ColonneInfo colonne = ColonneDuFiltre(ligne);
                pRemplissage = true;
                try
                {
                    if (ligne.Cells[colFiltreOperateur.Index].Value == null) ligne.Cells[colFiltreOperateur.Index].Value = "=";
                    if (colonne != null) ligne.Cells[colFiltreFormat.Index].Value = FormatPropose(colonne);
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
            Erreurs(resultat.Erreurs);
        }

        private ResultatGeneration GenererConsultation()
        {
            RequeteConsultation requete = new RequeteConsultation();
            requete.Table = pStructure.Table;
            requete.ToutesColonnes = new List<ColonneInfo>(pStructure.Colonnes);
            requete.Distinct = caseDistinct.Checked;
            requete.AvecRrn = caseRrn.Checked;
            requete.Limite = (int)champLimite.Value;

            List<KeyValuePair<int, CritereTri>> tris = new List<KeyValuePair<int, CritereTri>>();
            foreach (DataGridViewRow ligne in grilleChoixColonnes.Rows)
            {
                ColonneInfo colonne = (ColonneInfo)ligne.Tag;
                if (true.Equals(ligne.Cells[colAfficher.Index].Value)) requete.Colonnes.Add(colonne);

                string sens = ligne.Cells[colTri.Index].Value as string;
                if (string.IsNullOrEmpty(sens)) continue;
                int rang;
                if (!int.TryParse(Convert.ToString(ligne.Cells[colRangTri.Index].Value), out rang)) rang = 1000 + ligne.Index;
                tris.Add(new KeyValuePair<int, CritereTri>(rang, new CritereTri(colonne, sens == "Décroissant")));
            }
            // Tri stable par rang (List.Sort ne l'est pas : on départage par l'ordre d'origine).
            List<KeyValuePair<int, KeyValuePair<int, CritereTri>>> ordonnes = new List<KeyValuePair<int, KeyValuePair<int, CritereTri>>>();
            for (int i = 0; i < tris.Count; i++) ordonnes.Add(new KeyValuePair<int, KeyValuePair<int, CritereTri>>(i, tris[i]));
            ordonnes.Sort(delegate (KeyValuePair<int, KeyValuePair<int, CritereTri>> a, KeyValuePair<int, KeyValuePair<int, CritereTri>> b)
            {
                int comparaison = a.Value.Key.CompareTo(b.Value.Key);
                return comparaison != 0 ? comparaison : a.Key.CompareTo(b.Key);
            });
            foreach (KeyValuePair<int, KeyValuePair<int, CritereTri>> tri in ordonnes) requete.Tris.Add(tri.Value.Value);

            foreach (DataGridViewRow ligne in grilleFiltres.Rows)
            {
                if (ligne.IsNewRow) continue;
                ColonneInfo colonne = ColonneDuFiltre(ligne);
                string operateur = ligne.Cells[colFiltreOperateur.Index].Value as string;
                if (colonne == null || operateur == null || !pOperateurs.ContainsKey(operateur)) continue;

                CritereFiltre critere = new CritereFiltre();
                critere.Colonne = colonne;
                critere.Operateur = pOperateurs[operateur];
                critere.Valeur = Convert.ToString(ligne.Cells[colFiltreValeur.Index].Value);
                critere.Valeur2 = Convert.ToString(ligne.Cells[colFiltreValeur2.Index].Value);
                critere.FormatDate = FormatChoisi(ligne.Cells[colFiltreFormat.Index].Value);
                requete.Criteres.Add(critere);
            }

            return GenerateurSqlSimple.ConstruireSelect(requete);
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

        private ColonneInfo ColonneDuFiltre(DataGridViewRow ligne)
        {
            string libelle = ligne.Cells[colFiltreColonne.Index].Value as string;
            ColonneInfo colonne;
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
            List<string> liste = new List<string>(erreurs);
            etiquetteErreurs.Text = string.Join(Environment.NewLine, liste);
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

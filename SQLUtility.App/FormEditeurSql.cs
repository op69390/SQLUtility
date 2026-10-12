using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using SQLUtility.Core.Assistant;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Core.Edition;
using SQLUtility.Core.Execution;
using SQLUtility.Core.Optimisation;
using SQLUtility.Data.Fichiers;
using SQLUtility.Data.Parametres;
using SQLUtility.Data.Providers;

namespace SQLUtility.App
{
    /// <summary>
    /// Éditeur SQL : coloration syntaxique (EF-15, via <see cref="TokeniseurSql"/>),
    /// auto-complétion (EF-16, via <see cref="MoteurCompletion"/>) et lancement du lint (OPT-xx).
    /// Sources de complétion : mots-clés SQL et identifiants du document (le catalogue
    /// alimentera la complétion avec le câblage connexion d'EF-17/18).
    /// </summary>
    public partial class FormEditeurSql : Form
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int message, int wParam, int lParam);
        private const int WM_SETREDRAW = 0x000B;

        private bool pColorationEnCours;
        private readonly List<IRegleAnalyse> pRegles;

        // Types des colonnes du schéma chargé, pour OPT-09 (null : nom ambigu entre tables).
        private Dictionary<string, CategorieType?> pTypesCatalogue = new Dictionary<string, CategorieType?>(StringComparer.OrdinalIgnoreCase);
        private readonly List<ResultatAnalyse> pResultats = new List<ResultatAnalyse>();

        // Auto-complétion (EF-16)
        private ListBox pListeCompletion;
        private int pDebutMot;

        // Paramètres (profils de connexion) pour l'insertion de colonnes (EF-17/18).
        private readonly ParametresApplication pParametres;

        // Identifiants du catalogue (tables + colonnes) pour l'auto-complétion (EF-16).
        private List<string> pIdentifiantsCatalogue = new List<string>();
        private ConnexionProfil pProfilCatalogue;
        private IConnexionProvider pProviderCatalogue;
        private bool pChargementConnexionsEditeur;

        // Exécution (EF-22 à EF-32) : une seule instruction à la fois, une transaction au plus.
        private bool pExecutionEnCours;
        private ITransactionExecution pTransactionEnCours;
        private string pDescriptionTransaction;

        // Fichiers .sql (EF-33) : script courant et dernière version ouverte ou enregistrée.
        private readonly GestionnaireParametres pGestionnaire;
        private string pCheminFichier;
        private string pTexteEnregistre = string.Empty;

        public FormEditeurSql(ParametresApplication parametres)
            : this(parametres, null)
        {
        }

        /// <param name="gestionnaire">Pour mémoriser le dernier dossier des scripts (EF-33) ; facultatif.</param>
        public FormEditeurSql(ParametresApplication parametres, GestionnaireParametres gestionnaire)
        {
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));
            pParametres = parametres;
            pGestionnaire = gestionnaire;
            InitializeComponent();
            // OPT-01 à OPT-10 ; OPT-09 lit les types du schéma chargé (« Charger ce schéma »).
            pRegles = CatalogueRegles.Toutes(TypeColonneCatalogue);
            InitialiserCompletion();
            PeuplerConnexionsEditeur();
            // Second abonnement, hors de la garde de coloration : toute modification met le titre à jour.
            zoneEditeur.TextChanged += delegate { MettreAJourTitre(); };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // Exemple de départ (illustre la coloration et déclenche le lint OPT-01).
            zoneEditeur.Text =
                "SELECT *" + Environment.NewLine +
                "FROM ATLASDTA.ARTICLES" + Environment.NewLine +
                "WHERE DATCRE > 1260101  -- date au format CYYMMDD" + Environment.NewLine +
                "ORDER BY ARTICLE;";
            // L'exemple n'est pas un travail à enregistrer.
            pTexteEnregistre = zoneEditeur.Text;
            MettreAJourTitre();
        }

        // ----------------------------------------------------------------- Analyse et index conseillés (OPT-01 à OPT-11)

        /// <summary>Famille de type d'une colonne du schéma chargé ; null si inconnue ou ambiguë.</summary>
        private CategorieType? TypeColonneCatalogue(string nom)
        {
            CategorieType? categorie;
            return nom != null && pTypesCatalogue.TryGetValue(nom, out categorie) ? categorie : null;
        }

        private void boutonIndex_Click(object sender, EventArgs e)
        {
            RechercherIndexConseilles();
        }

        /// <summary>
        /// Index conseillés par l'optimiseur pour chaque table citée dans le script (hors étapes
        /// WITH) ; une table non qualifiée est cherchée dans la bibliothèque par défaut du profil.
        /// </summary>
        internal void RechercherIndexConseilles()
        {
            if (pProfilCatalogue == null || pProviderCatalogue == null)
            {
                Statut("Choisissez d'abord une connexion (liste « Connexion / schéma »).");
                return;
            }
            string schemaParDefaut = null;
            if (!string.IsNullOrEmpty(pProfilCatalogue.BibliothequeParDefaut))
                schemaParDefaut = pProfilCatalogue.BibliothequeParDefaut.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)[0].ToUpperInvariant();

            List<KeyValuePair<string, string>> tables = new List<KeyValuePair<string, string>>();
            List<JetonAnalyse> jetons = AnalyseLexicale.Jetons(zoneEditeur.Text);
            foreach (Plage plage in AnalyseLexicale.Instructions(jetons))
            {
                foreach (TableCitee table in AnalyseLexicale.TablesCitees(jetons, plage))
                {
                    string schema = table.Schema ?? schemaParDefaut;
                    if (schema == null) continue;
                    KeyValuePair<string, string> cle = new KeyValuePair<string, string>(schema, table.Nom);
                    if (!tables.Contains(cle)) tables.Add(cle);
                }
            }
            grilleIndex.Rows.Clear();
            ongletsBas.SelectedTab = ongletIndex;
            if (tables.Count == 0)
            {
                Statut("Aucune table qualifiée dans le script (ou pas de bibliothèque par défaut dans le profil).");
                return;
            }

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            int nombre = 0;
            try
            {
                foreach (KeyValuePair<string, string> table in tables)
                {
                    IReadOnlyList<IndexConseille> conseils = pProviderCatalogue.ListerIndexConseilles(pProfilCatalogue, table.Key, table.Value);
                    for (int i = 0; i < conseils.Count; i++)
                    {
                        IndexConseille conseil = conseils[i];
                        int ligne = grilleIndex.Rows.Add(conseil.Schema + "." + conseil.Table, conseil.Colonnes, conseil.TypeIndex,
                            conseil.NombreConseils.ToString("N0", CultureInfo.CurrentCulture),
                            conseil.DernierConseil.HasValue ? conseil.DernierConseil.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : string.Empty,
                            conseil.DureeCreationSecondes.HasValue ? conseil.DureeCreationSecondes.Value.ToString(CultureInfo.CurrentCulture) : string.Empty,
                            conseil.Raison, conseil.InstructionCreation(i + 1));
                        grilleIndex.Rows[ligne].Tag = conseil.InstructionCreation(i + 1);
                        nombre++;
                    }
                }
                Statut(nombre == 0
                    ? "Aucun index conseillé pour " + tables.Count + " table(s) : l'optimiseur n'a rien relevé."
                    : nombre + " index conseillé(s) pour " + tables.Count + " table(s).");
            }
            catch (Exception ex)
            {
                Statut("Index conseillés indisponibles : " + ex.Message
                    + " — vérifiez l'accès à QSYS2.SYSIXADV (SELECT * FROM QSYS2.SYSIXADV FETCH FIRST 1 ROW ONLY).");
            }
            finally
            {
                Cursor = curseur;
            }
        }

        /// <summary>Copie l'instruction de création dans l'éditeur, sans l'exécuter : un index se valide avant.</summary>
        private void grilleIndex_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            string instruction = grilleIndex.Rows[e.RowIndex].Tag as string;
            if (string.IsNullOrEmpty(instruction)) return;
            InsererInstruction("-- Index conseillé par l'optimiseur : à faire valider avant création (place disque, maintenance à chaque mise à jour)\n" + instruction);
            Statut("Instruction de création copiée dans l'éditeur : à faire valider avant toute exécution.");
        }

        // ----------------------------------------------------------------- Fichiers .sql (EF-33)

        private void menuNouveau_Click(object sender, EventArgs e)
        {
            if (!ConfirmerAbandon()) return;
            zoneEditeur.Clear();
            pCheminFichier = null;
            pTexteEnregistre = zoneEditeur.Text;
            MettreAJourTitre();
            Statut("Nouveau script.");
        }

        private void menuOuvrir_Click(object sender, EventArgs e)
        {
            if (!ConfirmerAbandon()) return;
            using (OpenFileDialog dialogue = new OpenFileDialog())
            {
                dialogue.Filter = FichierSql.Filtre;
                dialogue.Title = "Ouvrir un script SQL";
                if (!string.IsNullOrEmpty(pParametres.DernierRepertoireSql) && Directory.Exists(pParametres.DernierRepertoireSql))
                    dialogue.InitialDirectory = pParametres.DernierRepertoireSql;
                if (dialogue.ShowDialog(this) != DialogResult.OK) return;
                OuvrirFichier(dialogue.FileName);
            }
        }

        /// <summary>Charge un script dans l'éditeur ; retourne faux (et prévient) en cas d'échec de lecture.</summary>
        internal bool OuvrirFichier(string chemin)
        {
            string texte;
            try
            {
                texte = FichierSql.Lire(chemin);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Lecture impossible de « " + chemin + " » : " + ex.Message, "Ouvrir", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            zoneEditeur.Text = texte;
            pCheminFichier = chemin;
            pTexteEnregistre = zoneEditeur.Text;
            MemoriserRepertoire(chemin);
            MettreAJourTitre();
            Statut("Ouvert : " + chemin);
            return true;
        }

        private void menuEnregistrer_Click(object sender, EventArgs e)
        {
            Enregistrer();
        }

        private void menuEnregistrerSous_Click(object sender, EventArgs e)
        {
            EnregistrerSous();
        }

        private void menuFermer_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>Enregistre dans le fichier courant, ou demande un nom ; vrai si le script est enregistré.</summary>
        private bool Enregistrer()
        {
            return string.IsNullOrEmpty(pCheminFichier) ? EnregistrerSous() : EcrireFichier(pCheminFichier);
        }

        private bool EnregistrerSous()
        {
            using (SaveFileDialog dialogue = new SaveFileDialog())
            {
                dialogue.Filter = FichierSql.Filtre;
                dialogue.Title = "Enregistrer le script SQL";
                dialogue.DefaultExt = "sql";
                dialogue.AddExtension = true;
                dialogue.OverwritePrompt = true;
                if (!string.IsNullOrEmpty(pCheminFichier))
                {
                    dialogue.InitialDirectory = Path.GetDirectoryName(pCheminFichier);
                    dialogue.FileName = Path.GetFileName(pCheminFichier);
                }
                else if (!string.IsNullOrEmpty(pParametres.DernierRepertoireSql) && Directory.Exists(pParametres.DernierRepertoireSql))
                {
                    dialogue.InitialDirectory = pParametres.DernierRepertoireSql;
                }
                if (dialogue.ShowDialog(this) != DialogResult.OK) return false;
                return EcrireFichier(dialogue.FileName);
            }
        }

        /// <summary>Écrit le script ; retourne faux (et prévient) en cas d'échec, le texte restant marqué modifié.</summary>
        internal bool EcrireFichier(string chemin)
        {
            try
            {
                FichierSql.Ecrire(chemin, zoneEditeur.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Enregistrement impossible dans « " + chemin + " » : " + ex.Message, "Enregistrer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            pCheminFichier = chemin;
            pTexteEnregistre = zoneEditeur.Text;
            MemoriserRepertoire(chemin);
            MettreAJourTitre();
            Statut("Enregistré : " + chemin);
            return true;
        }

        /// <summary>
        /// Avant de remplacer ou de fermer un script modifié : Oui = enregistrer, Non = abandonner,
        /// Annuler = ne rien faire. Retourne vrai si l'on peut continuer.
        /// </summary>
        private bool ConfirmerAbandon()
        {
            if (!EstModifie) return true;
            DialogResult choix = MessageBox.Show(this,
                "Enregistrer les modifications de « " + NomAffiche + " » ?",
                "Script modifié", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (choix == DialogResult.Cancel) return false;
            return choix == DialogResult.No || Enregistrer();
        }

        /// <summary>Vrai si le texte diffère de la dernière version ouverte ou enregistrée.</summary>
        internal bool EstModifie
        {
            get { return !string.Equals(zoneEditeur.Text, pTexteEnregistre ?? string.Empty, StringComparison.Ordinal); }
        }

        private string NomAffiche
        {
            get { return string.IsNullOrEmpty(pCheminFichier) ? "(sans nom)" : Path.GetFileName(pCheminFichier); }
        }

        private void MettreAJourTitre()
        {
            string titre = "Éditeur SQL — " + NomAffiche + (EstModifie ? " *" : string.Empty);
            if (Text != titre) Text = titre;
        }

        /// <summary>Retient le dossier pour la prochaine boîte Ouvrir / Enregistrer ; un échec n'empêche rien.</summary>
        private void MemoriserRepertoire(string chemin)
        {
            string repertoire = Path.GetDirectoryName(Path.GetFullPath(chemin));
            if (string.Equals(repertoire, pParametres.DernierRepertoireSql, StringComparison.OrdinalIgnoreCase)) return;
            pParametres.DernierRepertoireSql = repertoire;
            if (pGestionnaire == null) return;
            try
            {
                pGestionnaire.Enregistrer(pParametres);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Dernier dossier des scripts non mémorisé : " + ex.Message);
            }
        }

        // ----------------------------------------------------------------- Coloration

        private void zoneEditeur_TextChanged(object sender, EventArgs e)
        {
            if (pColorationEnCours) return;
            Coloriser();
            MettreAJourCompletion(false);
        }

        private void Coloriser()
        {
            pColorationEnCours = true;
            int selDebut = zoneEditeur.SelectionStart;
            int selLong = zoneEditeur.SelectionLength;
            SendMessage(zoneEditeur.Handle, WM_SETREDRAW, 0, 0); // Suspend le redessin (anti-scintillement).
            try
            {
                zoneEditeur.SelectAll();
                zoneEditeur.SelectionColor = zoneEditeur.ForeColor;

                IReadOnlyList<JetonSql> jetons = TokeniseurSql.Tokeniser(zoneEditeur.Text);
                foreach (JetonSql jeton in jetons)
                {
                    Color couleur;
                    if (CouleurPour(jeton.Type, out couleur))
                    {
                        zoneEditeur.Select(jeton.Position, jeton.Longueur);
                        zoneEditeur.SelectionColor = couleur;
                    }
                }

                zoneEditeur.Select(selDebut, selLong);
                zoneEditeur.SelectionColor = zoneEditeur.ForeColor;
            }
            finally
            {
                SendMessage(zoneEditeur.Handle, WM_SETREDRAW, 1, 0);
                zoneEditeur.Invalidate();
                pColorationEnCours = false;
            }
        }

        private static bool CouleurPour(TypeJetonSql type, out Color couleur)
        {
            switch (type)
            {
                case TypeJetonSql.MotCle: couleur = Color.Blue; return true;
                case TypeJetonSql.Chaine: couleur = Color.FromArgb(163, 21, 21); return true;
                case TypeJetonSql.Commentaire: couleur = Color.FromArgb(0, 128, 0); return true;
                case TypeJetonSql.Nombre: couleur = Color.FromArgb(128, 0, 128); return true;
                default: couleur = Color.Empty; return false;
            }
        }

        // ----------------------------------------------------------------- Auto-complétion (EF-16)

        private void InitialiserCompletion()
        {
            pListeCompletion = new ListBox();
            pListeCompletion.BorderStyle = BorderStyle.FixedSingle;
            pListeCompletion.IntegralHeight = false;
            pListeCompletion.Font = zoneEditeur.Font;
            pListeCompletion.Size = new Size(260, 150);
            pListeCompletion.Visible = false;
            pListeCompletion.Click += pListeCompletion_Click;
            separateur.Panel1.Controls.Add(pListeCompletion);
            pListeCompletion.BringToFront();

            zoneEditeur.KeyDown += zoneEditeur_KeyDown;
            zoneEditeur.MouseDown += zoneEditeur_MouseDown;
            this.Deactivate += FormEditeurSql_Deactivate;
        }

        private void zoneEditeur_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Espace : force l'affichage des suggestions.
            if (e.Control && e.KeyCode == Keys.Space)
            {
                MettreAJourCompletion(true);
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }

            if (pListeCompletion == null || !pListeCompletion.Visible) return;

            switch (e.KeyCode)
            {
                case Keys.Down:
                    DeplacerSelection(1);
                    e.Handled = true; e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    DeplacerSelection(-1);
                    e.Handled = true; e.SuppressKeyPress = true;
                    break;
                case Keys.Enter:
                case Keys.Tab:
                    AccepterCompletion();
                    e.Handled = true; e.SuppressKeyPress = true;
                    break;
                case Keys.Escape:
                    MasquerPopup();
                    e.Handled = true; e.SuppressKeyPress = true;
                    break;
            }
        }

        private void MettreAJourCompletion(bool forcer)
        {
            int debut;
            string prefixe = MotCourant(out debut);
            if (!forcer && prefixe.Length < 2)
            {
                MasquerPopup();
                return;
            }

            List<string> candidats = new List<string>(MotsClesSql.Tous);
            candidats.AddRange(MoteurCompletion.IdentifiantsDistincts(zoneEditeur.Text));
            candidats.AddRange(pIdentifiantsCatalogue);
            IReadOnlyList<string> suggestions = MoteurCompletion.Suggerer(prefixe, candidats, 50);
            if (suggestions.Count == 0)
            {
                MasquerPopup();
                return;
            }

            pListeCompletion.BeginUpdate();
            pListeCompletion.Items.Clear();
            foreach (string suggestion in suggestions)
                pListeCompletion.Items.Add(suggestion);
            pListeCompletion.EndUpdate();
            pListeCompletion.SelectedIndex = 0;
            pDebutMot = debut;
            AfficherPopup();
        }

        private void AfficherPopup()
        {
            Point pos = zoneEditeur.GetPositionFromCharIndex(zoneEditeur.SelectionStart);
            int hauteurLigne = (int)Math.Ceiling(zoneEditeur.Font.GetHeight()) + 4;
            int x = pos.X;
            int y = pos.Y + hauteurLigne;

            // Repli (au-dessus / à gauche) si le popup déborde du panneau de l'éditeur.
            int largeurPanneau = separateur.Panel1.ClientSize.Width;
            int hauteurPanneau = separateur.Panel1.ClientSize.Height;
            if (y + pListeCompletion.Height > hauteurPanneau)
                y = Math.Max(0, pos.Y - pListeCompletion.Height);
            if (x + pListeCompletion.Width > largeurPanneau)
                x = Math.Max(0, largeurPanneau - pListeCompletion.Width);

            pListeCompletion.Location = new Point(x, y);
            pListeCompletion.Visible = true;
            pListeCompletion.BringToFront();
        }

        private void DeplacerSelection(int delta)
        {
            int total = pListeCompletion.Items.Count;
            if (total == 0) return;
            int index = pListeCompletion.SelectedIndex + delta;
            if (index < 0) index = 0;
            if (index >= total) index = total - 1;
            pListeCompletion.SelectedIndex = index;
        }

        private void AccepterCompletion()
        {
            if (pListeCompletion.SelectedItem == null)
            {
                MasquerPopup();
                return;
            }

            string choix = pListeCompletion.SelectedItem.ToString();
            int caret = zoneEditeur.SelectionStart;
            int longueurMot = caret - pDebutMot;
            if (longueurMot < 0)
            {
                MasquerPopup();
                return;
            }

            pColorationEnCours = true; // Évite la réentrance coloration/complétion pendant le remplacement.
            try
            {
                zoneEditeur.Select(pDebutMot, longueurMot);
                zoneEditeur.SelectedText = choix;
                zoneEditeur.SelectionStart = pDebutMot + choix.Length;
                zoneEditeur.SelectionLength = 0;
            }
            finally
            {
                pColorationEnCours = false;
            }

            MasquerPopup();
            Coloriser();
            zoneEditeur.Focus();
        }

        private void MasquerPopup()
        {
            if (pListeCompletion != null && pListeCompletion.Visible)
                pListeCompletion.Visible = false;
        }

        private string MotCourant(out int debut)
        {
            int caret = zoneEditeur.SelectionStart;
            string texte = zoneEditeur.Text;
            int i = caret;
            while (i > 0 && EstCarMot(texte[i - 1])) i--;
            debut = i;
            return texte.Substring(i, caret - i);
        }

        private static bool EstCarMot(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        private void pListeCompletion_Click(object sender, EventArgs e)
        {
            AccepterCompletion();
        }

        private void zoneEditeur_MouseDown(object sender, MouseEventArgs e)
        {
            MasquerPopup();
        }

        private void FormEditeurSql_Deactivate(object sender, EventArgs e)
        {
            MasquerPopup();
        }

        // ----------------------------------------------------------------- Lint

        private void boutonAnalyser_Click(object sender, EventArgs e)
        {
            Analyser();
        }

        private void boutonInsererColonne_Click(object sender, EventArgs e)
        {
            using (FormInsertionColonne dialogue = new FormInsertionColonne(pParametres))
            {
                if (dialogue.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(dialogue.TexteAInserer))
                {
                    zoneEditeur.SelectedText = dialogue.TexteAInserer;
                    zoneEditeur.Focus();
                }
            }
        }

        // ----------------------------------------------------------------- Assistant (EF-40 à EF-43)

        private void boutonAssistant_Click(object sender, EventArgs e)
        {
            if (pProfilCatalogue == null || pProviderCatalogue == null)
            {
                Statut("Choisissez d'abord une connexion (liste « Connexion / schéma »).");
                listeConnexionsEditeur.Focus();
                return;
            }

            string schema = listeSchemasEditeur.SelectedItem as string;
            if (schema == null && !string.IsNullOrEmpty(pProfilCatalogue.BibliothequeParDefaut))
                schema = pProfilCatalogue.BibliothequeParDefaut.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)[0];

            using (FormAssistantRequete assistant = new FormAssistantRequete(pProfilCatalogue, pProviderCatalogue, schema))
            {
                if (assistant.ShowDialog(this) == DialogResult.OK && !string.IsNullOrEmpty(assistant.SqlGenere))
                    InsererInstruction(assistant.SqlGenere);
            }
        }

        /// <summary>
        /// Ajoute l'instruction en fin de texte et la sélectionne : F5 n'exécutera qu'elle,
        /// avec l'analyse de risque et les confirmations habituelles.
        /// </summary>
        internal void InsererInstruction(string sql)
        {
            string bloc = sql.Replace("\r\n", "\n").TrimEnd() + ";";
            string texte = zoneEditeur.Text;
            string separation = texte.Trim().Length == 0 ? string.Empty : (texte.EndsWith("\n", StringComparison.Ordinal) ? "\n" : "\n\n");
            if (texte.Trim().Length == 0) zoneEditeur.Clear();

            int debut = zoneEditeur.TextLength + separation.Length;
            zoneEditeur.AppendText(separation + bloc);
            zoneEditeur.Select(debut, zoneEditeur.TextLength - debut);
            zoneEditeur.ScrollToCaret();
            zoneEditeur.Focus();
            Statut("Instruction insérée et sélectionnée : F5 pour l'exécuter.");
        }

        // ----------------------------------------------------------------- Catalogue (complétion EF-16)

        private void PeuplerConnexionsEditeur()
        {
            pChargementConnexionsEditeur = true;
            listeConnexionsEditeur.Items.Clear();
            if (pParametres.Profils != null)
            {
                foreach (ConnexionProfil profil in pParametres.Profils)
                    listeConnexionsEditeur.Items.Add(profil);
            }
            pChargementConnexionsEditeur = false;
            // Pas de présélection : on évite toute connexion à l'ouverture de l'éditeur.
        }

        private void listeConnexionsEditeur_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pChargementConnexionsEditeur) return;
            pProfilCatalogue = listeConnexionsEditeur.SelectedItem as ConnexionProfil;
            pProviderCatalogue = pProfilCatalogue != null ? ConnexionProviderFactory.Creer(pProfilCatalogue) : null;
            ChargerSchemasEditeur();
        }

        private void ChargerSchemasEditeur()
        {
            listeSchemasEditeur.Items.Clear();
            if (pProviderCatalogue == null || pProfilCatalogue == null) return;

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                IReadOnlyList<string> schemas = pProviderCatalogue.ListerSchemas(pProfilCatalogue);
                foreach (string schema in schemas) listeSchemasEditeur.Items.Add(schema);
                etiquetteStatut.Text = schemas.Count + " schéma(s) ; choisissez-en un puis « Charger ce schéma ».";
            }
            catch (Exception ex)
            {
                etiquetteStatut.Text = "Catalogue indisponible : " + ex.Message;
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void boutonChargerCatalogue_Click(object sender, EventArgs e)
        {
            if (pProviderCatalogue == null || pProfilCatalogue == null)
            {
                etiquetteStatut.Text = "Sélectionnez d'abord une connexion.";
                return;
            }
            string schema = listeSchemasEditeur.SelectedItem as string;
            if (schema == null)
            {
                etiquetteStatut.Text = "Sélectionnez un schéma.";
                return;
            }

            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                List<string> identifiants = new List<string>();
                foreach (TableInfo table in pProviderCatalogue.ListerTables(pProfilCatalogue, schema))
                {
                    if (!string.IsNullOrEmpty(table.Nom)) identifiants.Add(table.Nom);
                }
                // Noms (complétion) et types (OPT-09) en une seule lecture du catalogue.
                Dictionary<string, CategorieType?> types = new Dictionary<string, CategorieType?>(StringComparer.OrdinalIgnoreCase);
                foreach (ColonneInfo colonne in pProviderCatalogue.ListerTypesColonnes(pProfilCatalogue, schema))
                {
                    CategorieType categorie = TypesSql.Categorie(colonne.TypeDonnee);
                    CategorieType? connu;
                    if (!types.TryGetValue(colonne.Nom, out connu))
                    {
                        types[colonne.Nom] = categorie;
                        identifiants.Add(colonne.Nom);
                    }
                    else if (connu.HasValue && connu.Value != categorie)
                    {
                        types[colonne.Nom] = null; // Même nom, types différents selon les tables : ambigu, OPT-09 se tait.
                    }
                }
                pTypesCatalogue = types;

                pIdentifiantsCatalogue = identifiants;
                etiquetteStatut.Text = identifiants.Count + " identifiant(s) du schéma " + schema + " chargé(s) pour la complétion.";
            }
            catch (Exception ex)
            {
                etiquetteStatut.Text = "Échec du chargement : " + ex.Message;
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void Analyser()
        {
            pResultats.Clear();
            listeAnalyse.Items.Clear();
            string sql = zoneEditeur.Text;

            foreach (IRegleAnalyse regle in pRegles)
            {
                foreach (ResultatAnalyse resultat in regle.Analyser(sql))
                {
                    pResultats.Add(resultat);
                    listeAnalyse.Items.Add(FormaterResultat(resultat));
                }
            }

            etiquetteStatut.Text = pResultats.Count == 0
                ? "Analyse : aucun problème détecté."
                : "Analyse : " + pResultats.Count + " résultat(s).";
            ongletsBas.SelectedTab = ongletAnalyse;
        }

        // ----------------------------------------------------------------- Exécution (EF-22 à EF-32)

        private void boutonExecuter_Click(object sender, EventArgs e)
        {
            Executer();
        }

        private void FormEditeurSql_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 && !e.Control && !e.Shift && !e.Alt)
            {
                MasquerPopup();
                Executer();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Exécute la sélection, sinon tout le texte, à condition qu'il ne contienne qu'une
        /// instruction. SELECT : en lecture seule forcée. DML / DDL : table cible, journalisation,
        /// comptage préalable, confirmation graduée, puis transaction si la table est journalisée.
        /// </summary>
        private async void Executer()
        {
            if (pExecutionEnCours) return;
            if (pTransactionEnCours != null)
            {
                Statut("Validez (COMMIT) ou annulez (ROLLBACK) d'abord la transaction en cours.");
                return;
            }

            string texte = zoneEditeur.SelectionLength > 0 ? zoneEditeur.SelectedText : zoneEditeur.Text;
            IReadOnlyList<string> instructions = DecoupeurInstructions.Decouper(texte);
            if (instructions.Count == 0)
            {
                Statut("Rien à exécuter.");
                return;
            }
            if (instructions.Count > 1)
            {
                Statut(instructions.Count + " instructions : sélectionnez celle à exécuter (une à la fois).");
                return;
            }
            string sql = instructions[0];

            ConnexionProfil profil = pProfilCatalogue;
            IConnexionProvider provider = pProviderCatalogue;
            if (profil == null || provider == null)
            {
                Statut("Choisissez d'abord une connexion (liste « Connexion / schéma »).");
                listeConnexionsEditeur.Focus();
                return;
            }

            TypeInstructionSql type = AnalyseurInstruction.DetecterType(sql);
            if (type != TypeInstructionSql.Select && profil.LectureSeule)
            {
                MessageBox.Show(this,
                    "Le profil « " + profil.Nom + " » est en lecture seule (EF-06) : seules les consultations sont permises." + Environment.NewLine + Environment.NewLine +
                    "Pour modifier des données, décochez « Lecture seule » dans Connexions > Gérer les connexions.",
                    "Modification refusée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int limite = (int)champLimite.Value;
            DebuterExecution();
            try
            {
                if (type == TypeInstructionSql.Select)
                {
                    Statut("Exécution de la requête…");
                    Stopwatch chrono = Stopwatch.StartNew();
                    ResultatRequete resultat = await Task.Run(() => provider.ExecuterSelect(profil, sql, limite));
                    chrono.Stop();
                    AfficherResultat(resultat);
                    Statut(resultat.Lignes.Count.ToString("N0", CultureInfo.CurrentCulture) + " ligne(s)"
                        + (resultat.Tronque ? " — limite atteinte, résultat TRONQUÉ" : string.Empty)
                        + " en " + Duree(chrono) + ".");
                    return;
                }

                await ExecuterModification(profil, provider, sql, type);
            }
            catch (Exception ex)
            {
                Statut("Erreur : " + ex.Message);
                MessageBox.Show(this, ex.Message, "Erreur d'exécution", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                TerminerExecution();
            }
        }

        private async Task ExecuterModification(ConnexionProfil profil, IConnexionProvider provider, string sql, TypeInstructionSql type)
        {
            // EF-30 : table cible et journalisation. Table non identifiée = non journalisée (prudence).
            Statut("Analyse de l'instruction…");
            TableCible cible = TableCible.Extraire(sql, profil.BibliothequeParDefaut);
            bool journalisee = cible != null
                && await Task.Run(() => provider.EstJournalisee(profil, cible.Schema, cible.Nom));

            // Transaction seulement pour un DML sur table journalisée (le DDL et CALL valident immédiatement).
            bool estDml = type == TypeInstructionSql.Insert || type == TypeInstructionSql.Update
                || type == TypeInstructionSql.Delete || (type == TypeInstructionSql.Autre && cible != null);
            bool sousControle = estDml && journalisee;
            EvaluationRisque evaluation = AnalyseurInstruction.Evaluer(sql, journalisee);

            // EF-26 : comptage préalable des lignes visées par un UPDATE / DELETE.
            long? lignesEstimees = null;
            string comptage = AnalyseurInstruction.ConstruireComptage(sql);
            if (comptage != null)
            {
                Statut("Comptage des lignes concernées…");
                try
                {
                    lignesEstimees = await Task.Run(() => provider.CompterLignes(profil, comptage));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("Comptage préalable impossible : " + ex.Message);
                }
            }

            string titre = "Confirmer — risque " + MessageConfirmation.LibelleNiveau(evaluation.Niveau).ToLowerInvariant();
            MessageBoxIcon icone = evaluation.Niveau >= NiveauRisque.Eleve ? MessageBoxIcon.Warning : MessageBoxIcon.Question;
            if (MessageBox.Show(this, MessageConfirmation.Construire(sql, evaluation, cible, lignesEstimees, sousControle),
                    titre, MessageBoxButtons.YesNo, icone, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                Statut("Exécution abandonnée.");
                return;
            }
            if (evaluation.ConfirmationRenforcee
                && MessageBox.Show(this, MessageConfirmation.ConstruireRenforcee(evaluation, lignesEstimees, sousControle),
                    "Dernière confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Stop, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                Statut("Exécution abandonnée.");
                return;
            }

            Statut("Exécution…");
            ITransactionExecution transaction = await Task.Run(() => provider.ExecuterModification(profil, sql, sousControle));
            if (transaction.SousControleValidation)
            {
                pTransactionEnCours = transaction;
                pDescriptionTransaction = LibelleLignes(transaction.LignesAffectees) + " sur " + cible;
                AfficherTransaction();
                return;
            }

            int lignes = transaction.LignesAffectees;
            transaction.Dispose();
            Statut(LibelleLignes(lignes) + " — validé immédiatement (sans retour arrière possible).");
        }

        private void boutonValider_Click(object sender, EventArgs e)
        {
            TerminerTransaction(true);
        }

        private void boutonAnnuler_Click(object sender, EventArgs e)
        {
            TerminerTransaction(false);
        }

        /// <summary>COMMIT ou ROLLBACK de la transaction en cours ; retourne faux en cas d'échec.</summary>
        private bool TerminerTransaction(bool valider)
        {
            if (pTransactionEnCours == null) return true;
            try
            {
                if (valider) pTransactionEnCours.Valider();
                else pTransactionEnCours.Annuler();
                pTransactionEnCours.Dispose();
                pTransactionEnCours = null;
                panneauTransaction.Visible = false;
                Statut((valider ? "COMMIT effectué : " : "ROLLBACK effectué : ") + pDescriptionTransaction
                    + (valider ? " validé(es)." : " annulé(es)."));
                return true;
            }
            catch (Exception ex)
            {
                Statut("Échec du " + (valider ? "COMMIT" : "ROLLBACK") + " : " + ex.Message);
                MessageBox.Show(this, ex.Message, "Échec du " + (valider ? "COMMIT" : "ROLLBACK"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void FormEditeurSql_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (pExecutionEnCours)
            {
                MessageBox.Show(this, "Une instruction est en cours d'exécution : attendez sa fin avant de fermer.",
                    "Fermeture impossible", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }
            if (pTransactionEnCours == null)
            {
                // EF-33 : un script modifié n'est pas perdu sans confirmation.
                if (!ConfirmerAbandon()) e.Cancel = true;
                return;
            }

            DialogResult choix = MessageBox.Show(this,
                "Une transaction est en attente : " + pDescriptionTransaction + "." + Environment.NewLine + Environment.NewLine +
                "Oui : valider (COMMIT)" + Environment.NewLine +
                "Non : annuler (ROLLBACK)" + Environment.NewLine +
                "Annuler : ne pas fermer",
                "Transaction en attente", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (choix == DialogResult.Cancel || !TerminerTransaction(choix == DialogResult.Yes))
            {
                e.Cancel = true;
                return;
            }
            if (!ConfirmerAbandon()) e.Cancel = true;
        }

        private void AfficherTransaction()
        {
            etiquetteTransaction.Text = "Transaction EN ATTENTE : " + pDescriptionTransaction + "." + Environment.NewLine
                + "Les lignes restent verrouillées pour les autres utilisateurs d'Atlas jusqu'au COMMIT ou ROLLBACK.";
            panneauTransaction.Visible = true;
            Statut("Exécuté : " + pDescriptionTransaction + " — à valider ou annuler.");
        }

        private void AfficherResultat(ResultatRequete resultat)
        {
            grilleResultats.DataSource = null;
            grilleResultats.DataSource = ConstruireTable(resultat);
            grilleResultats.DefaultCellStyle.NullValue = "NULL";
            foreach (DataGridViewColumn colonne in grilleResultats.Columns)
                colonne.AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            ongletsBas.SelectedTab = ongletResultats;
        }

        /// <summary>
        /// Convertit le résultat en DataTable (liaison rapide et tri par colonne). Les noms en
        /// double (jointures) sont suffixés ; le type d'une colonne est celui de ses valeurs s'il
        /// est homogène, sinon object.
        /// </summary>
        internal static DataTable ConstruireTable(ResultatRequete resultat)
        {
            DataTable table = new DataTable("Resultat");
            table.Locale = CultureInfo.CurrentCulture;
            HashSet<string> nomsPris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < resultat.Colonnes.Count; i++)
            {
                string nom = string.IsNullOrEmpty(resultat.Colonnes[i]) ? "COL" + (i + 1) : resultat.Colonnes[i];
                string nomUnique = nom;
                for (int n = 2; !nomsPris.Add(nomUnique); n++) nomUnique = nom + "_" + n;
                table.Columns.Add(nomUnique, TypeHomogene(resultat, i));
            }

            table.BeginLoadData();
            foreach (object[] ligne in resultat.Lignes)
            {
                object[] valeurs = new object[ligne.Length];
                for (int i = 0; i < ligne.Length; i++) valeurs[i] = ligne[i] ?? DBNull.Value;
                table.Rows.Add(valeurs);
            }
            table.EndLoadData();
            return table;
        }

        private static Type TypeHomogene(ResultatRequete resultat, int index)
        {
            Type type = null;
            foreach (object[] ligne in resultat.Lignes)
            {
                object valeur = ligne[index];
                if (valeur == null) continue;
                if (type == null) type = valeur.GetType();
                else if (type != valeur.GetType()) return typeof(object);
            }
            return type ?? typeof(string);
        }

        private void DebuterExecution()
        {
            pExecutionEnCours = true;
            boutonExecuter.Enabled = false;
            UseWaitCursor = true;
        }

        private void TerminerExecution()
        {
            pExecutionEnCours = false;
            boutonExecuter.Enabled = true;
            UseWaitCursor = false;
        }

        private static string LibelleLignes(int lignes)
        {
            // Un DDL renvoie -1 : pas de nombre de lignes significatif.
            return lignes < 0 ? "instruction exécutée" : lignes.ToString("N0", CultureInfo.CurrentCulture) + " ligne(s)";
        }

        private static string Duree(Stopwatch chrono)
        {
            return chrono.Elapsed.TotalSeconds < 1
                ? chrono.ElapsedMilliseconds + " ms"
                : chrono.Elapsed.TotalSeconds.ToString("0.0", CultureInfo.CurrentCulture) + " s";
        }

        private void Statut(string message)
        {
            etiquetteStatut.Text = message;
        }

        private static string FormaterResultat(ResultatAnalyse resultat)
        {
            string texte = resultat.CodeRegle + " [" + resultat.Severite + "] " + resultat.Message;
            if (!string.IsNullOrEmpty(resultat.Suggestion))
                texte += "  → " + resultat.Suggestion;
            return texte;
        }

        private void listeAnalyse_DoubleClick(object sender, EventArgs e)
        {
            MasquerPopup();
            int index = listeAnalyse.SelectedIndex;
            if (index < 0 || index >= pResultats.Count) return;

            ResultatAnalyse resultat = pResultats[index];
            if (resultat.Position >= 0 && resultat.Position <= zoneEditeur.TextLength)
            {
                int longueur = resultat.Longueur > 0 ? resultat.Longueur : 0;
                zoneEditeur.Select(resultat.Position, longueur);
                zoneEditeur.ScrollToCaret();
                zoneEditeur.Focus();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Core.Edition;
using SQLUtility.Core.Optimisation;
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

        public FormEditeurSql(ParametresApplication parametres)
        {
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));
            pParametres = parametres;
            InitializeComponent();
            pRegles = new List<IRegleAnalyse> { new RegleSelectEtoile() };
            InitialiserCompletion();
            PeuplerConnexionsEditeur();
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
                identifiants.AddRange(pProviderCatalogue.ListerNomsColonnes(pProfilCatalogue, schema));

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

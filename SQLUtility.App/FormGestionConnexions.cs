using System;
using System.Drawing;
using System.Windows.Forms;
using SQLUtility.Core.Connexions;
using SQLUtility.Data.Parametres;
using SQLUtility.Data.Providers;
using SQLUtility.Data.Securite;

namespace SQLUtility.App
{
    /// <summary>
    /// Écran maître-détail de gestion des profils de connexion (EF-03 à EF-06).
    /// Liste des profils à gauche, édition à droite ; stockage chiffré DPAPI,
    /// mode lecture seule, test de connexion.
    /// </summary>
    public partial class FormGestionConnexions : Form
    {
        private readonly GestionnaireParametres pGestionnaire;
        private readonly ParametresApplication pParametres;

        // Profil en cours d'édition (null = saisie d'un nouveau profil).
        private ConnexionProfil pProfilCourant;

        // Empêche le traitement de SelectedIndexChanged pendant le remplissage de la liste.
        private bool pChargementListe;

        public FormGestionConnexions(GestionnaireParametres gestionnaire, ParametresApplication parametres)
        {
            if (gestionnaire == null) throw new ArgumentNullException(nameof(gestionnaire));
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));

            pGestionnaire = gestionnaire;
            pParametres = parametres;

            InitializeComponent();
            RafraichirListe(null);
            AfficherDefautDansStatut();
        }

        // ----------------------------------------------------------------- Liste

        private void RafraichirListe(ConnexionProfil aSelectionner)
        {
            pChargementListe = true;
            listeProfils.Items.Clear();
            if (pParametres.Profils != null)
            {
                foreach (ConnexionProfil profil in pParametres.Profils)
                    listeProfils.Items.Add(profil);
            }
            pChargementListe = false;

            if (aSelectionner != null && listeProfils.Items.Contains(aSelectionner))
                listeProfils.SelectedItem = aSelectionner;
            else if (listeProfils.Items.Count > 0)
                listeProfils.SelectedIndex = 0;
            else
            {
                pProfilCourant = null;
                ChargerChamps(null);
            }
        }

        private void listeProfils_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pChargementListe) return;
            pProfilCourant = listeProfils.SelectedItem as ConnexionProfil;
            ChargerChamps(pProfilCourant);
        }

        // ----------------------------------------------------------------- Champs

        private void ChargerChamps(ConnexionProfil profil)
        {
            if (profil == null)
            {
                champNom.Text = string.Empty;
                champServeur.Text = string.Empty;
                champUtilisateur.Text = string.Empty;
                champMotDePasse.Text = string.Empty;
                champBibliotheque.Text = string.Empty;
                caseLectureSeule.Checked = true;
                champDescription.Text = string.Empty;
                return;
            }

            champNom.Text = profil.Nom;
            champServeur.Text = profil.Serveur;
            champUtilisateur.Text = profil.Utilisateur;
            champMotDePasse.Text = string.Empty; // Vide = secret inchangé (jamais déchiffré pour affichage).
            champBibliotheque.Text = profil.BibliothequeParDefaut;
            caseLectureSeule.Checked = profil.LectureSeule;
            champDescription.Text = profil.Description;
        }

        /// <summary>Construit un profil à partir des champs ; conserve le secret existant si non saisi.</summary>
        private ConnexionProfil ConstruireDepuisChamps(ConnexionProfil cible)
        {
            ConnexionProfil profil = cible ?? new ConnexionProfil();
            profil.Nom = (champNom.Text ?? string.Empty).Trim();
            profil.Serveur = (champServeur.Text ?? string.Empty).Trim();
            profil.Utilisateur = (champUtilisateur.Text ?? string.Empty).Trim();
            profil.BibliothequeParDefaut = (champBibliotheque.Text ?? string.Empty).Trim();
            profil.LectureSeule = caseLectureSeule.Checked;
            profil.Description = champDescription.Text;
            profil.Type = TypeSgbd.Db2ForI;

            // Secret : seulement s'il a été saisi ; sinon on garde la valeur existante. (EF-05)
            if (champMotDePasse.Text.Length > 0)
                profil.SecretChiffre = ProtecteurSecret.Chiffrer(champMotDePasse.Text);

            return profil;
        }

        private bool ValiderSaisie()
        {
            if (string.IsNullOrWhiteSpace(champNom.Text))
            {
                Avertir("Le nom du profil est obligatoire.");
                champNom.Focus();
                return false;
            }
            if (string.IsNullOrWhiteSpace(champServeur.Text))
            {
                Avertir("Le serveur (nom système ou IP) est obligatoire.");
                champServeur.Focus();
                return false;
            }

            string nom = champNom.Text.Trim();
            if (pParametres.Profils != null)
            {
                foreach (ConnexionProfil autre in pParametres.Profils)
                {
                    if (!ReferenceEquals(autre, pProfilCourant)
                        && string.Equals(autre.Nom, nom, StringComparison.OrdinalIgnoreCase))
                    {
                        Avertir("Un autre profil porte déjà le nom « " + nom + " ».");
                        champNom.Focus();
                        return false;
                    }
                }
            }
            return true;
        }

        // ----------------------------------------------------------------- Boutons

        private void boutonNouveau_Click(object sender, EventArgs e)
        {
            pProfilCourant = null;
            listeProfils.ClearSelected();
            ChargerChamps(null);
            champNom.Focus();
            Informer("Saisie d'un nouveau profil.");
        }

        private void boutonEnregistrer_Click(object sender, EventArgs e)
        {
            if (!ValiderSaisie()) return;

            bool creation = pProfilCourant == null;
            ConnexionProfil profil = ConstruireDepuisChamps(pProfilCourant);

            if (creation)
            {
                pParametres.Profils.Add(profil);
                pProfilCourant = profil;
            }

            pGestionnaire.Enregistrer(pParametres);
            RafraichirListe(profil);
            Informer(creation ? "Profil créé et enregistré." : "Profil enregistré.");
        }

        private void boutonSupprimer_Click(object sender, EventArgs e)
        {
            if (pProfilCourant == null)
            {
                Avertir("Aucun profil sélectionné.");
                return;
            }

            DialogResult reponse = MessageBox.Show(
                this,
                "Supprimer le profil « " + pProfilCourant.Nom + " » ?",
                "Confirmation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (reponse != DialogResult.Yes) return;

            string nomSupprime = pProfilCourant.Nom;
            pParametres.Profils.Remove(pProfilCourant);
            if (string.Equals(pParametres.ProfilParDefaut, nomSupprime, StringComparison.OrdinalIgnoreCase))
                pParametres.ProfilParDefaut = null;

            pProfilCourant = null;
            pGestionnaire.Enregistrer(pParametres);
            RafraichirListe(null);
            Informer("Profil « " + nomSupprime + " » supprimé.");
        }

        private void boutonTester_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(champServeur.Text))
            {
                Avertir("Renseignez au moins le serveur pour tester.");
                return;
            }

            // Teste ce qui est à l'écran (profil transitoire, non enregistré). (EF-04)
            ConnexionProfil profil = ConstruireDepuisChamps(null);
            if (champMotDePasse.Text.Length == 0 && pProfilCourant != null)
                profil.SecretChiffre = pProfilCourant.SecretChiffre; // Réutilise le secret enregistré.

            Cursor curseurInitial = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                IConnexionProvider provider = ConnexionProviderFactory.Creer(profil);
                ResultatTestConnexion resultat = provider.TesterConnexion(profil);
                if (resultat.Succes)
                    Informer("Connexion réussie (" + (int)resultat.Duree.TotalMilliseconds + " ms).");
                else
                    Avertir("Échec : " + resultat.Message);
            }
            finally
            {
                Cursor = curseurInitial;
            }
        }

        private void boutonDefaut_Click(object sender, EventArgs e)
        {
            if (pProfilCourant == null)
            {
                Avertir("Sélectionnez un profil à définir par défaut.");
                return;
            }

            pParametres.ProfilParDefaut = pProfilCourant.Nom;
            pGestionnaire.Enregistrer(pParametres);
            AfficherDefautDansStatut();
        }

        private void boutonFermer_Click(object sender, EventArgs e)
        {
            Close();
        }

        // ----------------------------------------------------------------- Statut

        private void AfficherDefautDansStatut()
        {
            etiquetteStatut.ForeColor = SystemColors.ControlText;
            etiquetteStatut.Text = string.IsNullOrEmpty(pParametres.ProfilParDefaut)
                ? "Aucun profil par défaut."
                : "Profil par défaut : " + pParametres.ProfilParDefaut;
        }

        private void Informer(string message)
        {
            etiquetteStatut.ForeColor = Color.FromArgb(0, 110, 0);
            etiquetteStatut.Text = message;
        }

        private void Avertir(string message)
        {
            etiquetteStatut.ForeColor = Color.FromArgb(176, 0, 0);
            etiquetteStatut.Text = message;
        }
    }
}

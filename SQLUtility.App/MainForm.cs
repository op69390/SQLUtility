using System;
using System.Windows.Forms;
using SQLUtility.Data.Parametres;

namespace SQLUtility.App
{
    /// <summary>Fenêtre principale de l'outil SQLUtility.</summary>
    public partial class MainForm : Form
    {
        private readonly GestionnaireParametres pGestionnaire;
        private readonly ParametresApplication pParametres;

        public MainForm(GestionnaireParametres gestionnaire, ParametresApplication parametres)
        {
            if (gestionnaire == null) throw new ArgumentNullException(nameof(gestionnaire));
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));

            pGestionnaire = gestionnaire;
            pParametres = parametres;

            InitializeComponent();
            AfficherEtat();
        }

        /// <summary>Renseigne la barre d'état et la zone d'accueil à partir des paramètres chargés.</summary>
        private void AfficherEtat()
        {
            etiquetteFichier.Text = "Fichier de paramètres : " + pGestionnaire.CheminFichier;
            int nbProfils = pParametres.Profils != null ? pParametres.Profils.Count : 0;
            etatProfils.Text = string.IsNullOrEmpty(pParametres.ProfilParDefaut)
                ? "Profils : " + nbProfils
                : "Profils : " + nbProfils + " · défaut : " + pParametres.ProfilParDefaut;

            zoneAccueil.Text =
                "SQLUtility — squelette opérationnel." + Environment.NewLine + Environment.NewLine +
                "Cible : IBM DB2 for i (V7R5), ERP Atlas / MAPICS." + Environment.NewLine +
                "Provider d'accès : ODBC (par défaut)." + Environment.NewLine + Environment.NewLine +
                "Avancement (backlog) :" + Environment.NewLine +
                "  [x] 1. Détection de l'état de journalisation (EF-30)." + Environment.NewLine +
                "  [x] 2. Conversions de dates (les 7 formats)." + Environment.NewLine +
                "  [ ] 3. Explorateur de catalogue (EF-07 / EF-08)." + Environment.NewLine +
                "  [~] 4. Gestion des connexions (EF-03 à EF-06) — menu Connexions.";
        }

        private void menuFichierQuitter_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuConnexionsGerer_Click(object sender, EventArgs e)
        {
            using (FormGestionConnexions dialogue = new FormGestionConnexions(pGestionnaire, pParametres))
            {
                dialogue.ShowDialog(this);
            }
            AfficherEtat();
        }

        private void menuConnexionsExplorer_Click(object sender, EventArgs e)
        {
            using (FormExplorateurCatalogue explorateur = new FormExplorateurCatalogue(pParametres))
            {
                explorateur.ShowDialog(this);
            }
        }

        private void menuEditeurNouveau_Click(object sender, EventArgs e)
        {
            // Éditeur ouvert en mode non modal (fenêtre de travail) ; se libère à sa fermeture.
            FormEditeurSql editeur = new FormEditeurSql(pParametres);
            editeur.Show(this);
        }
    }
}

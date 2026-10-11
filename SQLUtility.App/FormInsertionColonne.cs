using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SQLUtility.Core.Catalogue;
using SQLUtility.Core.Connexions;
using SQLUtility.Core.Dates;
using SQLUtility.Data.Parametres;
using SQLUtility.Data.Providers;

namespace SQLUtility.App
{
    /// <summary>
    /// Dialogue d'insertion d'une colonne dans l'éditeur (EF-17/18) : connexion → schéma →
    /// table → colonne. Si la colonne est détectée comme date encodée, propose le format et
    /// insère l'expression de décodage SQL ; sinon insère le nom de la colonne.
    /// </summary>
    public partial class FormInsertionColonne : Form
    {
        private readonly ParametresApplication pParametres;
        private ConnexionProfil pProfil;
        private IConnexionProvider pProvider;

        /// <summary>Texte à insérer dans l'éditeur (renseigné si l'utilisateur valide).</summary>
        public string TexteAInserer { get; private set; }

        public FormInsertionColonne(ParametresApplication parametres)
        {
            if (parametres == null) throw new ArgumentNullException(nameof(parametres));
            pParametres = parametres;
            InitializeComponent();

            foreach (DescriptionFormatDate description in ReferentielFormatsDate.Tous)
                listeFormats.Items.Add(description);

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

        private void listeConnexions_SelectedIndexChanged(object sender, EventArgs e)
        {
            pProfil = listeConnexions.SelectedItem as ConnexionProfil;
            pProvider = pProfil != null ? ConnexionProviderFactory.Creer(pProfil) : null;
            ChargerSchemas();
        }

        // ----------------------------------------------------------------- Chargements

        private void ChargerSchemas()
        {
            listeSchemas.Items.Clear();
            listeTables.Items.Clear();
            listeColonnes.Items.Clear();
            if (pProvider == null || pProfil == null) return;

            Executer(() =>
            {
                IReadOnlyList<string> schemas = pProvider.ListerSchemas(pProfil);
                foreach (string schema in schemas) listeSchemas.Items.Add(schema);
                Statut(schemas.Count + " schéma(s).");
            });
        }

        private void listeSchemas_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargerTables();
        }

        private void ChargerTables()
        {
            listeTables.Items.Clear();
            listeColonnes.Items.Clear();
            string schema = listeSchemas.SelectedItem as string;
            if (pProvider == null || pProfil == null || schema == null) return;

            Executer(() =>
            {
                IReadOnlyList<TableInfo> tables = pProvider.ListerTables(pProfil, schema);
                foreach (TableInfo table in tables) listeTables.Items.Add(table);
                Statut(tables.Count + " table(s) dans " + schema + ".");
            });
        }

        private void listeTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChargerColonnes();
        }

        private void ChargerColonnes()
        {
            listeColonnes.Items.Clear();
            TableInfo table = listeTables.SelectedItem as TableInfo;
            if (pProvider == null || pProfil == null || table == null) return;

            Executer(() =>
            {
                IReadOnlyList<ColonneInfo> colonnes = pProvider.ListerColonnes(pProfil, table.Schema, table.Nom);
                foreach (ColonneInfo colonne in colonnes) listeColonnes.Items.Add(colonne);
                Statut(colonnes.Count + " colonne(s).");
            });
        }

        // ----------------------------------------------------------------- Sélection colonne

        private void listeColonnes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ColonneInfo colonne = listeColonnes.SelectedItem as ColonneInfo;
            if (colonne == null)
            {
                etiquetteDetail.Text = string.Empty;
                groupeDate.Enabled = false;
                caseConvertir.Checked = false;
                MajApercu();
                return;
            }

            etiquetteDetail.Text = "Type : " + colonne.TypeDonnee
                + "  |  longueur " + colonne.Longueur
                + "  |  échelle " + colonne.Echelle
                + (string.IsNullOrEmpty(colonne.Libelle) ? string.Empty : "  |  " + colonne.Libelle);

            ResultatDetectionDate detection = DetecteurFormatDate.Detecter(colonne);
            groupeDate.Enabled = detection.EstProbablementDate;
            caseConvertir.Checked = detection.EstProbablementDate;
            if (detection.EstProbablementDate)
                SelectionnerFormat(detection.FormatParDefaut);

            MajApercu();
        }

        private void SelectionnerFormat(FormatDate format)
        {
            for (int i = 0; i < listeFormats.Items.Count; i++)
            {
                DescriptionFormatDate description = listeFormats.Items[i] as DescriptionFormatDate;
                if (description != null && description.Format == format)
                {
                    listeFormats.SelectedIndex = i;
                    return;
                }
            }
        }

        private void caseConvertir_CheckedChanged(object sender, EventArgs e)
        {
            listeFormats.Enabled = caseConvertir.Checked;
            MajApercu();
        }

        private void listeFormats_SelectedIndexChanged(object sender, EventArgs e)
        {
            MajApercu();
        }

        private void MajApercu()
        {
            champApercu.Text = ConstruireTexte();
        }

        private string ConstruireTexte()
        {
            ColonneInfo colonne = listeColonnes.SelectedItem as ColonneInfo;
            if (colonne == null) return string.Empty;

            DescriptionFormatDate description = listeFormats.SelectedItem as DescriptionFormatDate;
            if (groupeDate.Enabled && caseConvertir.Checked && description != null)
                return GenerateurExpressionDate.Generer(colonne.Nom, description.Format) + " AS " + colonne.Nom;

            return colonne.Nom;
        }

        // ----------------------------------------------------------------- Validation

        private void boutonInserer_Click(object sender, EventArgs e)
        {
            if (listeColonnes.SelectedItem == null)
            {
                Statut("Sélectionnez une colonne à insérer.");
                return;
            }
            TexteAInserer = ConstruireTexte();
            DialogResult = DialogResult.OK;
            Close();
        }

        // ----------------------------------------------------------------- Utilitaires

        private void Executer(Action action)
        {
            Cursor curseur = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Statut("Erreur : " + ex.Message);
            }
            finally
            {
                Cursor = curseur;
            }
        }

        private void Statut(string message)
        {
            etiquetteStatut.Text = message;
        }
    }
}

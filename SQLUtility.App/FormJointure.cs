using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SQLUtility.Core.Assistant;
using SQLUtility.Core.Catalogue;

namespace SQLUtility.App
{
    /// <summary>Usage de la fenêtre de liaison entre une table et la requête.</summary>
    public enum ModeLiaison
    {
        /// <summary>Jointure INNER / LEFT OUTER (niveau 2).</summary>
        Jointure,

        /// <summary>Condition d'existence : EXISTS (radio « interne ») / NOT EXISTS (radio « externe »). (EF-48)</summary>
        Existence,

        /// <summary>Table source des valeurs d'une modification par lot : pas de choix de type. (EF-49)</summary>
        Source
    }

    /// <summary>
    /// Liaison d'une table à la requête : jointure (niveau 2, EF-44 / EF-45), condition
    /// d'existence ou source de valeurs (niveau 4, EF-48 / EF-49). Propositions du
    /// <see cref="DetecteurJointures"/> vis-à-vis de chaque table déjà présente, puis
    /// conditions ajustables à la main.
    /// </summary>
    public partial class FormJointure : Form
    {
        private readonly List<TableRequete> pPresentes;
        private readonly TableRequete pNouvelle;
        private readonly ModeLiaison pMode;
        private readonly Dictionary<string, ColonneInfo> pColonnesJointes = new Dictionary<string, ColonneInfo>();
        private readonly Dictionary<string, ColonneInfo> pColonnesLiees = new Dictionary<string, ColonneInfo>();
        private OrigineJointure pOrigine = OrigineJointure.Manuelle;
        private bool pRemplissage;

        /// <param name="presentes">Tables déjà dans la requête (la nouvelle ne peut se lier qu'à elles).</param>
        /// <param name="existante">Jointure à modifier, ou null pour une nouvelle jointure.</param>
        public FormJointure(IList<TableRequete> presentes, TableRequete nouvelle, Jointure existante)
            : this(presentes, nouvelle, existante, ModeLiaison.Jointure)
        {
        }

        /// <param name="existante">
        /// Liaison à modifier, ou null. En mode existence, son type porte la négation :
        /// <see cref="TypeJointure.Interne"/> = EXISTS, <see cref="TypeJointure.ExterneGauche"/> = NOT EXISTS.
        /// </param>
        public FormJointure(IList<TableRequete> presentes, TableRequete nouvelle, Jointure existante, ModeLiaison mode)
        {
            if (presentes == null || presentes.Count == 0) throw new ArgumentException("Aucune table présente.", nameof(presentes));
            if (nouvelle == null) throw new ArgumentNullException(nameof(nouvelle));
            pPresentes = new List<TableRequete>(presentes);
            pNouvelle = nouvelle;
            pMode = mode;
            InitializeComponent();

            TableInfo table = nouvelle.Table;
            string designation = nouvelle.Alias + " — " + table.NomQualifie
                + (string.IsNullOrEmpty(table.Libelle) ? string.Empty : " (" + table.Libelle + ")");
            switch (mode)
            {
                case ModeLiaison.Existence:
                    Text = "Condition d'existence";
                    etiquetteTitre.Text = "Condition d'existence dans " + designation;
                    radioInterne.Text = "Garder les lignes qui ont au moins une ligne liée dans " + nouvelle.Alias + " (EXISTS)";
                    radioExterne.Text = "Garder les lignes qui n'ont AUCUNE ligne liée dans " + nouvelle.Alias + " (NOT EXISTS)";
                    boutonOk.Text = "Ajouter";
                    break;
                case ModeLiaison.Source:
                    Text = "Table source des valeurs";
                    etiquetteTitre.Text = "Valeurs prises dans " + designation;
                    radioInterne.Visible = radioExterne.Visible = false;
                    boutonOk.Text = "Utiliser";
                    break;
                default:
                    etiquetteTitre.Text = "Joindre " + designation;
                    break;
            }
            colJointe.HeaderText = "Colonne de " + nouvelle.Alias + " (" + table.Nom + ")";
            foreach (ColonneInfo colonne in nouvelle.Structure.Colonnes)
            {
                string libelle = Libelle(colonne);
                pColonnesJointes[libelle] = colonne;
                colJointe.Items.Add(libelle);
            }
            foreach (TableRequete presente in pPresentes) listeTableLiee.Items.Add(presente);

            ChargerPropositions();
            if (existante != null) Reprendre(existante);
            else if (listePropositions.Items.Count > 0 && listePropositions.Items[0] is ElementProposition) listePropositions.SelectedIndex = 0;
            else ChoisirTableLiee(pPresentes[0]);
        }

        /// <summary>Jointure définie (renseignée à la fermeture par « Joindre »).</summary>
        public Jointure Jointure { get; private set; }

        // ----------------------------------------------------------------- Propositions

        private void ChargerPropositions()
        {
            listePropositions.Items.Clear();
            foreach (TableRequete presente in pPresentes)
            {
                foreach (JointureProposee proposition in DetecteurJointures.Proposer(presente.Structure, pNouvelle.Structure))
                    listePropositions.Items.Add(new ElementProposition(presente, proposition));
            }
            if (listePropositions.Items.Count == 0)
                listePropositions.Items.Add("Aucune jointure détectée (ni clé étrangère, ni clé retrouvée par les noms) : définissez les conditions ci-dessous.");
        }

        private void listePropositions_SelectedIndexChanged(object sender, EventArgs e)
        {
            ElementProposition element = listePropositions.SelectedItem as ElementProposition;
            if (element == null) return;

            ChoisirTableLiee(element.Presente);
            pRemplissage = true;
            try
            {
                grilleConditions.Rows.Clear();
                foreach (KeyValuePair<ColonneInfo, ColonneInfo> paire in element.Proposition.Paires)
                    grilleConditions.Rows.Add(Libelle(paire.Value), Libelle(paire.Key));
            }
            finally
            {
                pRemplissage = false;
            }
            pOrigine = element.Proposition.Origine;
            // Jointure de recherche de libellé (au plus une ligne) : LEFT pour ne perdre aucune
            // ligne ; sinon INNER. En existence ou source, le choix de l'utilisateur est conservé.
            if (pMode == ModeLiaison.Jointure)
            {
                radioExterne.Checked = element.Proposition.AuPlusUneLigne;
                radioInterne.Checked = !element.Proposition.AuPlusUneLigne;
            }
            MettreAJourAvertissement();
        }

        private void Reprendre(Jointure existante)
        {
            TableRequete liee = existante.Conditions.Count > 0 ? existante.Conditions[0].TableLiee : pPresentes[0];
            ChoisirTableLiee(pPresentes.Contains(liee) ? liee : pPresentes[0]);
            pRemplissage = true;
            try
            {
                grilleConditions.Rows.Clear();
                foreach (ConditionJointure condition in existante.Conditions)
                    if (condition.TableLiee == liee) grilleConditions.Rows.Add(Libelle(condition.ColonneJointe), Libelle(condition.ColonneLiee));
            }
            finally
            {
                pRemplissage = false;
            }
            pOrigine = existante.Origine;
            radioExterne.Checked = existante.Type == TypeJointure.ExterneGauche;
            radioInterne.Checked = existante.Type == TypeJointure.Interne;
            listePropositions.ClearSelected();
            MettreAJourAvertissement();
        }

        // ----------------------------------------------------------------- Table liée et conditions

        private void listeTableLiee_SelectionChangeCommitted(object sender, EventArgs e)
        {
            TableRequete table = listeTableLiee.SelectedItem as TableRequete;
            if (table == null) return;
            grilleConditions.Rows.Clear();
            ChoisirTableLiee(table);
            pOrigine = OrigineJointure.Manuelle;
            listePropositions.ClearSelected();
            MettreAJourAvertissement();
        }

        private void ChoisirTableLiee(TableRequete table)
        {
            listeTableLiee.SelectedItem = table;
            colLiee.HeaderText = "= colonne de " + table.Alias + " (" + table.Table.Nom + ")";
            if (grilleConditions.Rows.Count > 1 || (grilleConditions.Rows.Count == 1 && !grilleConditions.Rows[0].IsNewRow))
                grilleConditions.Rows.Clear(); // Les valeurs de l'ancienne table ne seraient plus valides.
            colLiee.Items.Clear();
            pColonnesLiees.Clear();
            foreach (ColonneInfo colonne in table.Structure.Colonnes)
            {
                string libelle = Libelle(colonne);
                pColonnesLiees[libelle] = colonne;
                colLiee.Items.Add(libelle);
            }
        }

        private void grilleConditions_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (grilleConditions.IsCurrentCellDirty) grilleConditions.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void grilleConditions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // RowIndex -1 : changement d'en-tête de colonne (HeaderText), pas une saisie.
            if (pRemplissage || e.RowIndex < 0) return;
            // Conditions réellement retouchées : ce n'est plus la proposition d'origine. (L'entrée
            // en édition d'une liste déroulante valide la cellule sans en changer la valeur.)
            ElementProposition choisie = listePropositions.SelectedItem as ElementProposition;
            if (choisie == null || !ConditionsIdentiques(choisie.Proposition))
            {
                pOrigine = OrigineJointure.Manuelle;
                listePropositions.ClearSelected();
            }
            MettreAJourAvertissement();
        }

        /// <summary>Vrai si la grille contient exactement les paires de la proposition, dans l'ordre.</summary>
        private bool ConditionsIdentiques(JointureProposee proposition)
        {
            List<KeyValuePair<ColonneInfo, ColonneInfo>> lues = new List<KeyValuePair<ColonneInfo, ColonneInfo>>();
            foreach (DataGridViewRow ligne in grilleConditions.Rows)
            {
                if (ligne.IsNewRow) continue;
                ColonneInfo jointe, liee;
                if (!LireLigne(ligne, out jointe, out liee)) return false;
                lues.Add(new KeyValuePair<ColonneInfo, ColonneInfo>(liee, jointe));
            }
            if (lues.Count != proposition.Paires.Count) return false;
            for (int i = 0; i < lues.Count; i++)
                if (lues[i].Key != proposition.Paires[i].Key || lues[i].Value != proposition.Paires[i].Value) return false;
            return true;
        }

        private void grilleConditions_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void MettreAJourAvertissement()
        {
            List<string> messages = new List<string>();
            // EXISTS ne multiplie jamais les lignes : la clé partielle n'y pose pas de problème.
            if (pOrigine == OrigineJointure.ClePartielleDeduite && pMode == ModeLiaison.Jointure)
                messages.Add("Clé partielle : une ligne peut correspondre à plusieurs lignes de " + pNouvelle.Alias + " (résultat multiplié).");
            if (pOrigine == OrigineJointure.ClePartielleDeduite && pMode == ModeLiaison.Source)
                messages.Add("Clé partielle : plusieurs lignes sources possibles ; l'exécution échouerait alors (SQL0811) sans rien modifier.");
            foreach (DataGridViewRow ligne in grilleConditions.Rows)
            {
                ColonneInfo jointe, liee;
                if (!LireLigne(ligne, out jointe, out liee)) continue;
                if (!DetecteurJointures.TypesCompatibles(jointe, liee))
                    messages.Add(jointe.Nom + " (" + jointe.TypeDonnee + ") et " + liee.Nom + " (" + liee.TypeDonnee + ") ne sont pas comparables.");
            }
            etiquetteAvertissement.Text = string.Join(Environment.NewLine, messages);
        }

        // ----------------------------------------------------------------- Validation

        private void boutonOk_Click(object sender, EventArgs e)
        {
            TableRequete liee = listeTableLiee.SelectedItem as TableRequete;
            if (liee == null)
            {
                MessageBox.Show(this, "Choisissez la table à laquelle lier " + pNouvelle.Alias + ".", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Jointure jointure = new Jointure(pNouvelle, radioExterne.Checked ? TypeJointure.ExterneGauche : TypeJointure.Interne, pOrigine);
            foreach (DataGridViewRow ligne in grilleConditions.Rows)
            {
                ColonneInfo jointe, liee2;
                if (!LireLigne(ligne, out jointe, out liee2)) continue;
                if (!DetecteurJointures.TypesCompatibles(jointe, liee2))
                {
                    MessageBox.Show(this, jointe.Nom + " et " + liee2.Nom + " ne sont pas de types comparables.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                jointure.Conditions.Add(new ConditionJointure(jointe, liee, liee2));
            }
            if (jointure.Conditions.Count == 0)
            {
                MessageBox.Show(this, "Définissez au moins une condition (colonne de " + pNouvelle.Alias + " = colonne de " + liee.Alias + ").",
                    Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Jointure = jointure;
            DialogResult = DialogResult.OK;
            Close();
        }

        private bool LireLigne(DataGridViewRow ligne, out ColonneInfo jointe, out ColonneInfo liee)
        {
            jointe = null;
            liee = null;
            if (ligne.IsNewRow) return false;
            string a = ligne.Cells[colJointe.Index].Value as string;
            string b = ligne.Cells[colLiee.Index].Value as string;
            return a != null && b != null && pColonnesJointes.TryGetValue(a, out jointe) && pColonnesLiees.TryGetValue(b, out liee);
        }

        private static string Libelle(ColonneInfo colonne)
        {
            return string.IsNullOrEmpty(colonne.Libelle) ? colonne.Nom : colonne.Nom + " — " + colonne.Libelle;
        }

        /// <summary>Proposition affichée : table présente concernée et description.</summary>
        private sealed class ElementProposition
        {
            public ElementProposition(TableRequete presente, JointureProposee proposition)
            {
                Presente = presente;
                Proposition = proposition;
            }

            public TableRequete Presente { get; }
            public JointureProposee Proposition { get; }

            public override string ToString()
            {
                return "avec " + Presente.Alias + " — " + Proposition.LibelleOrigine + " : " + Proposition.Description;
            }
        }
    }
}

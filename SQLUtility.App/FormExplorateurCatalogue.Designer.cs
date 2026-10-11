namespace SQLUtility.App
{
    partial class FormExplorateurCatalogue
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        private void InitializeComponent()
        {
            this.panneauHaut = new System.Windows.Forms.Panel();
            this.champFiltre = new System.Windows.Forms.TextBox();
            this.etiquetteFiltre = new System.Windows.Forms.Label();
            this.boutonRafraichir = new System.Windows.Forms.Button();
            this.listeConnexions = new System.Windows.Forms.ComboBox();
            this.etiquetteConnexion = new System.Windows.Forms.Label();
            this.separateur = new System.Windows.Forms.SplitContainer();
            this.arbreCatalogue = new System.Windows.Forms.TreeView();
            this.grilleColonnes = new System.Windows.Forms.DataGridView();
            this.colColonne = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLibelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLongueur = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEchelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNull = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNomSysteme = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPosition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.etiquetteResume = new System.Windows.Forms.Label();
            this.onglets = new System.Windows.Forms.TabControl();
            this.ongletColonnes = new System.Windows.Forms.TabPage();
            this.ongletCles = new System.Windows.Forms.TabPage();
            this.ongletRelations = new System.Windows.Forms.TabPage();
            this.grilleCles = new System.Windows.Forms.DataGridView();
            this.colCleNom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCleOrigine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCleUnique = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCleIdentifiante = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCleColonnes = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grilleRelations = new System.Windows.Forms.DataGridView();
            this.colRelSens = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelNom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelAutreTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelCorrespondance = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRelSuppression = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.barreStatut = new System.Windows.Forms.StatusStrip();
            this.etiquetteStatut = new System.Windows.Forms.ToolStripStatusLabel();
            this.panneauHaut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).BeginInit();
            this.separateur.Panel1.SuspendLayout();
            this.separateur.Panel2.SuspendLayout();
            this.separateur.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleColonnes)).BeginInit();
            this.onglets.SuspendLayout();
            this.ongletColonnes.SuspendLayout();
            this.ongletCles.SuspendLayout();
            this.ongletRelations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleCles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grilleRelations)).BeginInit();
            this.barreStatut.SuspendLayout();
            this.SuspendLayout();
            //
            // panneauHaut
            //
            this.panneauHaut.Controls.Add(this.champFiltre);
            this.panneauHaut.Controls.Add(this.etiquetteFiltre);
            this.panneauHaut.Controls.Add(this.boutonRafraichir);
            this.panneauHaut.Controls.Add(this.listeConnexions);
            this.panneauHaut.Controls.Add(this.etiquetteConnexion);
            this.panneauHaut.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauHaut.Location = new System.Drawing.Point(0, 0);
            this.panneauHaut.Name = "panneauHaut";
            this.panneauHaut.Size = new System.Drawing.Size(824, 44);
            this.panneauHaut.TabIndex = 0;
            //
            // champFiltre
            //
            this.champFiltre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.champFiltre.Location = new System.Drawing.Point(540, 12);
            this.champFiltre.Name = "champFiltre";
            this.champFiltre.Size = new System.Drawing.Size(272, 20);
            this.champFiltre.TabIndex = 4;
            this.champFiltre.TextChanged += new System.EventHandler(this.champFiltre_TextChanged);
            //
            // etiquetteFiltre
            //
            this.etiquetteFiltre.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteFiltre.AutoSize = true;
            this.etiquetteFiltre.Location = new System.Drawing.Point(450, 15);
            this.etiquetteFiltre.Name = "etiquetteFiltre";
            this.etiquetteFiltre.Size = new System.Drawing.Size(84, 13);
            this.etiquetteFiltre.TabIndex = 3;
            this.etiquetteFiltre.Text = "Filtre schémas :";
            //
            // boutonRafraichir
            //
            this.boutonRafraichir.Location = new System.Drawing.Point(330, 10);
            this.boutonRafraichir.Name = "boutonRafraichir";
            this.boutonRafraichir.Size = new System.Drawing.Size(100, 25);
            this.boutonRafraichir.TabIndex = 2;
            this.boutonRafraichir.Text = "Rafraîchir";
            this.boutonRafraichir.UseVisualStyleBackColor = true;
            this.boutonRafraichir.Click += new System.EventHandler(this.boutonRafraichir_Click);
            //
            // listeConnexions
            //
            this.listeConnexions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeConnexions.FormattingEnabled = true;
            this.listeConnexions.Location = new System.Drawing.Point(90, 11);
            this.listeConnexions.Name = "listeConnexions";
            this.listeConnexions.Size = new System.Drawing.Size(228, 21);
            this.listeConnexions.TabIndex = 1;
            //
            // etiquetteConnexion
            //
            this.etiquetteConnexion.AutoSize = true;
            this.etiquetteConnexion.Location = new System.Drawing.Point(12, 15);
            this.etiquetteConnexion.Name = "etiquetteConnexion";
            this.etiquetteConnexion.Size = new System.Drawing.Size(67, 13);
            this.etiquetteConnexion.TabIndex = 0;
            this.etiquetteConnexion.Text = "Connexion :";
            //
            // separateur
            //
            this.separateur.Dock = System.Windows.Forms.DockStyle.Fill;
            this.separateur.Location = new System.Drawing.Point(0, 44);
            this.separateur.Name = "separateur";
            //
            // separateur.Panel1
            //
            this.separateur.Panel1.Controls.Add(this.arbreCatalogue);
            this.separateur.Panel1MinSize = 180;
            //
            // separateur.Panel2
            //
            this.separateur.Panel2.Controls.Add(this.onglets);
            this.separateur.Panel2.Controls.Add(this.etiquetteResume);
            this.separateur.Panel2MinSize = 200;
            this.separateur.Size = new System.Drawing.Size(824, 495);
            this.separateur.SplitterDistance = 300;
            this.separateur.TabIndex = 1;
            //
            // arbreCatalogue
            //
            this.arbreCatalogue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.arbreCatalogue.HideSelection = false;
            this.arbreCatalogue.Location = new System.Drawing.Point(0, 0);
            this.arbreCatalogue.Name = "arbreCatalogue";
            this.arbreCatalogue.Size = new System.Drawing.Size(300, 495);
            this.arbreCatalogue.TabIndex = 0;
            this.arbreCatalogue.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.arbreCatalogue_BeforeExpand);
            this.arbreCatalogue.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.arbreCatalogue_AfterSelect);
            //
            // grilleColonnes
            //
            this.grilleColonnes.AllowUserToAddRows = false;
            this.grilleColonnes.AllowUserToDeleteRows = false;
            this.grilleColonnes.AutoGenerateColumns = false;
            this.grilleColonnes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleColonnes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleColonnes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colColonne,
            this.colLibelle,
            this.colType,
            this.colLongueur,
            this.colEchelle,
            this.colNull,
            this.colNomSysteme,
            this.colPosition,
            this.colCle,
            this.colReference});
            this.grilleColonnes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleColonnes.Location = new System.Drawing.Point(0, 0);
            this.grilleColonnes.Name = "grilleColonnes";
            this.grilleColonnes.ReadOnly = true;
            this.grilleColonnes.RowHeadersVisible = false;
            this.grilleColonnes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grilleColonnes.Size = new System.Drawing.Size(520, 495);
            this.grilleColonnes.TabIndex = 0;
            //
            // colColonne
            //
            this.colColonne.DataPropertyName = "Nom";
            this.colColonne.FillWeight = 130F;
            this.colColonne.HeaderText = "Colonne";
            this.colColonne.Name = "colColonne";
            this.colColonne.ReadOnly = true;
            //
            // colLibelle
            //
            this.colLibelle.DataPropertyName = "Libelle";
            this.colLibelle.FillWeight = 180F;
            this.colLibelle.HeaderText = "Libellé";
            this.colLibelle.Name = "colLibelle";
            this.colLibelle.ReadOnly = true;
            //
            // colType
            //
            this.colType.DataPropertyName = "TypeDonnee";
            this.colType.FillWeight = 80F;
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            //
            // colLongueur
            //
            this.colLongueur.DataPropertyName = "Longueur";
            this.colLongueur.FillWeight = 60F;
            this.colLongueur.HeaderText = "Long.";
            this.colLongueur.Name = "colLongueur";
            this.colLongueur.ReadOnly = true;
            //
            // colEchelle
            //
            this.colEchelle.DataPropertyName = "Echelle";
            this.colEchelle.FillWeight = 55F;
            this.colEchelle.HeaderText = "Éch.";
            this.colEchelle.Name = "colEchelle";
            this.colEchelle.ReadOnly = true;
            //
            // colNull
            //
            this.colNull.DataPropertyName = "AccepteNull";
            this.colNull.FillWeight = 55F;
            this.colNull.HeaderText = "Null";
            this.colNull.Name = "colNull";
            this.colNull.ReadOnly = true;
            //
            // colNomSysteme
            //
            this.colNomSysteme.DataPropertyName = "NomSysteme";
            this.colNomSysteme.FillWeight = 90F;
            this.colNomSysteme.HeaderText = "Nom système";
            this.colNomSysteme.Name = "colNomSysteme";
            this.colNomSysteme.ReadOnly = true;
            //
            // colPosition
            //
            this.colPosition.DataPropertyName = "Position";
            this.colPosition.FillWeight = 50F;
            this.colPosition.HeaderText = "Pos.";
            this.colPosition.Name = "colPosition";
            this.colPosition.ReadOnly = true;
            //
            // colCle
            //
            this.colCle.DataPropertyName = "RangCleTexte";
            this.colCle.FillWeight = 40F;
            this.colCle.HeaderText = "Clé";
            this.colCle.Name = "colCle";
            this.colCle.ReadOnly = true;
            this.colCle.ToolTipText = "Rang dans la clé d'identification de la table";
            //
            // colReference
            //
            this.colReference.DataPropertyName = "Reference";
            this.colReference.FillWeight = 120F;
            this.colReference.HeaderText = "Référence (FK)";
            this.colReference.Name = "colReference";
            this.colReference.ReadOnly = true;
            //
            // etiquetteResume
            //
            this.etiquetteResume.Dock = System.Windows.Forms.DockStyle.Top;
            this.etiquetteResume.Location = new System.Drawing.Point(0, 0);
            this.etiquetteResume.Name = "etiquetteResume";
            this.etiquetteResume.Padding = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.etiquetteResume.Size = new System.Drawing.Size(520, 40);
            this.etiquetteResume.TabIndex = 0;
            this.etiquetteResume.Text = "Sélectionnez une table.";
            //
            // onglets
            //
            this.onglets.Controls.Add(this.ongletColonnes);
            this.onglets.Controls.Add(this.ongletCles);
            this.onglets.Controls.Add(this.ongletRelations);
            this.onglets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.onglets.Location = new System.Drawing.Point(0, 40);
            this.onglets.Name = "onglets";
            this.onglets.SelectedIndex = 0;
            this.onglets.Size = new System.Drawing.Size(520, 455);
            this.onglets.TabIndex = 1;
            //
            // ongletColonnes
            //
            this.ongletColonnes.Controls.Add(this.grilleColonnes);
            this.ongletColonnes.Location = new System.Drawing.Point(4, 22);
            this.ongletColonnes.Name = "ongletColonnes";
            this.ongletColonnes.Size = new System.Drawing.Size(512, 429);
            this.ongletColonnes.TabIndex = 0;
            this.ongletColonnes.Text = "Colonnes";
            this.ongletColonnes.UseVisualStyleBackColor = true;
            //
            // ongletCles
            //
            this.ongletCles.Controls.Add(this.grilleCles);
            this.ongletCles.Location = new System.Drawing.Point(4, 22);
            this.ongletCles.Name = "ongletCles";
            this.ongletCles.Size = new System.Drawing.Size(512, 429);
            this.ongletCles.TabIndex = 1;
            this.ongletCles.Text = "Clés et index";
            this.ongletCles.UseVisualStyleBackColor = true;
            //
            // ongletRelations
            //
            this.ongletRelations.Controls.Add(this.grilleRelations);
            this.ongletRelations.Location = new System.Drawing.Point(4, 22);
            this.ongletRelations.Name = "ongletRelations";
            this.ongletRelations.Size = new System.Drawing.Size(512, 429);
            this.ongletRelations.TabIndex = 2;
            this.ongletRelations.Text = "Relations";
            this.ongletRelations.UseVisualStyleBackColor = true;
            //
            // grilleCles
            //
            this.grilleCles.AllowUserToAddRows = false;
            this.grilleCles.AllowUserToDeleteRows = false;
            this.grilleCles.AutoGenerateColumns = false;
            this.grilleCles.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleCles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleCles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCleNom,
            this.colCleOrigine,
            this.colCleUnique,
            this.colCleIdentifiante,
            this.colCleColonnes});
            this.grilleCles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleCles.Location = new System.Drawing.Point(0, 0);
            this.grilleCles.Name = "grilleCles";
            this.grilleCles.ReadOnly = true;
            this.grilleCles.RowHeadersVisible = false;
            this.grilleCles.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grilleCles.Size = new System.Drawing.Size(512, 429);
            this.grilleCles.TabIndex = 0;
            //
            // colCleNom
            //
            this.colCleNom.DataPropertyName = "Nom";
            this.colCleNom.FillWeight = 110F;
            this.colCleNom.HeaderText = "Nom";
            this.colCleNom.Name = "colCleNom";
            this.colCleNom.ReadOnly = true;
            //
            // colCleOrigine
            //
            this.colCleOrigine.DataPropertyName = "LibelleOrigine";
            this.colCleOrigine.FillWeight = 90F;
            this.colCleOrigine.HeaderText = "Origine";
            this.colCleOrigine.Name = "colCleOrigine";
            this.colCleOrigine.ReadOnly = true;
            //
            // colCleUnique
            //
            this.colCleUnique.DataPropertyName = "EstUnique";
            this.colCleUnique.FillWeight = 45F;
            this.colCleUnique.HeaderText = "Unique";
            this.colCleUnique.Name = "colCleUnique";
            this.colCleUnique.ReadOnly = true;
            //
            // colCleIdentifiante
            //
            this.colCleIdentifiante.DataPropertyName = "PeutIdentifierLigne";
            this.colCleIdentifiante.FillWeight = 60F;
            this.colCleIdentifiante.HeaderText = "Identifie 1 ligne";
            this.colCleIdentifiante.Name = "colCleIdentifiante";
            this.colCleIdentifiante.ReadOnly = true;
            this.colCleIdentifiante.ToolTipText = "Unique, sans sélection d'enregistrements ni clé dérivée";
            //
            // colCleColonnes
            //
            this.colCleColonnes.DataPropertyName = "ListeColonnes";
            this.colCleColonnes.FillWeight = 200F;
            this.colCleColonnes.HeaderText = "Colonnes de la clé";
            this.colCleColonnes.Name = "colCleColonnes";
            this.colCleColonnes.ReadOnly = true;
            //
            // grilleRelations
            //
            this.grilleRelations.AllowUserToAddRows = false;
            this.grilleRelations.AllowUserToDeleteRows = false;
            this.grilleRelations.AutoGenerateColumns = false;
            this.grilleRelations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleRelations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleRelations.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRelSens,
            this.colRelNom,
            this.colRelAutreTable,
            this.colRelCorrespondance,
            this.colRelSuppression});
            this.grilleRelations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleRelations.Location = new System.Drawing.Point(0, 0);
            this.grilleRelations.Name = "grilleRelations";
            this.grilleRelations.ReadOnly = true;
            this.grilleRelations.RowHeadersVisible = false;
            this.grilleRelations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grilleRelations.Size = new System.Drawing.Size(512, 429);
            this.grilleRelations.TabIndex = 0;
            //
            // colRelSens
            //
            this.colRelSens.DataPropertyName = "Sens";
            this.colRelSens.FillWeight = 60F;
            this.colRelSens.HeaderText = "Sens";
            this.colRelSens.Name = "colRelSens";
            this.colRelSens.ReadOnly = true;
            //
            // colRelNom
            //
            this.colRelNom.DataPropertyName = "Nom";
            this.colRelNom.FillWeight = 100F;
            this.colRelNom.HeaderText = "Contrainte";
            this.colRelNom.Name = "colRelNom";
            this.colRelNom.ReadOnly = true;
            //
            // colRelAutreTable
            //
            this.colRelAutreTable.DataPropertyName = "AutreTable";
            this.colRelAutreTable.FillWeight = 110F;
            this.colRelAutreTable.HeaderText = "Table liée";
            this.colRelAutreTable.Name = "colRelAutreTable";
            this.colRelAutreTable.ReadOnly = true;
            //
            // colRelCorrespondance
            //
            this.colRelCorrespondance.DataPropertyName = "Correspondance";
            this.colRelCorrespondance.FillWeight = 200F;
            this.colRelCorrespondance.HeaderText = "Correspondance (enfant = parent)";
            this.colRelCorrespondance.Name = "colRelCorrespondance";
            this.colRelCorrespondance.ReadOnly = true;
            //
            // colRelSuppression
            //
            this.colRelSuppression.DataPropertyName = "RegleSuppression";
            this.colRelSuppression.FillWeight = 70F;
            this.colRelSuppression.HeaderText = "Suppression";
            this.colRelSuppression.Name = "colRelSuppression";
            this.colRelSuppression.ReadOnly = true;
            //
            // barreStatut
            //
            this.barreStatut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.etiquetteStatut});
            this.barreStatut.Location = new System.Drawing.Point(0, 539);
            this.barreStatut.Name = "barreStatut";
            this.barreStatut.Size = new System.Drawing.Size(824, 22);
            this.barreStatut.TabIndex = 2;
            //
            // etiquetteStatut
            //
            this.etiquetteStatut.Name = "etiquetteStatut";
            this.etiquetteStatut.Size = new System.Drawing.Size(39, 17);
            this.etiquetteStatut.Text = "Prêt.";
            //
            // FormExplorateurCatalogue
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 561);
            this.Controls.Add(this.separateur);
            this.Controls.Add(this.panneauHaut);
            this.Controls.Add(this.barreStatut);
            this.MinimumSize = new System.Drawing.Size(680, 420);
            this.Name = "FormExplorateurCatalogue";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Explorateur de catalogue";
            this.panneauHaut.ResumeLayout(false);
            this.panneauHaut.PerformLayout();
            this.separateur.Panel1.ResumeLayout(false);
            this.separateur.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).EndInit();
            this.separateur.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleColonnes)).EndInit();
            this.onglets.ResumeLayout(false);
            this.ongletColonnes.ResumeLayout(false);
            this.ongletCles.ResumeLayout(false);
            this.ongletRelations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleCles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grilleRelations)).EndInit();
            this.barreStatut.ResumeLayout(false);
            this.barreStatut.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panneauHaut;
        private System.Windows.Forms.TextBox champFiltre;
        private System.Windows.Forms.Label etiquetteFiltre;
        private System.Windows.Forms.Button boutonRafraichir;
        private System.Windows.Forms.ComboBox listeConnexions;
        private System.Windows.Forms.Label etiquetteConnexion;
        private System.Windows.Forms.SplitContainer separateur;
        private System.Windows.Forms.TreeView arbreCatalogue;
        private System.Windows.Forms.DataGridView grilleColonnes;
        private System.Windows.Forms.DataGridViewTextBoxColumn colColonne;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLibelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLongueur;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEchelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNull;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNomSysteme;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPosition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
        private System.Windows.Forms.Label etiquetteResume;
        private System.Windows.Forms.TabControl onglets;
        private System.Windows.Forms.TabPage ongletColonnes;
        private System.Windows.Forms.TabPage ongletCles;
        private System.Windows.Forms.TabPage ongletRelations;
        private System.Windows.Forms.DataGridView grilleCles;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCleNom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCleOrigine;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCleUnique;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colCleIdentifiante;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCleColonnes;
        private System.Windows.Forms.DataGridView grilleRelations;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelSens;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelNom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelAutreTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelCorrespondance;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRelSuppression;
        private System.Windows.Forms.StatusStrip barreStatut;
        private System.Windows.Forms.ToolStripStatusLabel etiquetteStatut;
    }
}

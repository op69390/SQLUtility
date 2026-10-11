namespace SQLUtility.App
{
    partial class FormEditeurSql
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
            this.etiquetteAstuce = new System.Windows.Forms.Label();
            this.boutonAnalyser = new System.Windows.Forms.Button();
            this.boutonInsererColonne = new System.Windows.Forms.Button();
            this.etiquetteCatalogue = new System.Windows.Forms.Label();
            this.listeConnexionsEditeur = new System.Windows.Forms.ComboBox();
            this.listeSchemasEditeur = new System.Windows.Forms.ComboBox();
            this.boutonChargerCatalogue = new System.Windows.Forms.Button();
            this.separateur = new System.Windows.Forms.SplitContainer();
            this.zoneEditeur = new System.Windows.Forms.RichTextBox();
            this.listeAnalyse = new System.Windows.Forms.ListBox();
            this.boutonExecuter = new System.Windows.Forms.Button();
            this.boutonAssistant = new System.Windows.Forms.Button();
            this.etiquetteLimite = new System.Windows.Forms.Label();
            this.champLimite = new System.Windows.Forms.NumericUpDown();
            this.panneauTransaction = new System.Windows.Forms.Panel();
            this.etiquetteTransaction = new System.Windows.Forms.Label();
            this.boutonValider = new System.Windows.Forms.Button();
            this.boutonAnnuler = new System.Windows.Forms.Button();
            this.ongletsBas = new System.Windows.Forms.TabControl();
            this.ongletResultats = new System.Windows.Forms.TabPage();
            this.ongletAnalyse = new System.Windows.Forms.TabPage();
            this.grilleResultats = new System.Windows.Forms.DataGridView();
            this.barreStatut = new System.Windows.Forms.StatusStrip();
            this.etiquetteStatut = new System.Windows.Forms.ToolStripStatusLabel();
            this.panneauHaut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).BeginInit();
            this.separateur.Panel1.SuspendLayout();
            this.separateur.Panel2.SuspendLayout();
            this.separateur.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.champLimite)).BeginInit();
            this.panneauTransaction.SuspendLayout();
            this.ongletsBas.SuspendLayout();
            this.ongletResultats.SuspendLayout();
            this.ongletAnalyse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleResultats)).BeginInit();
            this.barreStatut.SuspendLayout();
            this.SuspendLayout();
            //
            // panneauHaut
            //
            this.panneauHaut.Controls.Add(this.boutonAssistant);
            this.panneauHaut.Controls.Add(this.champLimite);
            this.panneauHaut.Controls.Add(this.etiquetteLimite);
            this.panneauHaut.Controls.Add(this.boutonExecuter);
            this.panneauHaut.Controls.Add(this.boutonChargerCatalogue);
            this.panneauHaut.Controls.Add(this.listeSchemasEditeur);
            this.panneauHaut.Controls.Add(this.listeConnexionsEditeur);
            this.panneauHaut.Controls.Add(this.etiquetteCatalogue);
            this.panneauHaut.Controls.Add(this.etiquetteAstuce);
            this.panneauHaut.Controls.Add(this.boutonInsererColonne);
            this.panneauHaut.Controls.Add(this.boutonAnalyser);
            this.panneauHaut.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauHaut.Location = new System.Drawing.Point(0, 0);
            this.panneauHaut.Name = "panneauHaut";
            this.panneauHaut.Size = new System.Drawing.Size(784, 72);
            this.panneauHaut.TabIndex = 0;
            //
            // etiquetteAstuce
            //
            this.etiquetteAstuce.AutoSize = true;
            this.etiquetteAstuce.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteAstuce.Location = new System.Drawing.Point(530, 13);
            this.etiquetteAstuce.Name = "etiquetteAstuce";
            this.etiquetteAstuce.Size = new System.Drawing.Size(240, 13);
            this.etiquetteAstuce.TabIndex = 5;
            this.etiquetteAstuce.Text = "F5 : exécute la sélection, sinon tout le texte.";
            //
            // boutonExecuter
            //
            this.boutonExecuter.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.boutonExecuter.Location = new System.Drawing.Point(12, 7);
            this.boutonExecuter.Name = "boutonExecuter";
            this.boutonExecuter.Size = new System.Drawing.Size(110, 26);
            this.boutonExecuter.TabIndex = 0;
            this.boutonExecuter.Text = "Exécuter (F5)";
            this.boutonExecuter.UseVisualStyleBackColor = true;
            this.boutonExecuter.Click += new System.EventHandler(this.boutonExecuter_Click);
            //
            // boutonAssistant
            //
            this.boutonAssistant.Location = new System.Drawing.Point(648, 43);
            this.boutonAssistant.Name = "boutonAssistant";
            this.boutonAssistant.Size = new System.Drawing.Size(126, 23);
            this.boutonAssistant.TabIndex = 7;
            this.boutonAssistant.Text = "Assistant…";
            this.boutonAssistant.UseVisualStyleBackColor = true;
            this.boutonAssistant.Click += new System.EventHandler(this.boutonAssistant_Click);
            //
            // boutonAnalyser
            //
            this.boutonAnalyser.Location = new System.Drawing.Point(128, 7);
            this.boutonAnalyser.Name = "boutonAnalyser";
            this.boutonAnalyser.Size = new System.Drawing.Size(110, 26);
            this.boutonAnalyser.TabIndex = 1;
            this.boutonAnalyser.Text = "Analyser (lint)";
            this.boutonAnalyser.UseVisualStyleBackColor = true;
            this.boutonAnalyser.Click += new System.EventHandler(this.boutonAnalyser_Click);
            //
            // etiquetteLimite
            //
            this.etiquetteLimite.AutoSize = true;
            this.etiquetteLimite.Location = new System.Drawing.Point(404, 13);
            this.etiquetteLimite.Name = "etiquetteLimite";
            this.etiquetteLimite.Size = new System.Drawing.Size(42, 13);
            this.etiquetteLimite.TabIndex = 3;
            this.etiquetteLimite.Text = "Limite :";
            //
            // champLimite
            //
            this.champLimite.Increment = new decimal(new int[] { 500, 0, 0, 0 });
            this.champLimite.Location = new System.Drawing.Point(452, 10);
            this.champLimite.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            this.champLimite.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.champLimite.Name = "champLimite";
            this.champLimite.Size = new System.Drawing.Size(70, 20);
            this.champLimite.TabIndex = 4;
            this.champLimite.ThousandsSeparator = true;
            this.champLimite.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            //
            // boutonInsererColonne
            //
            this.boutonInsererColonne.Location = new System.Drawing.Point(244, 7);
            this.boutonInsererColonne.Name = "boutonInsererColonne";
            this.boutonInsererColonne.Size = new System.Drawing.Size(150, 26);
            this.boutonInsererColonne.TabIndex = 2;
            this.boutonInsererColonne.Text = "Insérer une colonne…";
            this.boutonInsererColonne.UseVisualStyleBackColor = true;
            this.boutonInsererColonne.Click += new System.EventHandler(this.boutonInsererColonne_Click);
            //
            // etiquetteCatalogue
            //
            this.etiquetteCatalogue.AutoSize = true;
            this.etiquetteCatalogue.Location = new System.Drawing.Point(12, 47);
            this.etiquetteCatalogue.Name = "etiquetteCatalogue";
            this.etiquetteCatalogue.Size = new System.Drawing.Size(118, 13);
            this.etiquetteCatalogue.TabIndex = 3;
            this.etiquetteCatalogue.Text = "Connexion / schéma :";
            //
            // listeConnexionsEditeur
            //
            this.listeConnexionsEditeur.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeConnexionsEditeur.FormattingEnabled = true;
            this.listeConnexionsEditeur.Location = new System.Drawing.Point(140, 44);
            this.listeConnexionsEditeur.Name = "listeConnexionsEditeur";
            this.listeConnexionsEditeur.Size = new System.Drawing.Size(160, 21);
            this.listeConnexionsEditeur.TabIndex = 4;
            this.listeConnexionsEditeur.SelectedIndexChanged += new System.EventHandler(this.listeConnexionsEditeur_SelectedIndexChanged);
            //
            // listeSchemasEditeur
            //
            this.listeSchemasEditeur.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeSchemasEditeur.FormattingEnabled = true;
            this.listeSchemasEditeur.Location = new System.Drawing.Point(306, 44);
            this.listeSchemasEditeur.Name = "listeSchemasEditeur";
            this.listeSchemasEditeur.Size = new System.Drawing.Size(150, 21);
            this.listeSchemasEditeur.TabIndex = 5;
            //
            // boutonChargerCatalogue
            //
            this.boutonChargerCatalogue.Location = new System.Drawing.Point(462, 43);
            this.boutonChargerCatalogue.Name = "boutonChargerCatalogue";
            this.boutonChargerCatalogue.Size = new System.Drawing.Size(180, 23);
            this.boutonChargerCatalogue.TabIndex = 6;
            this.boutonChargerCatalogue.Text = "Charger ce schéma";
            this.boutonChargerCatalogue.UseVisualStyleBackColor = true;
            this.boutonChargerCatalogue.Click += new System.EventHandler(this.boutonChargerCatalogue_Click);
            //
            // separateur
            //
            this.separateur.Dock = System.Windows.Forms.DockStyle.Fill;
            this.separateur.Location = new System.Drawing.Point(0, 40);
            this.separateur.Name = "separateur";
            this.separateur.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // separateur.Panel1
            //
            this.separateur.Panel1.Controls.Add(this.zoneEditeur);
            this.separateur.Panel1MinSize = 120;
            //
            // separateur.Panel2
            //
            this.separateur.Panel2.Controls.Add(this.ongletsBas);
            this.separateur.Panel2.Controls.Add(this.panneauTransaction);
            this.separateur.Panel2MinSize = 120;
            this.separateur.Size = new System.Drawing.Size(784, 467);
            this.separateur.SplitterDistance = 220;
            this.separateur.TabIndex = 1;
            //
            // zoneEditeur
            //
            this.zoneEditeur.AcceptsTab = true;
            this.zoneEditeur.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoneEditeur.Font = new System.Drawing.Font("Consolas", 10F);
            this.zoneEditeur.HideSelection = false;
            this.zoneEditeur.Location = new System.Drawing.Point(0, 0);
            this.zoneEditeur.Name = "zoneEditeur";
            this.zoneEditeur.Size = new System.Drawing.Size(784, 360);
            this.zoneEditeur.TabIndex = 0;
            this.zoneEditeur.Text = "";
            this.zoneEditeur.WordWrap = false;
            this.zoneEditeur.TextChanged += new System.EventHandler(this.zoneEditeur_TextChanged);
            //
            // listeAnalyse
            //
            this.listeAnalyse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listeAnalyse.FormattingEnabled = true;
            this.listeAnalyse.HorizontalScrollbar = true;
            this.listeAnalyse.IntegralHeight = false;
            this.listeAnalyse.Location = new System.Drawing.Point(0, 0);
            this.listeAnalyse.Name = "listeAnalyse";
            this.listeAnalyse.Size = new System.Drawing.Size(784, 135);
            this.listeAnalyse.TabIndex = 0;
            this.listeAnalyse.DoubleClick += new System.EventHandler(this.listeAnalyse_DoubleClick);
            //
            // panneauTransaction
            //
            this.panneauTransaction.BackColor = System.Drawing.Color.FromArgb(255, 243, 205);
            this.panneauTransaction.Controls.Add(this.etiquetteTransaction);
            this.panneauTransaction.Controls.Add(this.boutonValider);
            this.panneauTransaction.Controls.Add(this.boutonAnnuler);
            this.panneauTransaction.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauTransaction.Location = new System.Drawing.Point(0, 0);
            this.panneauTransaction.Name = "panneauTransaction";
            this.panneauTransaction.Size = new System.Drawing.Size(784, 40);
            this.panneauTransaction.TabIndex = 0;
            this.panneauTransaction.Visible = false;
            //
            // etiquetteTransaction
            //
            this.etiquetteTransaction.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteTransaction.Location = new System.Drawing.Point(8, 2);
            this.etiquetteTransaction.Name = "etiquetteTransaction";
            this.etiquetteTransaction.Size = new System.Drawing.Size(500, 36);
            this.etiquetteTransaction.TabIndex = 0;
            this.etiquetteTransaction.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // boutonValider
            //
            this.boutonValider.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonValider.Location = new System.Drawing.Point(516, 7);
            this.boutonValider.Name = "boutonValider";
            this.boutonValider.Size = new System.Drawing.Size(126, 26);
            this.boutonValider.TabIndex = 1;
            this.boutonValider.Text = "Valider (COMMIT)";
            this.boutonValider.UseVisualStyleBackColor = true;
            this.boutonValider.Click += new System.EventHandler(this.boutonValider_Click);
            //
            // boutonAnnuler
            //
            this.boutonAnnuler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonAnnuler.Location = new System.Drawing.Point(648, 7);
            this.boutonAnnuler.Name = "boutonAnnuler";
            this.boutonAnnuler.Size = new System.Drawing.Size(126, 26);
            this.boutonAnnuler.TabIndex = 2;
            this.boutonAnnuler.Text = "Annuler (ROLLBACK)";
            this.boutonAnnuler.UseVisualStyleBackColor = true;
            this.boutonAnnuler.Click += new System.EventHandler(this.boutonAnnuler_Click);
            //
            // ongletsBas
            //
            this.ongletsBas.Controls.Add(this.ongletResultats);
            this.ongletsBas.Controls.Add(this.ongletAnalyse);
            this.ongletsBas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ongletsBas.Location = new System.Drawing.Point(0, 40);
            this.ongletsBas.Name = "ongletsBas";
            this.ongletsBas.SelectedIndex = 0;
            this.ongletsBas.Size = new System.Drawing.Size(784, 203);
            this.ongletsBas.TabIndex = 1;
            //
            // ongletResultats
            //
            this.ongletResultats.Controls.Add(this.grilleResultats);
            this.ongletResultats.Location = new System.Drawing.Point(4, 22);
            this.ongletResultats.Name = "ongletResultats";
            this.ongletResultats.Size = new System.Drawing.Size(776, 177);
            this.ongletResultats.TabIndex = 0;
            this.ongletResultats.Text = "Résultats";
            this.ongletResultats.UseVisualStyleBackColor = true;
            //
            // ongletAnalyse
            //
            this.ongletAnalyse.Controls.Add(this.listeAnalyse);
            this.ongletAnalyse.Location = new System.Drawing.Point(4, 22);
            this.ongletAnalyse.Name = "ongletAnalyse";
            this.ongletAnalyse.Size = new System.Drawing.Size(776, 177);
            this.ongletAnalyse.TabIndex = 1;
            this.ongletAnalyse.Text = "Analyse (double-clic : aller à la position)";
            this.ongletAnalyse.UseVisualStyleBackColor = true;
            //
            // grilleResultats
            //
            this.grilleResultats.AllowUserToAddRows = false;
            this.grilleResultats.AllowUserToDeleteRows = false;
            this.grilleResultats.AllowUserToOrderColumns = true;
            this.grilleResultats.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            this.grilleResultats.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleResultats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleResultats.Location = new System.Drawing.Point(0, 0);
            this.grilleResultats.Name = "grilleResultats";
            this.grilleResultats.ReadOnly = true;
            this.grilleResultats.RowHeadersWidth = 24;
            this.grilleResultats.Size = new System.Drawing.Size(776, 177);
            this.grilleResultats.TabIndex = 0;
            //
            // barreStatut
            //
            this.barreStatut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.etiquetteStatut});
            this.barreStatut.Location = new System.Drawing.Point(0, 539);
            this.barreStatut.Name = "barreStatut";
            this.barreStatut.Size = new System.Drawing.Size(784, 22);
            this.barreStatut.TabIndex = 2;
            //
            // etiquetteStatut
            //
            this.etiquetteStatut.Name = "etiquetteStatut";
            this.etiquetteStatut.Size = new System.Drawing.Size(39, 17);
            this.etiquetteStatut.Text = "Prêt.";
            //
            // FormEditeurSql
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.separateur);
            this.Controls.Add(this.panneauHaut);
            this.Controls.Add(this.barreStatut);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "FormEditeurSql";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Éditeur SQL";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormEditeurSql_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FormEditeurSql_KeyDown);
            this.panneauHaut.ResumeLayout(false);
            this.panneauHaut.PerformLayout();
            this.separateur.Panel1.ResumeLayout(false);
            this.separateur.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).EndInit();
            this.separateur.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.champLimite)).EndInit();
            this.panneauTransaction.ResumeLayout(false);
            this.ongletsBas.ResumeLayout(false);
            this.ongletResultats.ResumeLayout(false);
            this.ongletAnalyse.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleResultats)).EndInit();
            this.barreStatut.ResumeLayout(false);
            this.barreStatut.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panneauHaut;
        private System.Windows.Forms.Label etiquetteAstuce;
        private System.Windows.Forms.Button boutonAnalyser;
        private System.Windows.Forms.Button boutonInsererColonne;
        private System.Windows.Forms.Label etiquetteCatalogue;
        private System.Windows.Forms.ComboBox listeConnexionsEditeur;
        private System.Windows.Forms.ComboBox listeSchemasEditeur;
        private System.Windows.Forms.Button boutonChargerCatalogue;
        private System.Windows.Forms.SplitContainer separateur;
        private System.Windows.Forms.RichTextBox zoneEditeur;
        private System.Windows.Forms.ListBox listeAnalyse;
        private System.Windows.Forms.Button boutonExecuter;
        private System.Windows.Forms.Button boutonAssistant;
        private System.Windows.Forms.Label etiquetteLimite;
        private System.Windows.Forms.NumericUpDown champLimite;
        private System.Windows.Forms.Panel panneauTransaction;
        private System.Windows.Forms.Label etiquetteTransaction;
        private System.Windows.Forms.Button boutonValider;
        private System.Windows.Forms.Button boutonAnnuler;
        private System.Windows.Forms.TabControl ongletsBas;
        private System.Windows.Forms.TabPage ongletResultats;
        private System.Windows.Forms.TabPage ongletAnalyse;
        private System.Windows.Forms.DataGridView grilleResultats;
        private System.Windows.Forms.StatusStrip barreStatut;
        private System.Windows.Forms.ToolStripStatusLabel etiquetteStatut;
    }
}

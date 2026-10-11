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
            this.barreStatut = new System.Windows.Forms.StatusStrip();
            this.etiquetteStatut = new System.Windows.Forms.ToolStripStatusLabel();
            this.panneauHaut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).BeginInit();
            this.separateur.Panel1.SuspendLayout();
            this.separateur.Panel2.SuspendLayout();
            this.separateur.SuspendLayout();
            this.barreStatut.SuspendLayout();
            this.SuspendLayout();
            //
            // panneauHaut
            //
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
            this.etiquetteAstuce.Location = new System.Drawing.Point(320, 13);
            this.etiquetteAstuce.Name = "etiquetteAstuce";
            this.etiquetteAstuce.Size = new System.Drawing.Size(372, 13);
            this.etiquetteAstuce.TabIndex = 1;
            this.etiquetteAstuce.Text = "Astuce : double-cliquez un résultat d'analyse pour aller à la position concernée.";
            //
            // boutonAnalyser
            //
            this.boutonAnalyser.Location = new System.Drawing.Point(12, 7);
            this.boutonAnalyser.Name = "boutonAnalyser";
            this.boutonAnalyser.Size = new System.Drawing.Size(130, 26);
            this.boutonAnalyser.TabIndex = 0;
            this.boutonAnalyser.Text = "Analyser (lint)";
            this.boutonAnalyser.UseVisualStyleBackColor = true;
            this.boutonAnalyser.Click += new System.EventHandler(this.boutonAnalyser_Click);
            //
            // boutonInsererColonne
            //
            this.boutonInsererColonne.Location = new System.Drawing.Point(148, 7);
            this.boutonInsererColonne.Name = "boutonInsererColonne";
            this.boutonInsererColonne.Size = new System.Drawing.Size(160, 26);
            this.boutonInsererColonne.TabIndex = 1;
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
            this.etiquetteCatalogue.Text = "Complétion catalogue :";
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
            this.separateur.Panel2.Controls.Add(this.listeAnalyse);
            this.separateur.Panel2MinSize = 80;
            this.separateur.Size = new System.Drawing.Size(784, 499);
            this.separateur.SplitterDistance = 360;
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
            this.MinimumSize = new System.Drawing.Size(560, 360);
            this.Name = "FormEditeurSql";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Éditeur SQL";
            this.panneauHaut.ResumeLayout(false);
            this.panneauHaut.PerformLayout();
            this.separateur.Panel1.ResumeLayout(false);
            this.separateur.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.separateur)).EndInit();
            this.separateur.ResumeLayout(false);
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
        private System.Windows.Forms.StatusStrip barreStatut;
        private System.Windows.Forms.ToolStripStatusLabel etiquetteStatut;
    }
}

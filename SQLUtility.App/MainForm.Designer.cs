namespace SQLUtility.App
{
    partial class MainForm
    {
        /// <summary>Conteneur de composants requis.</summary>
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
            this.menuPrincipal = new System.Windows.Forms.MenuStrip();
            this.menuFichier = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFichierQuitter = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConnexions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConnexionsGerer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuConnexionsExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditeur = new System.Windows.Forms.ToolStripMenuItem();
            this.menuEditeurNouveau = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOutils = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAide = new System.Windows.Forms.ToolStripMenuItem();
            this.barreEtat = new System.Windows.Forms.StatusStrip();
            this.etiquetteFichier = new System.Windows.Forms.ToolStripStatusLabel();
            this.etatProfils = new System.Windows.Forms.ToolStripStatusLabel();
            this.zoneAccueil = new System.Windows.Forms.TextBox();
            this.menuPrincipal.SuspendLayout();
            this.barreEtat.SuspendLayout();
            this.SuspendLayout();
            //
            // menuPrincipal
            //
            this.menuPrincipal.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFichier,
            this.menuConnexions,
            this.menuEditeur,
            this.menuOutils,
            this.menuAide});
            this.menuPrincipal.Location = new System.Drawing.Point(0, 0);
            this.menuPrincipal.Name = "menuPrincipal";
            this.menuPrincipal.Size = new System.Drawing.Size(884, 24);
            this.menuPrincipal.TabIndex = 0;
            this.menuPrincipal.Text = "menuPrincipal";
            //
            // menuFichier
            //
            this.menuFichier.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFichierQuitter});
            this.menuFichier.Name = "menuFichier";
            this.menuFichier.Size = new System.Drawing.Size(54, 20);
            this.menuFichier.Text = "&Fichier";
            //
            // menuFichierQuitter
            //
            this.menuFichierQuitter.Name = "menuFichierQuitter";
            this.menuFichierQuitter.Size = new System.Drawing.Size(118, 22);
            this.menuFichierQuitter.Text = "&Quitter";
            this.menuFichierQuitter.Click += new System.EventHandler(this.menuFichierQuitter_Click);
            //
            // menuConnexions
            //
            this.menuConnexions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuConnexionsGerer,
            this.menuConnexionsExplorer});
            this.menuConnexions.Name = "menuConnexions";
            this.menuConnexions.Size = new System.Drawing.Size(89, 20);
            this.menuConnexions.Text = "&Connexions";
            //
            // menuConnexionsGerer
            //
            this.menuConnexionsGerer.Name = "menuConnexionsGerer";
            this.menuConnexionsGerer.Size = new System.Drawing.Size(204, 22);
            this.menuConnexionsGerer.Text = "&Gérer les connexions…";
            this.menuConnexionsGerer.Click += new System.EventHandler(this.menuConnexionsGerer_Click);
            //
            // menuConnexionsExplorer
            //
            this.menuConnexionsExplorer.Name = "menuConnexionsExplorer";
            this.menuConnexionsExplorer.Size = new System.Drawing.Size(204, 22);
            this.menuConnexionsExplorer.Text = "&Explorateur de catalogue…";
            this.menuConnexionsExplorer.Click += new System.EventHandler(this.menuConnexionsExplorer_Click);
            //
            // menuEditeur
            //
            this.menuEditeur.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuEditeurNouveau});
            this.menuEditeur.Name = "menuEditeur";
            this.menuEditeur.Size = new System.Drawing.Size(58, 20);
            this.menuEditeur.Text = "&Éditeur";
            //
            // menuEditeurNouveau
            //
            this.menuEditeurNouveau.Name = "menuEditeurNouveau";
            this.menuEditeurNouveau.Size = new System.Drawing.Size(200, 22);
            this.menuEditeurNouveau.Text = "&Nouvel éditeur SQL…";
            this.menuEditeurNouveau.Click += new System.EventHandler(this.menuEditeurNouveau_Click);
            //
            // menuOutils
            //
            this.menuOutils.Name = "menuOutils";
            this.menuOutils.Size = new System.Drawing.Size(55, 20);
            this.menuOutils.Text = "&Outils";
            //
            // menuAide
            //
            this.menuAide.Name = "menuAide";
            this.menuAide.Size = new System.Drawing.Size(46, 20);
            this.menuAide.Text = "&Aide";
            //
            // barreEtat
            //
            this.barreEtat.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.etiquetteFichier,
            this.etatProfils});
            this.barreEtat.Location = new System.Drawing.Point(0, 539);
            this.barreEtat.Name = "barreEtat";
            this.barreEtat.Size = new System.Drawing.Size(884, 22);
            this.barreEtat.TabIndex = 1;
            this.barreEtat.Text = "barreEtat";
            //
            // etiquetteFichier
            //
            this.etiquetteFichier.Name = "etiquetteFichier";
            this.etiquetteFichier.Size = new System.Drawing.Size(131, 17);
            this.etiquetteFichier.Spring = true;
            this.etiquetteFichier.Text = "Fichier de paramètres :";
            this.etiquetteFichier.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // etatProfils
            //
            this.etatProfils.Name = "etatProfils";
            this.etatProfils.Size = new System.Drawing.Size(56, 17);
            this.etatProfils.Text = "Profils : 0";
            //
            // zoneAccueil
            //
            this.zoneAccueil.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.zoneAccueil.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zoneAccueil.Location = new System.Drawing.Point(0, 24);
            this.zoneAccueil.Multiline = true;
            this.zoneAccueil.Name = "zoneAccueil";
            this.zoneAccueil.ReadOnly = true;
            this.zoneAccueil.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.zoneAccueil.Size = new System.Drawing.Size(884, 515);
            this.zoneAccueil.TabIndex = 2;
            this.zoneAccueil.TabStop = false;
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(884, 561);
            this.Controls.Add(this.zoneAccueil);
            this.Controls.Add(this.barreEtat);
            this.Controls.Add(this.menuPrincipal);
            this.MainMenuStrip = this.menuPrincipal;
            this.MinimumSize = new System.Drawing.Size(640, 400);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQLUtility";
            this.menuPrincipal.ResumeLayout(false);
            this.menuPrincipal.PerformLayout();
            this.barreEtat.ResumeLayout(false);
            this.barreEtat.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuPrincipal;
        private System.Windows.Forms.ToolStripMenuItem menuFichier;
        private System.Windows.Forms.ToolStripMenuItem menuFichierQuitter;
        private System.Windows.Forms.ToolStripMenuItem menuConnexions;
        private System.Windows.Forms.ToolStripMenuItem menuConnexionsGerer;
        private System.Windows.Forms.ToolStripMenuItem menuConnexionsExplorer;
        private System.Windows.Forms.ToolStripMenuItem menuEditeur;
        private System.Windows.Forms.ToolStripMenuItem menuEditeurNouveau;
        private System.Windows.Forms.ToolStripMenuItem menuOutils;
        private System.Windows.Forms.ToolStripMenuItem menuAide;
        private System.Windows.Forms.StatusStrip barreEtat;
        private System.Windows.Forms.ToolStripStatusLabel etiquetteFichier;
        private System.Windows.Forms.ToolStripStatusLabel etatProfils;
        private System.Windows.Forms.TextBox zoneAccueil;
    }
}

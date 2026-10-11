namespace SQLUtility.App
{
    partial class FormGestionConnexions
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
            this.etiquetteListe = new System.Windows.Forms.Label();
            this.listeProfils = new System.Windows.Forms.ListBox();
            this.groupeProfil = new System.Windows.Forms.GroupBox();
            this.etiquetteNom = new System.Windows.Forms.Label();
            this.champNom = new System.Windows.Forms.TextBox();
            this.etiquetteServeur = new System.Windows.Forms.Label();
            this.champServeur = new System.Windows.Forms.TextBox();
            this.etiquetteUtilisateur = new System.Windows.Forms.Label();
            this.champUtilisateur = new System.Windows.Forms.TextBox();
            this.etiquetteMotDePasse = new System.Windows.Forms.Label();
            this.champMotDePasse = new System.Windows.Forms.TextBox();
            this.etiquetteIndiceMdp = new System.Windows.Forms.Label();
            this.etiquetteBibliotheque = new System.Windows.Forms.Label();
            this.champBibliotheque = new System.Windows.Forms.TextBox();
            this.caseLectureSeule = new System.Windows.Forms.CheckBox();
            this.etiquetteDescription = new System.Windows.Forms.Label();
            this.champDescription = new System.Windows.Forms.TextBox();
            this.boutonTester = new System.Windows.Forms.Button();
            this.boutonNouveau = new System.Windows.Forms.Button();
            this.boutonEnregistrer = new System.Windows.Forms.Button();
            this.boutonSupprimer = new System.Windows.Forms.Button();
            this.boutonDefaut = new System.Windows.Forms.Button();
            this.etiquetteStatut = new System.Windows.Forms.Label();
            this.boutonFermer = new System.Windows.Forms.Button();
            this.groupeProfil.SuspendLayout();
            this.SuspendLayout();
            //
            // etiquetteListe
            //
            this.etiquetteListe.AutoSize = true;
            this.etiquetteListe.Location = new System.Drawing.Point(12, 9);
            this.etiquetteListe.Name = "etiquetteListe";
            this.etiquetteListe.Size = new System.Drawing.Size(110, 13);
            this.etiquetteListe.TabIndex = 0;
            this.etiquetteListe.Text = "Profils enregistrés :";
            //
            // listeProfils
            //
            this.listeProfils.FormattingEnabled = true;
            this.listeProfils.IntegralHeight = false;
            this.listeProfils.Location = new System.Drawing.Point(12, 28);
            this.listeProfils.Name = "listeProfils";
            this.listeProfils.Size = new System.Drawing.Size(210, 344);
            this.listeProfils.TabIndex = 1;
            this.listeProfils.SelectedIndexChanged += new System.EventHandler(this.listeProfils_SelectedIndexChanged);
            //
            // groupeProfil
            //
            this.groupeProfil.Controls.Add(this.etiquetteNom);
            this.groupeProfil.Controls.Add(this.champNom);
            this.groupeProfil.Controls.Add(this.etiquetteServeur);
            this.groupeProfil.Controls.Add(this.champServeur);
            this.groupeProfil.Controls.Add(this.etiquetteUtilisateur);
            this.groupeProfil.Controls.Add(this.champUtilisateur);
            this.groupeProfil.Controls.Add(this.etiquetteMotDePasse);
            this.groupeProfil.Controls.Add(this.champMotDePasse);
            this.groupeProfil.Controls.Add(this.etiquetteIndiceMdp);
            this.groupeProfil.Controls.Add(this.etiquetteBibliotheque);
            this.groupeProfil.Controls.Add(this.champBibliotheque);
            this.groupeProfil.Controls.Add(this.caseLectureSeule);
            this.groupeProfil.Controls.Add(this.etiquetteDescription);
            this.groupeProfil.Controls.Add(this.champDescription);
            this.groupeProfil.Location = new System.Drawing.Point(234, 12);
            this.groupeProfil.Name = "groupeProfil";
            this.groupeProfil.Size = new System.Drawing.Size(458, 360);
            this.groupeProfil.TabIndex = 2;
            this.groupeProfil.TabStop = false;
            this.groupeProfil.Text = "Profil";
            //
            // etiquetteNom
            //
            this.etiquetteNom.AutoSize = true;
            this.etiquetteNom.Location = new System.Drawing.Point(18, 33);
            this.etiquetteNom.Name = "etiquetteNom";
            this.etiquetteNom.Size = new System.Drawing.Size(35, 13);
            this.etiquetteNom.TabIndex = 0;
            this.etiquetteNom.Text = "Nom :";
            //
            // champNom
            //
            this.champNom.Location = new System.Drawing.Point(150, 30);
            this.champNom.MaxLength = 64;
            this.champNom.Name = "champNom";
            this.champNom.Size = new System.Drawing.Size(290, 20);
            this.champNom.TabIndex = 1;
            //
            // etiquetteServeur
            //
            this.etiquetteServeur.AutoSize = true;
            this.etiquetteServeur.Location = new System.Drawing.Point(18, 65);
            this.etiquetteServeur.Name = "etiquetteServeur";
            this.etiquetteServeur.Size = new System.Drawing.Size(106, 13);
            this.etiquetteServeur.TabIndex = 2;
            this.etiquetteServeur.Text = "Serveur / Système :";
            //
            // champServeur
            //
            this.champServeur.Location = new System.Drawing.Point(150, 62);
            this.champServeur.MaxLength = 128;
            this.champServeur.Name = "champServeur";
            this.champServeur.Size = new System.Drawing.Size(290, 20);
            this.champServeur.TabIndex = 3;
            //
            // etiquetteUtilisateur
            //
            this.etiquetteUtilisateur.AutoSize = true;
            this.etiquetteUtilisateur.Location = new System.Drawing.Point(18, 97);
            this.etiquetteUtilisateur.Name = "etiquetteUtilisateur";
            this.etiquetteUtilisateur.Size = new System.Drawing.Size(63, 13);
            this.etiquetteUtilisateur.TabIndex = 4;
            this.etiquetteUtilisateur.Text = "Utilisateur :";
            //
            // champUtilisateur
            //
            this.champUtilisateur.Location = new System.Drawing.Point(150, 94);
            this.champUtilisateur.MaxLength = 128;
            this.champUtilisateur.Name = "champUtilisateur";
            this.champUtilisateur.Size = new System.Drawing.Size(290, 20);
            this.champUtilisateur.TabIndex = 5;
            //
            // etiquetteMotDePasse
            //
            this.etiquetteMotDePasse.AutoSize = true;
            this.etiquetteMotDePasse.Location = new System.Drawing.Point(18, 129);
            this.etiquetteMotDePasse.Name = "etiquetteMotDePasse";
            this.etiquetteMotDePasse.Size = new System.Drawing.Size(79, 13);
            this.etiquetteMotDePasse.TabIndex = 6;
            this.etiquetteMotDePasse.Text = "Mot de passe :";
            //
            // champMotDePasse
            //
            this.champMotDePasse.Location = new System.Drawing.Point(150, 126);
            this.champMotDePasse.MaxLength = 128;
            this.champMotDePasse.Name = "champMotDePasse";
            this.champMotDePasse.Size = new System.Drawing.Size(290, 20);
            this.champMotDePasse.TabIndex = 7;
            this.champMotDePasse.UseSystemPasswordChar = true;
            //
            // etiquetteIndiceMdp
            //
            this.etiquetteIndiceMdp.AutoSize = true;
            this.etiquetteIndiceMdp.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteIndiceMdp.Location = new System.Drawing.Point(147, 149);
            this.etiquetteIndiceMdp.Name = "etiquetteIndiceMdp";
            this.etiquetteIndiceMdp.Size = new System.Drawing.Size(193, 13);
            this.etiquetteIndiceMdp.TabIndex = 8;
            this.etiquetteIndiceMdp.Text = "Laisser vide pour ne pas modifier le secret.";
            //
            // etiquetteBibliotheque
            //
            this.etiquetteBibliotheque.AutoSize = true;
            this.etiquetteBibliotheque.Location = new System.Drawing.Point(18, 181);
            this.etiquetteBibliotheque.Name = "etiquetteBibliotheque";
            this.etiquetteBibliotheque.Size = new System.Drawing.Size(89, 13);
            this.etiquetteBibliotheque.TabIndex = 9;
            this.etiquetteBibliotheque.Text = "Bibliothèque(s) :";
            //
            // champBibliotheque
            //
            this.champBibliotheque.Location = new System.Drawing.Point(150, 178);
            this.champBibliotheque.MaxLength = 256;
            this.champBibliotheque.Name = "champBibliotheque";
            this.champBibliotheque.Size = new System.Drawing.Size(290, 20);
            this.champBibliotheque.TabIndex = 10;
            //
            // caseLectureSeule
            //
            this.caseLectureSeule.AutoSize = true;
            this.caseLectureSeule.Checked = true;
            this.caseLectureSeule.CheckState = System.Windows.Forms.CheckState.Checked;
            this.caseLectureSeule.Location = new System.Drawing.Point(150, 209);
            this.caseLectureSeule.Name = "caseLectureSeule";
            this.caseLectureSeule.Size = new System.Drawing.Size(238, 17);
            this.caseLectureSeule.TabIndex = 11;
            this.caseLectureSeule.Text = "Lecture seule (interdit tout DML / DDL)";
            this.caseLectureSeule.UseVisualStyleBackColor = true;
            //
            // etiquetteDescription
            //
            this.etiquetteDescription.AutoSize = true;
            this.etiquetteDescription.Location = new System.Drawing.Point(18, 241);
            this.etiquetteDescription.Name = "etiquetteDescription";
            this.etiquetteDescription.Size = new System.Drawing.Size(69, 13);
            this.etiquetteDescription.TabIndex = 12;
            this.etiquetteDescription.Text = "Description :";
            //
            // champDescription
            //
            this.champDescription.Location = new System.Drawing.Point(150, 238);
            this.champDescription.MaxLength = 512;
            this.champDescription.Multiline = true;
            this.champDescription.Name = "champDescription";
            this.champDescription.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.champDescription.Size = new System.Drawing.Size(290, 108);
            this.champDescription.TabIndex = 13;
            //
            // boutonTester
            //
            this.boutonTester.Location = new System.Drawing.Point(12, 384);
            this.boutonTester.Name = "boutonTester";
            this.boutonTester.Size = new System.Drawing.Size(210, 30);
            this.boutonTester.TabIndex = 3;
            this.boutonTester.Text = "Tester la connexion";
            this.boutonTester.UseVisualStyleBackColor = true;
            this.boutonTester.Click += new System.EventHandler(this.boutonTester_Click);
            //
            // boutonNouveau
            //
            this.boutonNouveau.Location = new System.Drawing.Point(234, 384);
            this.boutonNouveau.Name = "boutonNouveau";
            this.boutonNouveau.Size = new System.Drawing.Size(104, 30);
            this.boutonNouveau.TabIndex = 4;
            this.boutonNouveau.Text = "Nouveau";
            this.boutonNouveau.UseVisualStyleBackColor = true;
            this.boutonNouveau.Click += new System.EventHandler(this.boutonNouveau_Click);
            //
            // boutonEnregistrer
            //
            this.boutonEnregistrer.Location = new System.Drawing.Point(344, 384);
            this.boutonEnregistrer.Name = "boutonEnregistrer";
            this.boutonEnregistrer.Size = new System.Drawing.Size(104, 30);
            this.boutonEnregistrer.TabIndex = 5;
            this.boutonEnregistrer.Text = "Enregistrer";
            this.boutonEnregistrer.UseVisualStyleBackColor = true;
            this.boutonEnregistrer.Click += new System.EventHandler(this.boutonEnregistrer_Click);
            //
            // boutonSupprimer
            //
            this.boutonSupprimer.Location = new System.Drawing.Point(454, 384);
            this.boutonSupprimer.Name = "boutonSupprimer";
            this.boutonSupprimer.Size = new System.Drawing.Size(104, 30);
            this.boutonSupprimer.TabIndex = 6;
            this.boutonSupprimer.Text = "Supprimer";
            this.boutonSupprimer.UseVisualStyleBackColor = true;
            this.boutonSupprimer.Click += new System.EventHandler(this.boutonSupprimer_Click);
            //
            // boutonDefaut
            //
            this.boutonDefaut.Location = new System.Drawing.Point(588, 384);
            this.boutonDefaut.Name = "boutonDefaut";
            this.boutonDefaut.Size = new System.Drawing.Size(104, 30);
            this.boutonDefaut.TabIndex = 7;
            this.boutonDefaut.Text = "Par défaut";
            this.boutonDefaut.UseVisualStyleBackColor = true;
            this.boutonDefaut.Click += new System.EventHandler(this.boutonDefaut_Click);
            //
            // etiquetteStatut
            //
            this.etiquetteStatut.AutoEllipsis = true;
            this.etiquetteStatut.Location = new System.Drawing.Point(12, 425);
            this.etiquetteStatut.Name = "etiquetteStatut";
            this.etiquetteStatut.Size = new System.Drawing.Size(580, 50);
            this.etiquetteStatut.TabIndex = 8;
            //
            // boutonFermer
            //
            this.boutonFermer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.boutonFermer.Location = new System.Drawing.Point(602, 480);
            this.boutonFermer.Name = "boutonFermer";
            this.boutonFermer.Size = new System.Drawing.Size(90, 30);
            this.boutonFermer.TabIndex = 9;
            this.boutonFermer.Text = "Fermer";
            this.boutonFermer.UseVisualStyleBackColor = true;
            this.boutonFermer.Click += new System.EventHandler(this.boutonFermer_Click);
            //
            // FormGestionConnexions
            //
            this.AcceptButton = this.boutonEnregistrer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.boutonFermer;
            this.ClientSize = new System.Drawing.Size(704, 521);
            this.Controls.Add(this.boutonFermer);
            this.Controls.Add(this.etiquetteStatut);
            this.Controls.Add(this.boutonDefaut);
            this.Controls.Add(this.boutonSupprimer);
            this.Controls.Add(this.boutonEnregistrer);
            this.Controls.Add(this.boutonNouveau);
            this.Controls.Add(this.boutonTester);
            this.Controls.Add(this.groupeProfil);
            this.Controls.Add(this.listeProfils);
            this.Controls.Add(this.etiquetteListe);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormGestionConnexions";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Gestion des connexions";
            this.groupeProfil.ResumeLayout(false);
            this.groupeProfil.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label etiquetteListe;
        private System.Windows.Forms.ListBox listeProfils;
        private System.Windows.Forms.GroupBox groupeProfil;
        private System.Windows.Forms.Label etiquetteNom;
        private System.Windows.Forms.TextBox champNom;
        private System.Windows.Forms.Label etiquetteServeur;
        private System.Windows.Forms.TextBox champServeur;
        private System.Windows.Forms.Label etiquetteUtilisateur;
        private System.Windows.Forms.TextBox champUtilisateur;
        private System.Windows.Forms.Label etiquetteMotDePasse;
        private System.Windows.Forms.TextBox champMotDePasse;
        private System.Windows.Forms.Label etiquetteIndiceMdp;
        private System.Windows.Forms.Label etiquetteBibliotheque;
        private System.Windows.Forms.TextBox champBibliotheque;
        private System.Windows.Forms.CheckBox caseLectureSeule;
        private System.Windows.Forms.Label etiquetteDescription;
        private System.Windows.Forms.TextBox champDescription;
        private System.Windows.Forms.Button boutonTester;
        private System.Windows.Forms.Button boutonNouveau;
        private System.Windows.Forms.Button boutonEnregistrer;
        private System.Windows.Forms.Button boutonSupprimer;
        private System.Windows.Forms.Button boutonDefaut;
        private System.Windows.Forms.Label etiquetteStatut;
        private System.Windows.Forms.Button boutonFermer;
    }
}

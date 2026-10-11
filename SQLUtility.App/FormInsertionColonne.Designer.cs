namespace SQLUtility.App
{
    partial class FormInsertionColonne
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
            this.etiquetteConnexion = new System.Windows.Forms.Label();
            this.listeConnexions = new System.Windows.Forms.ComboBox();
            this.etiquetteSchema = new System.Windows.Forms.Label();
            this.listeSchemas = new System.Windows.Forms.ComboBox();
            this.etiquetteTable = new System.Windows.Forms.Label();
            this.listeTables = new System.Windows.Forms.ComboBox();
            this.etiquetteColonne = new System.Windows.Forms.Label();
            this.listeColonnes = new System.Windows.Forms.ListBox();
            this.etiquetteDetail = new System.Windows.Forms.Label();
            this.groupeDate = new System.Windows.Forms.GroupBox();
            this.listeFormats = new System.Windows.Forms.ComboBox();
            this.etiquetteFormat = new System.Windows.Forms.Label();
            this.caseConvertir = new System.Windows.Forms.CheckBox();
            this.etiquetteApercuTitre = new System.Windows.Forms.Label();
            this.champApercu = new System.Windows.Forms.TextBox();
            this.boutonInserer = new System.Windows.Forms.Button();
            this.boutonAnnuler = new System.Windows.Forms.Button();
            this.barreStatut = new System.Windows.Forms.StatusStrip();
            this.etiquetteStatut = new System.Windows.Forms.ToolStripStatusLabel();
            this.groupeDate.SuspendLayout();
            this.barreStatut.SuspendLayout();
            this.SuspendLayout();
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
            // listeConnexions
            //
            this.listeConnexions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeConnexions.FormattingEnabled = true;
            this.listeConnexions.Location = new System.Drawing.Point(110, 12);
            this.listeConnexions.Name = "listeConnexions";
            this.listeConnexions.Size = new System.Drawing.Size(410, 21);
            this.listeConnexions.TabIndex = 1;
            this.listeConnexions.SelectedIndexChanged += new System.EventHandler(this.listeConnexions_SelectedIndexChanged);
            //
            // etiquetteSchema
            //
            this.etiquetteSchema.AutoSize = true;
            this.etiquetteSchema.Location = new System.Drawing.Point(12, 45);
            this.etiquetteSchema.Name = "etiquetteSchema";
            this.etiquetteSchema.Size = new System.Drawing.Size(52, 13);
            this.etiquetteSchema.TabIndex = 2;
            this.etiquetteSchema.Text = "Schéma :";
            //
            // listeSchemas
            //
            this.listeSchemas.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeSchemas.FormattingEnabled = true;
            this.listeSchemas.Location = new System.Drawing.Point(110, 42);
            this.listeSchemas.Name = "listeSchemas";
            this.listeSchemas.Size = new System.Drawing.Size(410, 21);
            this.listeSchemas.TabIndex = 3;
            this.listeSchemas.SelectedIndexChanged += new System.EventHandler(this.listeSchemas_SelectedIndexChanged);
            //
            // etiquetteTable
            //
            this.etiquetteTable.AutoSize = true;
            this.etiquetteTable.Location = new System.Drawing.Point(12, 75);
            this.etiquetteTable.Name = "etiquetteTable";
            this.etiquetteTable.Size = new System.Drawing.Size(40, 13);
            this.etiquetteTable.TabIndex = 4;
            this.etiquetteTable.Text = "Table :";
            //
            // listeTables
            //
            this.listeTables.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeTables.FormattingEnabled = true;
            this.listeTables.Location = new System.Drawing.Point(110, 72);
            this.listeTables.Name = "listeTables";
            this.listeTables.Size = new System.Drawing.Size(410, 21);
            this.listeTables.TabIndex = 5;
            this.listeTables.SelectedIndexChanged += new System.EventHandler(this.listeTables_SelectedIndexChanged);
            //
            // etiquetteColonne
            //
            this.etiquetteColonne.AutoSize = true;
            this.etiquetteColonne.Location = new System.Drawing.Point(12, 108);
            this.etiquetteColonne.Name = "etiquetteColonne";
            this.etiquetteColonne.Size = new System.Drawing.Size(54, 13);
            this.etiquetteColonne.TabIndex = 6;
            this.etiquetteColonne.Text = "Colonne :";
            //
            // listeColonnes
            //
            this.listeColonnes.FormattingEnabled = true;
            this.listeColonnes.IntegralHeight = false;
            this.listeColonnes.Location = new System.Drawing.Point(110, 105);
            this.listeColonnes.Name = "listeColonnes";
            this.listeColonnes.Size = new System.Drawing.Size(410, 121);
            this.listeColonnes.TabIndex = 7;
            this.listeColonnes.SelectedIndexChanged += new System.EventHandler(this.listeColonnes_SelectedIndexChanged);
            //
            // etiquetteDetail
            //
            this.etiquetteDetail.AutoEllipsis = true;
            this.etiquetteDetail.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteDetail.Location = new System.Drawing.Point(110, 229);
            this.etiquetteDetail.Name = "etiquetteDetail";
            this.etiquetteDetail.Size = new System.Drawing.Size(410, 16);
            this.etiquetteDetail.TabIndex = 8;
            //
            // groupeDate
            //
            this.groupeDate.Controls.Add(this.listeFormats);
            this.groupeDate.Controls.Add(this.etiquetteFormat);
            this.groupeDate.Controls.Add(this.caseConvertir);
            this.groupeDate.Enabled = false;
            this.groupeDate.Location = new System.Drawing.Point(12, 252);
            this.groupeDate.Name = "groupeDate";
            this.groupeDate.Size = new System.Drawing.Size(508, 80);
            this.groupeDate.TabIndex = 9;
            this.groupeDate.TabStop = false;
            this.groupeDate.Text = "Date encodée";
            //
            // listeFormats
            //
            this.listeFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeFormats.FormattingEnabled = true;
            this.listeFormats.Location = new System.Drawing.Point(72, 47);
            this.listeFormats.Name = "listeFormats";
            this.listeFormats.Size = new System.Drawing.Size(424, 21);
            this.listeFormats.TabIndex = 2;
            this.listeFormats.SelectedIndexChanged += new System.EventHandler(this.listeFormats_SelectedIndexChanged);
            //
            // etiquetteFormat
            //
            this.etiquetteFormat.AutoSize = true;
            this.etiquetteFormat.Location = new System.Drawing.Point(14, 50);
            this.etiquetteFormat.Name = "etiquetteFormat";
            this.etiquetteFormat.Size = new System.Drawing.Size(48, 13);
            this.etiquetteFormat.TabIndex = 1;
            this.etiquetteFormat.Text = "Format :";
            //
            // caseConvertir
            //
            this.caseConvertir.AutoSize = true;
            this.caseConvertir.Location = new System.Drawing.Point(16, 22);
            this.caseConvertir.Name = "caseConvertir";
            this.caseConvertir.Size = new System.Drawing.Size(213, 17);
            this.caseConvertir.TabIndex = 0;
            this.caseConvertir.Text = "Convertir en DATE (décodage SQL auto)";
            this.caseConvertir.UseVisualStyleBackColor = true;
            this.caseConvertir.CheckedChanged += new System.EventHandler(this.caseConvertir_CheckedChanged);
            //
            // etiquetteApercuTitre
            //
            this.etiquetteApercuTitre.AutoSize = true;
            this.etiquetteApercuTitre.Location = new System.Drawing.Point(12, 342);
            this.etiquetteApercuTitre.Name = "etiquetteApercuTitre";
            this.etiquetteApercuTitre.Size = new System.Drawing.Size(116, 13);
            this.etiquetteApercuTitre.TabIndex = 10;
            this.etiquetteApercuTitre.Text = "Aperçu de l'insertion :";
            //
            // champApercu
            //
            this.champApercu.Font = new System.Drawing.Font("Consolas", 9F);
            this.champApercu.Location = new System.Drawing.Point(12, 358);
            this.champApercu.Multiline = true;
            this.champApercu.Name = "champApercu";
            this.champApercu.ReadOnly = true;
            this.champApercu.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.champApercu.Size = new System.Drawing.Size(508, 64);
            this.champApercu.TabIndex = 11;
            //
            // boutonInserer
            //
            this.boutonInserer.Location = new System.Drawing.Point(352, 432);
            this.boutonInserer.Name = "boutonInserer";
            this.boutonInserer.Size = new System.Drawing.Size(80, 28);
            this.boutonInserer.TabIndex = 12;
            this.boutonInserer.Text = "Insérer";
            this.boutonInserer.UseVisualStyleBackColor = true;
            this.boutonInserer.Click += new System.EventHandler(this.boutonInserer_Click);
            //
            // boutonAnnuler
            //
            this.boutonAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.boutonAnnuler.Location = new System.Drawing.Point(440, 432);
            this.boutonAnnuler.Name = "boutonAnnuler";
            this.boutonAnnuler.Size = new System.Drawing.Size(80, 28);
            this.boutonAnnuler.TabIndex = 13;
            this.boutonAnnuler.Text = "Annuler";
            this.boutonAnnuler.UseVisualStyleBackColor = true;
            //
            // barreStatut
            //
            this.barreStatut.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.etiquetteStatut});
            this.barreStatut.Location = new System.Drawing.Point(0, 479);
            this.barreStatut.Name = "barreStatut";
            this.barreStatut.Size = new System.Drawing.Size(532, 22);
            this.barreStatut.TabIndex = 14;
            //
            // etiquetteStatut
            //
            this.etiquetteStatut.Name = "etiquetteStatut";
            this.etiquetteStatut.Size = new System.Drawing.Size(39, 17);
            this.etiquetteStatut.Text = "Prêt.";
            //
            // FormInsertionColonne
            //
            this.AcceptButton = this.boutonInserer;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.boutonAnnuler;
            this.ClientSize = new System.Drawing.Size(532, 501);
            this.Controls.Add(this.barreStatut);
            this.Controls.Add(this.boutonAnnuler);
            this.Controls.Add(this.boutonInserer);
            this.Controls.Add(this.champApercu);
            this.Controls.Add(this.etiquetteApercuTitre);
            this.Controls.Add(this.groupeDate);
            this.Controls.Add(this.etiquetteDetail);
            this.Controls.Add(this.listeColonnes);
            this.Controls.Add(this.etiquetteColonne);
            this.Controls.Add(this.listeTables);
            this.Controls.Add(this.etiquetteTable);
            this.Controls.Add(this.listeSchemas);
            this.Controls.Add(this.etiquetteSchema);
            this.Controls.Add(this.listeConnexions);
            this.Controls.Add(this.etiquetteConnexion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormInsertionColonne";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Insérer une colonne";
            this.groupeDate.ResumeLayout(false);
            this.groupeDate.PerformLayout();
            this.barreStatut.ResumeLayout(false);
            this.barreStatut.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label etiquetteConnexion;
        private System.Windows.Forms.ComboBox listeConnexions;
        private System.Windows.Forms.Label etiquetteSchema;
        private System.Windows.Forms.ComboBox listeSchemas;
        private System.Windows.Forms.Label etiquetteTable;
        private System.Windows.Forms.ComboBox listeTables;
        private System.Windows.Forms.Label etiquetteColonne;
        private System.Windows.Forms.ListBox listeColonnes;
        private System.Windows.Forms.Label etiquetteDetail;
        private System.Windows.Forms.GroupBox groupeDate;
        private System.Windows.Forms.ComboBox listeFormats;
        private System.Windows.Forms.Label etiquetteFormat;
        private System.Windows.Forms.CheckBox caseConvertir;
        private System.Windows.Forms.Label etiquetteApercuTitre;
        private System.Windows.Forms.TextBox champApercu;
        private System.Windows.Forms.Button boutonInserer;
        private System.Windows.Forms.Button boutonAnnuler;
        private System.Windows.Forms.StatusStrip barreStatut;
        private System.Windows.Forms.ToolStripStatusLabel etiquetteStatut;
    }
}

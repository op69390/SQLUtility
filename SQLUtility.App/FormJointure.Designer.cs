namespace SQLUtility.App
{
    partial class FormJointure
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
            this.etiquetteTitre = new System.Windows.Forms.Label();
            this.etiquettePropositions = new System.Windows.Forms.Label();
            this.listePropositions = new System.Windows.Forms.ListBox();
            this.etiquetteTableLiee = new System.Windows.Forms.Label();
            this.listeTableLiee = new System.Windows.Forms.ComboBox();
            this.grilleConditions = new System.Windows.Forms.DataGridView();
            this.colJointe = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colLiee = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.radioInterne = new System.Windows.Forms.RadioButton();
            this.radioExterne = new System.Windows.Forms.RadioButton();
            this.etiquetteAvertissement = new System.Windows.Forms.Label();
            this.boutonOk = new System.Windows.Forms.Button();
            this.boutonAnnuler = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grilleConditions)).BeginInit();
            this.SuspendLayout();
            //
            // etiquetteTitre
            //
            this.etiquetteTitre.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteTitre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.etiquetteTitre.Location = new System.Drawing.Point(12, 10);
            this.etiquetteTitre.Name = "etiquetteTitre";
            this.etiquetteTitre.Size = new System.Drawing.Size(736, 22);
            this.etiquetteTitre.TabIndex = 0;
            //
            // etiquettePropositions
            //
            this.etiquettePropositions.AutoSize = true;
            this.etiquettePropositions.Location = new System.Drawing.Point(12, 40);
            this.etiquettePropositions.Name = "etiquettePropositions";
            this.etiquettePropositions.Size = new System.Drawing.Size(250, 13);
            this.etiquettePropositions.TabIndex = 1;
            this.etiquettePropositions.Text = "Jointures proposées (de la plus sûre à la moins sûre) :";
            //
            // listePropositions
            //
            this.listePropositions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listePropositions.FormattingEnabled = true;
            this.listePropositions.HorizontalScrollbar = true;
            this.listePropositions.IntegralHeight = false;
            this.listePropositions.Location = new System.Drawing.Point(12, 58);
            this.listePropositions.Name = "listePropositions";
            this.listePropositions.Size = new System.Drawing.Size(736, 110);
            this.listePropositions.TabIndex = 2;
            this.listePropositions.SelectedIndexChanged += new System.EventHandler(this.listePropositions_SelectedIndexChanged);
            //
            // etiquetteTableLiee
            //
            this.etiquetteTableLiee.AutoSize = true;
            this.etiquetteTableLiee.Location = new System.Drawing.Point(12, 181);
            this.etiquetteTableLiee.Name = "etiquetteTableLiee";
            this.etiquetteTableLiee.Size = new System.Drawing.Size(110, 13);
            this.etiquetteTableLiee.TabIndex = 3;
            this.etiquetteTableLiee.Text = "Liée à la table :";
            //
            // listeTableLiee
            //
            this.listeTableLiee.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.listeTableLiee.FormattingEnabled = true;
            this.listeTableLiee.Location = new System.Drawing.Point(128, 178);
            this.listeTableLiee.Name = "listeTableLiee";
            this.listeTableLiee.Size = new System.Drawing.Size(300, 21);
            this.listeTableLiee.TabIndex = 4;
            this.listeTableLiee.SelectionChangeCommitted += new System.EventHandler(this.listeTableLiee_SelectionChangeCommitted);
            //
            // grilleConditions
            //
            this.grilleConditions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grilleConditions.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleConditions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleConditions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colJointe,
            this.colLiee});
            this.grilleConditions.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grilleConditions.Location = new System.Drawing.Point(12, 208);
            this.grilleConditions.Name = "grilleConditions";
            this.grilleConditions.RowHeadersWidth = 24;
            this.grilleConditions.Size = new System.Drawing.Size(736, 130);
            this.grilleConditions.TabIndex = 5;
            this.grilleConditions.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grilleConditions_CellValueChanged);
            this.grilleConditions.CurrentCellDirtyStateChanged += new System.EventHandler(this.grilleConditions_CurrentCellDirtyStateChanged);
            this.grilleConditions.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.grilleConditions_DataError);
            //
            // colJointe
            //
            this.colJointe.HeaderText = "Colonne de la table jointe";
            this.colJointe.Name = "colJointe";
            //
            // colLiee
            //
            this.colLiee.HeaderText = "= colonne de la table liée";
            this.colLiee.Name = "colLiee";
            //
            // radioInterne
            //
            this.radioInterne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioInterne.AutoSize = true;
            this.radioInterne.Checked = true;
            this.radioInterne.Location = new System.Drawing.Point(12, 348);
            this.radioInterne.Name = "radioInterne";
            this.radioInterne.Size = new System.Drawing.Size(420, 17);
            this.radioInterne.TabIndex = 6;
            this.radioInterne.TabStop = true;
            this.radioInterne.Text = "Interne (INNER JOIN) : seulement les lignes qui ont une correspondance";
            this.radioInterne.UseVisualStyleBackColor = true;
            //
            // radioExterne
            //
            this.radioExterne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioExterne.AutoSize = true;
            this.radioExterne.Location = new System.Drawing.Point(12, 371);
            this.radioExterne.Name = "radioExterne";
            this.radioExterne.Size = new System.Drawing.Size(500, 17);
            this.radioExterne.TabIndex = 7;
            this.radioExterne.Text = "Externe (LEFT OUTER JOIN) : garder aussi les lignes sans correspondance (colonnes à NULL)";
            this.radioExterne.UseVisualStyleBackColor = true;
            //
            // etiquetteAvertissement
            //
            this.etiquetteAvertissement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteAvertissement.ForeColor = System.Drawing.Color.FromArgb(160, 82, 0);
            this.etiquetteAvertissement.Location = new System.Drawing.Point(12, 396);
            this.etiquetteAvertissement.Name = "etiquetteAvertissement";
            this.etiquetteAvertissement.Size = new System.Drawing.Size(540, 40);
            this.etiquetteAvertissement.TabIndex = 8;
            this.etiquetteAvertissement.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // boutonOk
            //
            this.boutonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.boutonOk.Location = new System.Drawing.Point(562, 404);
            this.boutonOk.Name = "boutonOk";
            this.boutonOk.Size = new System.Drawing.Size(90, 28);
            this.boutonOk.TabIndex = 9;
            this.boutonOk.Text = "Joindre";
            this.boutonOk.UseVisualStyleBackColor = true;
            this.boutonOk.Click += new System.EventHandler(this.boutonOk_Click);
            //
            // boutonAnnuler
            //
            this.boutonAnnuler.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonAnnuler.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.boutonAnnuler.Location = new System.Drawing.Point(658, 404);
            this.boutonAnnuler.Name = "boutonAnnuler";
            this.boutonAnnuler.Size = new System.Drawing.Size(90, 28);
            this.boutonAnnuler.TabIndex = 10;
            this.boutonAnnuler.Text = "Annuler";
            this.boutonAnnuler.UseVisualStyleBackColor = true;
            //
            // FormJointure
            //
            this.AcceptButton = this.boutonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.boutonAnnuler;
            this.ClientSize = new System.Drawing.Size(760, 444);
            this.Controls.Add(this.boutonAnnuler);
            this.Controls.Add(this.boutonOk);
            this.Controls.Add(this.etiquetteAvertissement);
            this.Controls.Add(this.radioExterne);
            this.Controls.Add(this.radioInterne);
            this.Controls.Add(this.grilleConditions);
            this.Controls.Add(this.listeTableLiee);
            this.Controls.Add(this.etiquetteTableLiee);
            this.Controls.Add(this.listePropositions);
            this.Controls.Add(this.etiquettePropositions);
            this.Controls.Add(this.etiquetteTitre);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "FormJointure";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Joindre une table";
            ((System.ComponentModel.ISupportInitialize)(this.grilleConditions)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label etiquetteTitre;
        private System.Windows.Forms.Label etiquettePropositions;
        private System.Windows.Forms.ListBox listePropositions;
        private System.Windows.Forms.Label etiquetteTableLiee;
        private System.Windows.Forms.ComboBox listeTableLiee;
        private System.Windows.Forms.DataGridView grilleConditions;
        private System.Windows.Forms.DataGridViewComboBoxColumn colJointe;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLiee;
        private System.Windows.Forms.RadioButton radioInterne;
        private System.Windows.Forms.RadioButton radioExterne;
        private System.Windows.Forms.Label etiquetteAvertissement;
        private System.Windows.Forms.Button boutonOk;
        private System.Windows.Forms.Button boutonAnnuler;
    }
}

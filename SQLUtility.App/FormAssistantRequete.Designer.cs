namespace SQLUtility.App
{
    partial class FormAssistantRequete
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
            this.panneauTable = new System.Windows.Forms.Panel();
            this.etiquetteSchema = new System.Windows.Forms.Label();
            this.listeSchemas = new System.Windows.Forms.ComboBox();
            this.etiquetteTable = new System.Windows.Forms.Label();
            this.listeTables = new System.Windows.Forms.ComboBox();
            this.boutonChargerTable = new System.Windows.Forms.Button();
            this.boutonJoindre = new System.Windows.Forms.Button();
            this.boutonEtape = new System.Windows.Forms.Button();
            this.boutonRetirerEtape = new System.Windows.Forms.Button();
            this.panneauTables = new System.Windows.Forms.Panel();
            this.etiquetteTables = new System.Windows.Forms.Label();
            this.grilleTables = new System.Windows.Forms.DataGridView();
            this.colTAlias = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTJointure = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTConditions = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.boutonModifierJointure = new System.Windows.Forms.Button();
            this.boutonRetirerTable = new System.Windows.Forms.Button();
            this.colTableC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.boutonExistence = new System.Windows.Forms.Button();
            this.ongletLot = new System.Windows.Forms.TabPage();
            this.panneauLotModes = new System.Windows.Forms.Panel();
            this.radioLotSupprimer = new System.Windows.Forms.RadioButton();
            this.radioLotModifier = new System.Windows.Forms.RadioButton();
            this.radioLotCopier = new System.Windows.Forms.RadioButton();
            this.etiquetteAideLot = new System.Windows.Forms.Label();
            this.panneauLotOutils = new System.Windows.Forms.Panel();
            this.boutonSourceLot = new System.Windows.Forms.Button();
            this.boutonCibleLot = new System.Windows.Forms.Button();
            this.etiquetteSourceLot = new System.Windows.Forms.Label();
            this.grilleLot = new System.Windows.Forms.DataGridView();
            this.colLNom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLLibelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLSource = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colLValeur = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLNull = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colLFormat = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.etiquetteInfoTable = new System.Windows.Forms.Label();
            this.onglets = new System.Windows.Forms.TabControl();
            this.ongletConsulter = new System.Windows.Forms.TabPage();
            this.separateurConsulter = new System.Windows.Forms.SplitContainer();
            this.grilleChoixColonnes = new System.Windows.Forms.DataGridView();
            this.colAfficher = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colNom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLibelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTri = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colRangTri = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCalcul = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colFormatC = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.caseCompter = new System.Windows.Forms.CheckBox();
            this.colFiltrePorte = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.panneauOptions = new System.Windows.Forms.Panel();
            this.boutonToutCocher = new System.Windows.Forms.Button();
            this.boutonToutDecocher = new System.Windows.Forms.Button();
            this.caseDistinct = new System.Windows.Forms.CheckBox();
            this.caseRrn = new System.Windows.Forms.CheckBox();
            this.etiquetteLimite = new System.Windows.Forms.Label();
            this.champLimite = new System.Windows.Forms.NumericUpDown();
            this.etiquetteAideLimite = new System.Windows.Forms.Label();
            this.grilleFiltres = new System.Windows.Forms.DataGridView();
            this.colFiltreColonne = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colFiltreOperateur = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colFiltreValeur = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFiltreValeur2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFiltreFormat = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.etiquetteFiltres = new System.Windows.Forms.Label();
            this.ongletLigne = new System.Windows.Forms.TabPage();
            this.grilleValeurs = new System.Windows.Forms.DataGridView();
            this.colVNom = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVLibelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVCle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVActuelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVNouvelle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVNull = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colVFormat = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.panneauModes = new System.Windows.Forms.Panel();
            this.radioAjouter = new System.Windows.Forms.RadioButton();
            this.radioModifier = new System.Windows.Forms.RadioButton();
            this.radioSupprimer = new System.Windows.Forms.RadioButton();
            this.etiquetteRrn = new System.Windows.Forms.Label();
            this.champRrn = new System.Windows.Forms.TextBox();
            this.boutonChargerLigne = new System.Windows.Forms.Button();
            this.etiquetteAideLigne = new System.Windows.Forms.Label();
            this.panneauBas = new System.Windows.Forms.Panel();
            this.apercuSql = new System.Windows.Forms.TextBox();
            this.etiquetteApercu = new System.Windows.Forms.Label();
            this.panneauBoutons = new System.Windows.Forms.Panel();
            this.etiquetteErreurs = new System.Windows.Forms.Label();
            this.boutonInserer = new System.Windows.Forms.Button();
            this.boutonFermer = new System.Windows.Forms.Button();
            this.panneauTable.SuspendLayout();
            this.onglets.SuspendLayout();
            this.ongletConsulter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.separateurConsulter)).BeginInit();
            this.separateurConsulter.Panel1.SuspendLayout();
            this.separateurConsulter.Panel2.SuspendLayout();
            this.separateurConsulter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleChoixColonnes)).BeginInit();
            this.panneauTables.SuspendLayout();
            this.ongletLot.SuspendLayout();
            this.panneauLotModes.SuspendLayout();
            this.panneauLotOutils.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleLot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grilleTables)).BeginInit();
            this.panneauOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.champLimite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grilleFiltres)).BeginInit();
            this.ongletLigne.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleValeurs)).BeginInit();
            this.panneauModes.SuspendLayout();
            this.panneauBas.SuspendLayout();
            this.panneauBoutons.SuspendLayout();
            this.SuspendLayout();
            //
            // panneauTable
            //
            this.panneauTable.Controls.Add(this.etiquetteInfoTable);
            this.panneauTable.Controls.Add(this.boutonRetirerEtape);
            this.panneauTable.Controls.Add(this.boutonEtape);
            this.panneauTable.Controls.Add(this.boutonJoindre);
            this.panneauTable.Controls.Add(this.boutonChargerTable);
            this.panneauTable.Controls.Add(this.listeTables);
            this.panneauTable.Controls.Add(this.etiquetteTable);
            this.panneauTable.Controls.Add(this.listeSchemas);
            this.panneauTable.Controls.Add(this.etiquetteSchema);
            this.panneauTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauTable.Location = new System.Drawing.Point(0, 0);
            this.panneauTable.Name = "panneauTable";
            this.panneauTable.Size = new System.Drawing.Size(984, 64);
            this.panneauTable.TabIndex = 0;
            //
            // etiquetteSchema
            //
            this.etiquetteSchema.AutoSize = true;
            this.etiquetteSchema.Location = new System.Drawing.Point(12, 13);
            this.etiquetteSchema.Name = "etiquetteSchema";
            this.etiquetteSchema.Size = new System.Drawing.Size(52, 13);
            this.etiquetteSchema.TabIndex = 0;
            this.etiquetteSchema.Text = "Schéma :";
            //
            // listeSchemas
            //
            this.listeSchemas.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.listeSchemas.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.listeSchemas.FormattingEnabled = true;
            this.listeSchemas.Location = new System.Drawing.Point(70, 10);
            this.listeSchemas.Name = "listeSchemas";
            this.listeSchemas.Size = new System.Drawing.Size(150, 21);
            this.listeSchemas.TabIndex = 1;
            this.listeSchemas.SelectionChangeCommitted += new System.EventHandler(this.listeSchemas_SelectionChangeCommitted);
            this.listeSchemas.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listeSchemas_KeyDown);
            //
            // etiquetteTable
            //
            this.etiquetteTable.AutoSize = true;
            this.etiquetteTable.Location = new System.Drawing.Point(234, 13);
            this.etiquetteTable.Name = "etiquetteTable";
            this.etiquetteTable.Size = new System.Drawing.Size(40, 13);
            this.etiquetteTable.TabIndex = 2;
            this.etiquetteTable.Text = "Table :";
            //
            // listeTables
            //
            this.listeTables.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.listeTables.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.listeTables.FormattingEnabled = true;
            this.listeTables.Location = new System.Drawing.Point(280, 10);
            this.listeTables.Name = "listeTables";
            this.listeTables.Size = new System.Drawing.Size(330, 21);
            this.listeTables.TabIndex = 3;
            this.listeTables.SelectionChangeCommitted += new System.EventHandler(this.listeTables_SelectionChangeCommitted);
            this.listeTables.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listeTables_KeyDown);
            //
            // boutonChargerTable
            //
            this.boutonChargerTable.Location = new System.Drawing.Point(618, 8);
            this.boutonChargerTable.Name = "boutonChargerTable";
            this.boutonChargerTable.Size = new System.Drawing.Size(90, 25);
            this.boutonChargerTable.TabIndex = 4;
            this.boutonChargerTable.Text = "Charger";
            this.boutonChargerTable.UseVisualStyleBackColor = true;
            this.boutonChargerTable.Click += new System.EventHandler(this.boutonChargerTable_Click);
            //
            // boutonJoindre
            //
            this.boutonJoindre.Location = new System.Drawing.Point(714, 8);
            this.boutonJoindre.Name = "boutonJoindre";
            this.boutonJoindre.Size = new System.Drawing.Size(140, 25);
            this.boutonJoindre.TabIndex = 5;
            this.boutonJoindre.Text = "Joindre cette table…";
            this.boutonJoindre.UseVisualStyleBackColor = true;
            this.boutonJoindre.Click += new System.EventHandler(this.boutonJoindre_Click);
            //
            // boutonEtape
            //
            this.boutonEtape.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonEtape.Location = new System.Drawing.Point(862, 8);
            this.boutonEtape.Name = "boutonEtape";
            this.boutonEtape.Size = new System.Drawing.Size(112, 25);
            this.boutonEtape.TabIndex = 6;
            this.boutonEtape.Text = "En faire une étape";
            this.boutonEtape.UseVisualStyleBackColor = true;
            this.boutonEtape.Click += new System.EventHandler(this.boutonEtape_Click);
            //
            // boutonRetirerEtape
            //
            this.boutonRetirerEtape.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonRetirerEtape.Enabled = false;
            this.boutonRetirerEtape.Location = new System.Drawing.Point(862, 36);
            this.boutonRetirerEtape.Name = "boutonRetirerEtape";
            this.boutonRetirerEtape.Size = new System.Drawing.Size(112, 23);
            this.boutonRetirerEtape.TabIndex = 7;
            this.boutonRetirerEtape.Text = "Retirer l'étape";
            this.boutonRetirerEtape.UseVisualStyleBackColor = true;
            this.boutonRetirerEtape.Click += new System.EventHandler(this.boutonRetirerEtape_Click);
            //
            // panneauTables
            //
            this.panneauTables.Controls.Add(this.boutonExistence);
            this.panneauTables.Controls.Add(this.boutonRetirerTable);
            this.panneauTables.Controls.Add(this.boutonModifierJointure);
            this.panneauTables.Controls.Add(this.grilleTables);
            this.panneauTables.Controls.Add(this.etiquetteTables);
            this.panneauTables.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauTables.Location = new System.Drawing.Point(0, 0);
            this.panneauTables.Name = "panneauTables";
            this.panneauTables.Size = new System.Drawing.Size(976, 112);
            this.panneauTables.TabIndex = 0;
            //
            // etiquetteTables
            //
            this.etiquetteTables.AutoSize = true;
            this.etiquetteTables.Location = new System.Drawing.Point(6, 5);
            this.etiquetteTables.Name = "etiquetteTables";
            this.etiquetteTables.Size = new System.Drawing.Size(300, 13);
            this.etiquetteTables.TabIndex = 0;
            this.etiquetteTables.Text = "Tables de la requête (dans les filtres, colonnes sans préfixe = table A) :";
            //
            // grilleTables
            //
            this.grilleTables.AllowUserToAddRows = false;
            this.grilleTables.AllowUserToDeleteRows = false;
            this.grilleTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grilleTables.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleTables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleTables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTAlias,
            this.colTTable,
            this.colTJointure,
            this.colTConditions});
            this.grilleTables.Location = new System.Drawing.Point(6, 22);
            this.grilleTables.MultiSelect = false;
            this.grilleTables.Name = "grilleTables";
            this.grilleTables.ReadOnly = true;
            this.grilleTables.RowHeadersVisible = false;
            this.grilleTables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grilleTables.Size = new System.Drawing.Size(818, 86);
            this.grilleTables.TabIndex = 1;
            this.grilleTables.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grilleTables_CellDoubleClick);
            //
            // colTAlias
            //
            this.colTAlias.FillWeight = 25F;
            this.colTAlias.HeaderText = "Alias";
            this.colTAlias.Name = "colTAlias";
            //
            // colTTable
            //
            this.colTTable.FillWeight = 150F;
            this.colTTable.HeaderText = "Table";
            this.colTTable.Name = "colTTable";
            //
            // colTJointure
            //
            this.colTJointure.FillWeight = 110F;
            this.colTJointure.HeaderText = "Jointure";
            this.colTJointure.Name = "colTJointure";
            //
            // colTConditions
            //
            this.colTConditions.FillWeight = 220F;
            this.colTConditions.HeaderText = "Conditions";
            this.colTConditions.Name = "colTConditions";
            //
            // boutonModifierJointure
            //
            this.boutonModifierJointure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonModifierJointure.Location = new System.Drawing.Point(832, 20);
            this.boutonModifierJointure.Name = "boutonModifierJointure";
            this.boutonModifierJointure.Size = new System.Drawing.Size(138, 24);
            this.boutonModifierJointure.TabIndex = 2;
            this.boutonModifierJointure.Text = "Modifier…";
            this.boutonModifierJointure.UseVisualStyleBackColor = true;
            this.boutonModifierJointure.Click += new System.EventHandler(this.boutonModifierJointure_Click);
            //
            // boutonRetirerTable
            //
            this.boutonRetirerTable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonRetirerTable.Location = new System.Drawing.Point(832, 49);
            this.boutonRetirerTable.Name = "boutonRetirerTable";
            this.boutonRetirerTable.Size = new System.Drawing.Size(138, 24);
            this.boutonRetirerTable.TabIndex = 3;
            this.boutonRetirerTable.Text = "Retirer la table";
            this.boutonRetirerTable.UseVisualStyleBackColor = true;
            this.boutonRetirerTable.Click += new System.EventHandler(this.boutonRetirerTable_Click);
            //
            // boutonExistence
            //
            this.boutonExistence.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonExistence.Location = new System.Drawing.Point(832, 78);
            this.boutonExistence.Name = "boutonExistence";
            this.boutonExistence.Size = new System.Drawing.Size(138, 24);
            this.boutonExistence.TabIndex = 4;
            this.boutonExistence.Text = "Condition d'existence…";
            this.boutonExistence.UseVisualStyleBackColor = true;
            this.boutonExistence.Click += new System.EventHandler(this.boutonExistence_Click);
            //
            // ongletLot
            //
            this.ongletLot.Controls.Add(this.grilleLot);
            this.ongletLot.Controls.Add(this.panneauLotOutils);
            this.ongletLot.Controls.Add(this.panneauLotModes);
            this.ongletLot.Location = new System.Drawing.Point(4, 22);
            this.ongletLot.Name = "ongletLot";
            this.ongletLot.Size = new System.Drawing.Size(976, 441);
            this.ongletLot.TabIndex = 2;
            this.ongletLot.Text = "Traitement par lot";
            this.ongletLot.UseVisualStyleBackColor = true;
            //
            // panneauLotModes
            //
            this.panneauLotModes.Controls.Add(this.etiquetteAideLot);
            this.panneauLotModes.Controls.Add(this.radioLotCopier);
            this.panneauLotModes.Controls.Add(this.radioLotModifier);
            this.panneauLotModes.Controls.Add(this.radioLotSupprimer);
            this.panneauLotModes.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauLotModes.Location = new System.Drawing.Point(0, 0);
            this.panneauLotModes.Name = "panneauLotModes";
            this.panneauLotModes.Size = new System.Drawing.Size(976, 80);
            this.panneauLotModes.TabIndex = 0;
            //
            // radioLotSupprimer
            //
            this.radioLotSupprimer.AutoSize = true;
            this.radioLotSupprimer.Location = new System.Drawing.Point(12, 8);
            this.radioLotSupprimer.Name = "radioLotSupprimer";
            this.radioLotSupprimer.Size = new System.Drawing.Size(240, 17);
            this.radioLotSupprimer.TabIndex = 0;
            this.radioLotSupprimer.Text = "Supprimer les lignes consultées (DELETE)";
            this.radioLotSupprimer.UseVisualStyleBackColor = true;
            this.radioLotSupprimer.CheckedChanged += new System.EventHandler(this.ModeLot_CheckedChanged);
            //
            // radioLotModifier
            //
            this.radioLotModifier.AutoSize = true;
            this.radioLotModifier.Checked = true;
            this.radioLotModifier.Location = new System.Drawing.Point(12, 31);
            this.radioLotModifier.Name = "radioLotModifier";
            this.radioLotModifier.Size = new System.Drawing.Size(240, 17);
            this.radioLotModifier.TabIndex = 1;
            this.radioLotModifier.TabStop = true;
            this.radioLotModifier.Text = "Modifier les lignes consultées (UPDATE)";
            this.radioLotModifier.UseVisualStyleBackColor = true;
            this.radioLotModifier.CheckedChanged += new System.EventHandler(this.ModeLot_CheckedChanged);
            //
            // radioLotCopier
            //
            this.radioLotCopier.AutoSize = true;
            this.radioLotCopier.Location = new System.Drawing.Point(12, 54);
            this.radioLotCopier.Name = "radioLotCopier";
            this.radioLotCopier.Size = new System.Drawing.Size(330, 17);
            this.radioLotCopier.TabIndex = 2;
            this.radioLotCopier.Text = "Copier les lignes consultées dans une autre table (INSERT … SELECT)";
            this.radioLotCopier.UseVisualStyleBackColor = true;
            this.radioLotCopier.CheckedChanged += new System.EventHandler(this.ModeLot_CheckedChanged);
            //
            // etiquetteAideLot
            //
            this.etiquetteAideLot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteAideLot.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteAideLot.Location = new System.Drawing.Point(420, 4);
            this.etiquetteAideLot.Name = "etiquetteAideLot";
            this.etiquetteAideLot.Size = new System.Drawing.Size(550, 72);
            this.etiquetteAideLot.TabIndex = 3;
            this.etiquetteAideLot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // panneauLotOutils
            //
            this.panneauLotOutils.Controls.Add(this.etiquetteSourceLot);
            this.panneauLotOutils.Controls.Add(this.boutonCibleLot);
            this.panneauLotOutils.Controls.Add(this.boutonSourceLot);
            this.panneauLotOutils.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauLotOutils.Location = new System.Drawing.Point(0, 80);
            this.panneauLotOutils.Name = "panneauLotOutils";
            this.panneauLotOutils.Size = new System.Drawing.Size(976, 34);
            this.panneauLotOutils.TabIndex = 1;
            //
            // boutonSourceLot
            //
            this.boutonSourceLot.Location = new System.Drawing.Point(6, 5);
            this.boutonSourceLot.Name = "boutonSourceLot";
            this.boutonSourceLot.Size = new System.Drawing.Size(300, 24);
            this.boutonSourceLot.TabIndex = 0;
            this.boutonSourceLot.Text = "Prendre des valeurs dans la table choisie en haut…";
            this.boutonSourceLot.UseVisualStyleBackColor = true;
            this.boutonSourceLot.Click += new System.EventHandler(this.boutonSourceLot_Click);
            //
            // boutonCibleLot
            //
            this.boutonCibleLot.Location = new System.Drawing.Point(6, 5);
            this.boutonCibleLot.Name = "boutonCibleLot";
            this.boutonCibleLot.Size = new System.Drawing.Size(300, 24);
            this.boutonCibleLot.TabIndex = 1;
            this.boutonCibleLot.Text = "Copier vers la table choisie en haut…";
            this.boutonCibleLot.UseVisualStyleBackColor = true;
            this.boutonCibleLot.Click += new System.EventHandler(this.boutonCibleLot_Click);
            //
            // etiquetteSourceLot
            //
            this.etiquetteSourceLot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteSourceLot.Location = new System.Drawing.Point(314, 10);
            this.etiquetteSourceLot.Name = "etiquetteSourceLot";
            this.etiquetteSourceLot.Size = new System.Drawing.Size(656, 16);
            this.etiquetteSourceLot.TabIndex = 2;
            //
            // grilleLot
            //
            this.grilleLot.AllowUserToAddRows = false;
            this.grilleLot.AllowUserToDeleteRows = false;
            this.grilleLot.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleLot.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleLot.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLNom,
            this.colLLibelle,
            this.colLType,
            this.colLSource,
            this.colLValeur,
            this.colLNull,
            this.colLFormat});
            this.grilleLot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleLot.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grilleLot.Location = new System.Drawing.Point(0, 114);
            this.grilleLot.Name = "grilleLot";
            this.grilleLot.RowHeadersVisible = false;
            this.grilleLot.Size = new System.Drawing.Size(976, 327);
            this.grilleLot.TabIndex = 2;
            this.grilleLot.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grille_CellValueChanged);
            this.grilleLot.CurrentCellDirtyStateChanged += new System.EventHandler(this.Grille_CurrentCellDirtyStateChanged);
            this.grilleLot.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.Grille_DataError);
            //
            // colLNom
            //
            this.colLNom.FillWeight = 80F;
            this.colLNom.HeaderText = "Colonne";
            this.colLNom.Name = "colLNom";
            this.colLNom.ReadOnly = true;
            //
            // colLLibelle
            //
            this.colLLibelle.FillWeight = 150F;
            this.colLLibelle.HeaderText = "Libellé";
            this.colLLibelle.Name = "colLLibelle";
            this.colLLibelle.ReadOnly = true;
            //
            // colLType
            //
            this.colLType.FillWeight = 80F;
            this.colLType.HeaderText = "Type";
            this.colLType.Name = "colLType";
            this.colLType.ReadOnly = true;
            //
            // colLSource
            //
            this.colLSource.FillWeight = 160F;
            this.colLSource.HeaderText = "Prendre la valeur de";
            this.colLSource.Name = "colLSource";
            //
            // colLValeur
            //
            this.colLValeur.FillWeight = 110F;
            this.colLValeur.HeaderText = "ou valeur fixe";
            this.colLValeur.Name = "colLValeur";
            //
            // colLNull
            //
            this.colLNull.FillWeight = 35F;
            this.colLNull.HeaderText = "NULL";
            this.colLNull.Name = "colLNull";
            //
            // colLFormat
            //
            this.colLFormat.FillWeight = 100F;
            this.colLFormat.HeaderText = "Date Atlas stockée en";
            this.colLFormat.Name = "colLFormat";
            //
            // colTableC
            //
            this.colTableC.FillWeight = 30F;
            this.colTableC.HeaderText = "Table";
            this.colTableC.Name = "colTableC";
            this.colTableC.ReadOnly = true;
            //
            // etiquetteInfoTable
            //
            this.etiquetteInfoTable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteInfoTable.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteInfoTable.Location = new System.Drawing.Point(12, 40);
            this.etiquetteInfoTable.Name = "etiquetteInfoTable";
            this.etiquetteInfoTable.Size = new System.Drawing.Size(844, 18);
            this.etiquetteInfoTable.TabIndex = 5;
            this.etiquetteInfoTable.Text = "Choisissez un schéma puis une table.";
            //
            // onglets
            //
            this.onglets.Controls.Add(this.ongletConsulter);
            this.onglets.Controls.Add(this.ongletLigne);
            this.onglets.Controls.Add(this.ongletLot);
            this.onglets.Dock = System.Windows.Forms.DockStyle.Fill;
            this.onglets.Location = new System.Drawing.Point(0, 64);
            this.onglets.Name = "onglets";
            this.onglets.SelectedIndex = 0;
            this.onglets.Size = new System.Drawing.Size(984, 467);
            this.onglets.TabIndex = 1;
            this.onglets.SelectedIndexChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // ongletConsulter
            //
            this.ongletConsulter.Controls.Add(this.separateurConsulter);
            this.ongletConsulter.Location = new System.Drawing.Point(4, 22);
            this.ongletConsulter.Name = "ongletConsulter";
            this.ongletConsulter.Size = new System.Drawing.Size(976, 441);
            this.ongletConsulter.TabIndex = 0;
            this.ongletConsulter.Text = "Consulter";
            this.ongletConsulter.UseVisualStyleBackColor = true;
            //
            // separateurConsulter
            //
            this.separateurConsulter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.separateurConsulter.Location = new System.Drawing.Point(0, 0);
            this.separateurConsulter.Name = "separateurConsulter";
            this.separateurConsulter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // separateurConsulter.Panel1
            //
            this.separateurConsulter.Panel1.Controls.Add(this.grilleChoixColonnes);
            this.separateurConsulter.Panel1.Controls.Add(this.panneauOptions);
            this.separateurConsulter.Panel1.Controls.Add(this.panneauTables);
            //
            // separateurConsulter.Panel2
            //
            this.separateurConsulter.Panel2.Controls.Add(this.grilleFiltres);
            this.separateurConsulter.Panel2.Controls.Add(this.etiquetteFiltres);
            this.separateurConsulter.Size = new System.Drawing.Size(976, 441);
            this.separateurConsulter.SplitterDistance = 320;
            this.separateurConsulter.TabIndex = 0;
            //
            // panneauOptions
            //
            this.panneauOptions.Controls.Add(this.caseCompter);
            this.panneauOptions.Controls.Add(this.etiquetteAideLimite);
            this.panneauOptions.Controls.Add(this.champLimite);
            this.panneauOptions.Controls.Add(this.etiquetteLimite);
            this.panneauOptions.Controls.Add(this.caseRrn);
            this.panneauOptions.Controls.Add(this.caseDistinct);
            this.panneauOptions.Controls.Add(this.boutonToutDecocher);
            this.panneauOptions.Controls.Add(this.boutonToutCocher);
            this.panneauOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauOptions.Location = new System.Drawing.Point(0, 0);
            this.panneauOptions.Name = "panneauOptions";
            this.panneauOptions.Size = new System.Drawing.Size(976, 34);
            this.panneauOptions.TabIndex = 0;
            //
            // boutonToutCocher
            //
            this.boutonToutCocher.Location = new System.Drawing.Point(6, 5);
            this.boutonToutCocher.Name = "boutonToutCocher";
            this.boutonToutCocher.Size = new System.Drawing.Size(90, 24);
            this.boutonToutCocher.TabIndex = 0;
            this.boutonToutCocher.Text = "Tout cocher";
            this.boutonToutCocher.UseVisualStyleBackColor = true;
            this.boutonToutCocher.Click += new System.EventHandler(this.boutonToutCocher_Click);
            //
            // boutonToutDecocher
            //
            this.boutonToutDecocher.Location = new System.Drawing.Point(100, 5);
            this.boutonToutDecocher.Name = "boutonToutDecocher";
            this.boutonToutDecocher.Size = new System.Drawing.Size(100, 24);
            this.boutonToutDecocher.TabIndex = 1;
            this.boutonToutDecocher.Text = "Tout décocher";
            this.boutonToutDecocher.UseVisualStyleBackColor = true;
            this.boutonToutDecocher.Click += new System.EventHandler(this.boutonToutDecocher_Click);
            //
            // caseDistinct
            //
            this.caseDistinct.AutoSize = true;
            this.caseDistinct.Location = new System.Drawing.Point(216, 9);
            this.caseDistinct.Name = "caseDistinct";
            this.caseDistinct.Size = new System.Drawing.Size(117, 17);
            this.caseDistinct.TabIndex = 2;
            this.caseDistinct.Text = "Sans doublons (DISTINCT)";
            this.caseDistinct.UseVisualStyleBackColor = true;
            this.caseDistinct.CheckedChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // caseRrn
            //
            this.caseRrn.AutoSize = true;
            this.caseRrn.Location = new System.Drawing.Point(386, 9);
            this.caseRrn.Name = "caseRrn";
            this.caseRrn.Size = new System.Drawing.Size(140, 17);
            this.caseRrn.TabIndex = 3;
            this.caseRrn.Text = "Inclure le n° d'enregistrement (RRN)";
            this.caseRrn.UseVisualStyleBackColor = true;
            this.caseRrn.CheckedChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // etiquetteLimite
            //
            this.etiquetteLimite.AutoSize = true;
            this.etiquetteLimite.Location = new System.Drawing.Point(600, 10);
            this.etiquetteLimite.Name = "etiquetteLimite";
            this.etiquetteLimite.Size = new System.Drawing.Size(42, 13);
            this.etiquetteLimite.TabIndex = 4;
            this.etiquetteLimite.Text = "Limite :";
            //
            // champLimite
            //
            this.champLimite.Location = new System.Drawing.Point(648, 7);
            this.champLimite.Maximum = new decimal(new int[] { 1000000, 0, 0, 0 });
            this.champLimite.Name = "champLimite";
            this.champLimite.Size = new System.Drawing.Size(80, 20);
            this.champLimite.TabIndex = 5;
            this.champLimite.ThousandsSeparator = true;
            this.champLimite.Value = new decimal(new int[] { 100, 0, 0, 0 });
            this.champLimite.ValueChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // etiquetteAideLimite
            //
            this.etiquetteAideLimite.AutoSize = true;
            this.etiquetteAideLimite.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteAideLimite.Location = new System.Drawing.Point(734, 10);
            this.etiquetteAideLimite.Name = "etiquetteAideLimite";
            this.etiquetteAideLimite.Size = new System.Drawing.Size(90, 13);
            this.etiquetteAideLimite.TabIndex = 6;
            this.etiquetteAideLimite.Text = "(0 = sans limite)";
            //
            // grilleChoixColonnes
            //
            this.grilleChoixColonnes.AllowUserToAddRows = false;
            this.grilleChoixColonnes.AllowUserToDeleteRows = false;
            this.grilleChoixColonnes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleChoixColonnes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleChoixColonnes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAfficher,
            this.colTableC,
            this.colNom,
            this.colLibelle,
            this.colType,
            this.colCle,
            this.colTri,
            this.colRangTri,
            this.colCalcul,
            this.colFormatC});
            this.grilleChoixColonnes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleChoixColonnes.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grilleChoixColonnes.Location = new System.Drawing.Point(0, 34);
            this.grilleChoixColonnes.Name = "grilleChoixColonnes";
            this.grilleChoixColonnes.RowHeadersVisible = false;
            this.grilleChoixColonnes.Size = new System.Drawing.Size(976, 236);
            this.grilleChoixColonnes.TabIndex = 1;
            this.grilleChoixColonnes.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grille_CellValueChanged);
            this.grilleChoixColonnes.CurrentCellDirtyStateChanged += new System.EventHandler(this.Grille_CurrentCellDirtyStateChanged);
            this.grilleChoixColonnes.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.Grille_DataError);
            //
            // colAfficher
            //
            this.colAfficher.FillWeight = 40F;
            this.colAfficher.HeaderText = "Afficher";
            this.colAfficher.Name = "colAfficher";
            //
            // colNom
            //
            this.colNom.FillWeight = 80F;
            this.colNom.HeaderText = "Colonne";
            this.colNom.Name = "colNom";
            this.colNom.ReadOnly = true;
            //
            // colLibelle
            //
            this.colLibelle.FillWeight = 200F;
            this.colLibelle.HeaderText = "Libellé";
            this.colLibelle.Name = "colLibelle";
            this.colLibelle.ReadOnly = true;
            //
            // colType
            //
            this.colType.FillWeight = 80F;
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            //
            // colCle
            //
            this.colCle.FillWeight = 30F;
            this.colCle.HeaderText = "Clé";
            this.colCle.Name = "colCle";
            this.colCle.ReadOnly = true;
            //
            // colTri
            //
            this.colTri.FillWeight = 70F;
            this.colTri.HeaderText = "Tri";
            this.colTri.Items.AddRange(new object[] { "", "Croissant", "Décroissant" });
            this.colTri.Name = "colTri";
            //
            // colRangTri
            //
            this.colRangTri.FillWeight = 45F;
            this.colRangTri.HeaderText = "Rang du tri";
            this.colRangTri.Name = "colRangTri";
            this.colRangTri.ToolTipText = "Ordre des critères de tri (1, 2, 3...)";
            //
            // colCalcul
            //
            this.colCalcul.FillWeight = 110F;
            this.colCalcul.HeaderText = "Calcul (regroupement)";
            this.colCalcul.Name = "colCalcul";
            this.colCalcul.ToolTipText = "Regrouper sur cette colonne, ou la totaliser : la requête devient un GROUP BY";
            //
            // colFormatC
            //
            this.colFormatC.FillWeight = 100F;
            this.colFormatC.HeaderText = "Date Atlas stockée en";
            this.colFormatC.Name = "colFormatC";
            this.colFormatC.ToolTipText = "Pour regrouper une date Atlas numérique par année, mois ou jour";
            //
            // caseCompter
            //
            this.caseCompter.AutoSize = true;
            this.caseCompter.Location = new System.Drawing.Point(836, 9);
            this.caseCompter.Name = "caseCompter";
            this.caseCompter.Size = new System.Drawing.Size(110, 17);
            this.caseCompter.TabIndex = 7;
            this.caseCompter.Text = "Nombre de lignes";
            this.caseCompter.UseVisualStyleBackColor = true;
            this.caseCompter.CheckedChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // colFiltrePorte
            //
            this.colFiltrePorte.FillWeight = 110F;
            this.colFiltrePorte.HeaderText = "Porte sur";
            this.colFiltrePorte.Name = "colFiltrePorte";
            this.colFiltrePorte.ToolTipText = "Les lignes (WHERE) ou un calcul par groupe (HAVING)";
            //
            // etiquetteFiltres
            //
            this.etiquetteFiltres.Dock = System.Windows.Forms.DockStyle.Top;
            this.etiquetteFiltres.Location = new System.Drawing.Point(0, 0);
            this.etiquetteFiltres.Name = "etiquetteFiltres";
            this.etiquetteFiltres.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.etiquetteFiltres.Size = new System.Drawing.Size(976, 22);
            this.etiquetteFiltres.TabIndex = 0;
            this.etiquetteFiltres.Text = "Filtres (tous combinés par ET) — dates : jj/mm/aaaa ; « dans la liste » : valeurs séparées par « ; » ; « Porte sur » un calcul = filtre par groupe (HAVING).";
            //
            // grilleFiltres
            //
            this.grilleFiltres.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleFiltres.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleFiltres.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFiltreColonne,
            this.colFiltreOperateur,
            this.colFiltreValeur,
            this.colFiltreValeur2,
            this.colFiltreFormat,
            this.colFiltrePorte});
            this.grilleFiltres.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleFiltres.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grilleFiltres.Location = new System.Drawing.Point(0, 22);
            this.grilleFiltres.Name = "grilleFiltres";
            this.grilleFiltres.RowHeadersWidth = 24;
            this.grilleFiltres.Size = new System.Drawing.Size(976, 145);
            this.grilleFiltres.TabIndex = 1;
            this.grilleFiltres.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grilleFiltres_CellValueChanged);
            this.grilleFiltres.CurrentCellDirtyStateChanged += new System.EventHandler(this.Grille_CurrentCellDirtyStateChanged);
            this.grilleFiltres.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.Grille_DataError);
            this.grilleFiltres.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(this.grilleFiltres_RowsRemoved);
            //
            // colFiltreColonne
            //
            this.colFiltreColonne.FillWeight = 160F;
            this.colFiltreColonne.HeaderText = "Colonne";
            this.colFiltreColonne.Name = "colFiltreColonne";
            //
            // colFiltreOperateur
            //
            this.colFiltreOperateur.FillWeight = 90F;
            this.colFiltreOperateur.HeaderText = "Opérateur";
            this.colFiltreOperateur.Name = "colFiltreOperateur";
            //
            // colFiltreValeur
            //
            this.colFiltreValeur.FillWeight = 120F;
            this.colFiltreValeur.HeaderText = "Valeur";
            this.colFiltreValeur.Name = "colFiltreValeur";
            //
            // colFiltreValeur2
            //
            this.colFiltreValeur2.FillWeight = 100F;
            this.colFiltreValeur2.HeaderText = "Valeur 2 (entre)";
            this.colFiltreValeur2.Name = "colFiltreValeur2";
            //
            // colFiltreFormat
            //
            this.colFiltreFormat.FillWeight = 110F;
            this.colFiltreFormat.HeaderText = "Date Atlas stockée en";
            this.colFiltreFormat.Name = "colFiltreFormat";
            this.colFiltreFormat.ToolTipText = "Pour une colonne numérique contenant une date : format de stockage";
            //
            // ongletLigne
            //
            this.ongletLigne.Controls.Add(this.grilleValeurs);
            this.ongletLigne.Controls.Add(this.panneauModes);
            this.ongletLigne.Location = new System.Drawing.Point(4, 22);
            this.ongletLigne.Name = "ongletLigne";
            this.ongletLigne.Size = new System.Drawing.Size(976, 441);
            this.ongletLigne.TabIndex = 1;
            this.ongletLigne.Text = "Ajouter / modifier / supprimer une ligne";
            this.ongletLigne.UseVisualStyleBackColor = true;
            //
            // panneauModes
            //
            this.panneauModes.Controls.Add(this.etiquetteAideLigne);
            this.panneauModes.Controls.Add(this.boutonChargerLigne);
            this.panneauModes.Controls.Add(this.champRrn);
            this.panneauModes.Controls.Add(this.etiquetteRrn);
            this.panneauModes.Controls.Add(this.radioSupprimer);
            this.panneauModes.Controls.Add(this.radioModifier);
            this.panneauModes.Controls.Add(this.radioAjouter);
            this.panneauModes.Dock = System.Windows.Forms.DockStyle.Top;
            this.panneauModes.Location = new System.Drawing.Point(0, 0);
            this.panneauModes.Name = "panneauModes";
            this.panneauModes.Size = new System.Drawing.Size(976, 68);
            this.panneauModes.TabIndex = 0;
            //
            // radioAjouter
            //
            this.radioAjouter.AutoSize = true;
            this.radioAjouter.Location = new System.Drawing.Point(12, 9);
            this.radioAjouter.Name = "radioAjouter";
            this.radioAjouter.Size = new System.Drawing.Size(105, 17);
            this.radioAjouter.TabIndex = 0;
            this.radioAjouter.Text = "Ajouter une ligne";
            this.radioAjouter.UseVisualStyleBackColor = true;
            this.radioAjouter.CheckedChanged += new System.EventHandler(this.Mode_CheckedChanged);
            //
            // radioModifier
            //
            this.radioModifier.AutoSize = true;
            this.radioModifier.Checked = true;
            this.radioModifier.Location = new System.Drawing.Point(150, 9);
            this.radioModifier.Name = "radioModifier";
            this.radioModifier.Size = new System.Drawing.Size(110, 17);
            this.radioModifier.TabIndex = 1;
            this.radioModifier.TabStop = true;
            this.radioModifier.Text = "Modifier une ligne";
            this.radioModifier.UseVisualStyleBackColor = true;
            this.radioModifier.CheckedChanged += new System.EventHandler(this.Mode_CheckedChanged);
            //
            // radioSupprimer
            //
            this.radioSupprimer.AutoSize = true;
            this.radioSupprimer.Location = new System.Drawing.Point(292, 9);
            this.radioSupprimer.Name = "radioSupprimer";
            this.radioSupprimer.Size = new System.Drawing.Size(117, 17);
            this.radioSupprimer.TabIndex = 2;
            this.radioSupprimer.Text = "Supprimer une ligne";
            this.radioSupprimer.UseVisualStyleBackColor = true;
            this.radioSupprimer.CheckedChanged += new System.EventHandler(this.Mode_CheckedChanged);
            //
            // etiquetteRrn
            //
            this.etiquetteRrn.AutoSize = true;
            this.etiquetteRrn.Location = new System.Drawing.Point(12, 41);
            this.etiquetteRrn.Name = "etiquetteRrn";
            this.etiquetteRrn.Size = new System.Drawing.Size(150, 13);
            this.etiquetteRrn.TabIndex = 3;
            this.etiquetteRrn.Text = "ou n° d'enregistrement (RRN) :";
            //
            // champRrn
            //
            this.champRrn.Location = new System.Drawing.Point(170, 38);
            this.champRrn.Name = "champRrn";
            this.champRrn.Size = new System.Drawing.Size(100, 20);
            this.champRrn.TabIndex = 4;
            this.champRrn.TextChanged += new System.EventHandler(this.Saisie_Changee);
            //
            // boutonChargerLigne
            //
            this.boutonChargerLigne.Location = new System.Drawing.Point(280, 36);
            this.boutonChargerLigne.Name = "boutonChargerLigne";
            this.boutonChargerLigne.Size = new System.Drawing.Size(130, 24);
            this.boutonChargerLigne.TabIndex = 5;
            this.boutonChargerLigne.Text = "Charger la ligne";
            this.boutonChargerLigne.UseVisualStyleBackColor = true;
            this.boutonChargerLigne.Click += new System.EventHandler(this.boutonChargerLigne_Click);
            //
            // etiquetteAideLigne
            //
            this.etiquetteAideLigne.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteAideLigne.ForeColor = System.Drawing.SystemColors.GrayText;
            this.etiquetteAideLigne.Location = new System.Drawing.Point(420, 4);
            this.etiquetteAideLigne.Name = "etiquetteAideLigne";
            this.etiquetteAideLigne.Size = new System.Drawing.Size(550, 60);
            this.etiquetteAideLigne.TabIndex = 6;
            this.etiquetteAideLigne.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // grilleValeurs
            //
            this.grilleValeurs.AllowUserToAddRows = false;
            this.grilleValeurs.AllowUserToDeleteRows = false;
            this.grilleValeurs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grilleValeurs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grilleValeurs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVNom,
            this.colVLibelle,
            this.colVType,
            this.colVCle,
            this.colVActuelle,
            this.colVNouvelle,
            this.colVNull,
            this.colVFormat});
            this.grilleValeurs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grilleValeurs.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.grilleValeurs.Location = new System.Drawing.Point(0, 68);
            this.grilleValeurs.Name = "grilleValeurs";
            this.grilleValeurs.RowHeadersVisible = false;
            this.grilleValeurs.Size = new System.Drawing.Size(976, 373);
            this.grilleValeurs.TabIndex = 1;
            this.grilleValeurs.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.Grille_CellValueChanged);
            this.grilleValeurs.CurrentCellDirtyStateChanged += new System.EventHandler(this.Grille_CurrentCellDirtyStateChanged);
            this.grilleValeurs.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.Grille_DataError);
            //
            // colVNom
            //
            this.colVNom.FillWeight = 80F;
            this.colVNom.HeaderText = "Colonne";
            this.colVNom.Name = "colVNom";
            this.colVNom.ReadOnly = true;
            //
            // colVLibelle
            //
            this.colVLibelle.FillWeight = 170F;
            this.colVLibelle.HeaderText = "Libellé";
            this.colVLibelle.Name = "colVLibelle";
            this.colVLibelle.ReadOnly = true;
            //
            // colVType
            //
            this.colVType.FillWeight = 80F;
            this.colVType.HeaderText = "Type";
            this.colVType.Name = "colVType";
            this.colVType.ReadOnly = true;
            //
            // colVCle
            //
            this.colVCle.FillWeight = 30F;
            this.colVCle.HeaderText = "Clé";
            this.colVCle.Name = "colVCle";
            this.colVCle.ReadOnly = true;
            //
            // colVActuelle
            //
            this.colVActuelle.FillWeight = 120F;
            this.colVActuelle.HeaderText = "Valeur actuelle / clé";
            this.colVActuelle.Name = "colVActuelle";
            //
            // colVNouvelle
            //
            this.colVNouvelle.FillWeight = 120F;
            this.colVNouvelle.HeaderText = "Nouvelle valeur";
            this.colVNouvelle.Name = "colVNouvelle";
            //
            // colVNull
            //
            this.colVNull.FillWeight = 35F;
            this.colVNull.HeaderText = "NULL";
            this.colVNull.Name = "colVNull";
            //
            // colVFormat
            //
            this.colVFormat.FillWeight = 100F;
            this.colVFormat.HeaderText = "Date Atlas stockée en";
            this.colVFormat.Name = "colVFormat";
            //
            // panneauBas
            //
            this.panneauBas.Controls.Add(this.apercuSql);
            this.panneauBas.Controls.Add(this.etiquetteApercu);
            this.panneauBas.Controls.Add(this.panneauBoutons);
            this.panneauBas.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panneauBas.Location = new System.Drawing.Point(0, 531);
            this.panneauBas.Name = "panneauBas";
            this.panneauBas.Size = new System.Drawing.Size(984, 210);
            this.panneauBas.TabIndex = 2;
            //
            // etiquetteApercu
            //
            this.etiquetteApercu.Dock = System.Windows.Forms.DockStyle.Top;
            this.etiquetteApercu.Location = new System.Drawing.Point(0, 0);
            this.etiquetteApercu.Name = "etiquetteApercu";
            this.etiquetteApercu.Padding = new System.Windows.Forms.Padding(4, 4, 0, 0);
            this.etiquetteApercu.Size = new System.Drawing.Size(984, 20);
            this.etiquetteApercu.TabIndex = 0;
            this.etiquetteApercu.Text = "SQL généré (inséré dans l'éditeur, puis exécuté avec F5 et ses confirmations) :";
            //
            // apercuSql
            //
            this.apercuSql.BackColor = System.Drawing.SystemColors.Window;
            this.apercuSql.Dock = System.Windows.Forms.DockStyle.Fill;
            this.apercuSql.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.apercuSql.Location = new System.Drawing.Point(0, 20);
            this.apercuSql.Multiline = true;
            this.apercuSql.Name = "apercuSql";
            this.apercuSql.ReadOnly = true;
            this.apercuSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.apercuSql.Size = new System.Drawing.Size(984, 146);
            this.apercuSql.TabIndex = 1;
            this.apercuSql.WordWrap = false;
            //
            // panneauBoutons
            //
            this.panneauBoutons.Controls.Add(this.etiquetteErreurs);
            this.panneauBoutons.Controls.Add(this.boutonInserer);
            this.panneauBoutons.Controls.Add(this.boutonFermer);
            this.panneauBoutons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panneauBoutons.Location = new System.Drawing.Point(0, 166);
            this.panneauBoutons.Name = "panneauBoutons";
            this.panneauBoutons.Size = new System.Drawing.Size(984, 44);
            this.panneauBoutons.TabIndex = 2;
            //
            // etiquetteErreurs
            //
            this.etiquetteErreurs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.etiquetteErreurs.ForeColor = System.Drawing.Color.DarkRed;
            this.etiquetteErreurs.Location = new System.Drawing.Point(8, 4);
            this.etiquetteErreurs.Name = "etiquetteErreurs";
            this.etiquetteErreurs.Size = new System.Drawing.Size(696, 36);
            this.etiquetteErreurs.TabIndex = 0;
            this.etiquetteErreurs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // boutonInserer
            //
            this.boutonInserer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonInserer.Enabled = false;
            this.boutonInserer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold);
            this.boutonInserer.Location = new System.Drawing.Point(712, 9);
            this.boutonInserer.Name = "boutonInserer";
            this.boutonInserer.Size = new System.Drawing.Size(166, 28);
            this.boutonInserer.TabIndex = 1;
            this.boutonInserer.Text = "Insérer dans l'éditeur";
            this.boutonInserer.UseVisualStyleBackColor = true;
            this.boutonInserer.Click += new System.EventHandler(this.boutonInserer_Click);
            //
            // boutonFermer
            //
            this.boutonFermer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.boutonFermer.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.boutonFermer.Location = new System.Drawing.Point(884, 9);
            this.boutonFermer.Name = "boutonFermer";
            this.boutonFermer.Size = new System.Drawing.Size(90, 28);
            this.boutonFermer.TabIndex = 2;
            this.boutonFermer.Text = "Fermer";
            this.boutonFermer.UseVisualStyleBackColor = true;
            //
            // FormAssistantRequete
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.boutonFermer;
            this.ClientSize = new System.Drawing.Size(984, 741);
            this.Controls.Add(this.onglets);
            this.Controls.Add(this.panneauBas);
            this.Controls.Add(this.panneauTable);
            this.MinimumSize = new System.Drawing.Size(860, 620);
            this.Name = "FormAssistantRequete";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Assistant de requête";
            this.panneauTable.ResumeLayout(false);
            this.panneauTable.PerformLayout();
            this.onglets.ResumeLayout(false);
            this.ongletConsulter.ResumeLayout(false);
            this.separateurConsulter.Panel1.ResumeLayout(false);
            this.separateurConsulter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.separateurConsulter)).EndInit();
            this.separateurConsulter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleChoixColonnes)).EndInit();
            this.panneauTables.ResumeLayout(false);
            this.ongletLot.ResumeLayout(false);
            this.panneauLotModes.ResumeLayout(false);
            this.panneauLotModes.PerformLayout();
            this.panneauLotOutils.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleLot)).EndInit();
            this.panneauTables.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grilleTables)).EndInit();
            this.panneauOptions.ResumeLayout(false);
            this.panneauOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.champLimite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grilleFiltres)).EndInit();
            this.ongletLigne.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grilleValeurs)).EndInit();
            this.panneauModes.ResumeLayout(false);
            this.panneauModes.PerformLayout();
            this.panneauBas.ResumeLayout(false);
            this.panneauBas.PerformLayout();
            this.panneauBoutons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panneauTable;
        private System.Windows.Forms.Label etiquetteSchema;
        private System.Windows.Forms.ComboBox listeSchemas;
        private System.Windows.Forms.Label etiquetteTable;
        private System.Windows.Forms.ComboBox listeTables;
        private System.Windows.Forms.Button boutonChargerTable;
        private System.Windows.Forms.Button boutonJoindre;
        private System.Windows.Forms.Button boutonEtape;
        private System.Windows.Forms.Button boutonRetirerEtape;
        private System.Windows.Forms.Panel panneauTables;
        private System.Windows.Forms.Label etiquetteTables;
        private System.Windows.Forms.DataGridView grilleTables;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTAlias;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTJointure;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTConditions;
        private System.Windows.Forms.Button boutonModifierJointure;
        private System.Windows.Forms.Button boutonRetirerTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTableC;
        private System.Windows.Forms.Button boutonExistence;
        private System.Windows.Forms.TabPage ongletLot;
        private System.Windows.Forms.Panel panneauLotModes;
        private System.Windows.Forms.RadioButton radioLotSupprimer;
        private System.Windows.Forms.RadioButton radioLotModifier;
        private System.Windows.Forms.RadioButton radioLotCopier;
        private System.Windows.Forms.Label etiquetteAideLot;
        private System.Windows.Forms.Panel panneauLotOutils;
        private System.Windows.Forms.Button boutonSourceLot;
        private System.Windows.Forms.Button boutonCibleLot;
        private System.Windows.Forms.Label etiquetteSourceLot;
        private System.Windows.Forms.DataGridView grilleLot;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLNom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLLibelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLType;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLValeur;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colLNull;
        private System.Windows.Forms.DataGridViewComboBoxColumn colLFormat;
        private System.Windows.Forms.Label etiquetteInfoTable;
        private System.Windows.Forms.TabControl onglets;
        private System.Windows.Forms.TabPage ongletConsulter;
        private System.Windows.Forms.SplitContainer separateurConsulter;
        private System.Windows.Forms.DataGridView grilleChoixColonnes;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colAfficher;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLibelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCle;
        private System.Windows.Forms.DataGridViewComboBoxColumn colTri;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRangTri;
        private System.Windows.Forms.DataGridViewComboBoxColumn colCalcul;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFormatC;
        private System.Windows.Forms.CheckBox caseCompter;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFiltrePorte;
        private System.Windows.Forms.Panel panneauOptions;
        private System.Windows.Forms.Button boutonToutCocher;
        private System.Windows.Forms.Button boutonToutDecocher;
        private System.Windows.Forms.CheckBox caseDistinct;
        private System.Windows.Forms.CheckBox caseRrn;
        private System.Windows.Forms.Label etiquetteLimite;
        private System.Windows.Forms.NumericUpDown champLimite;
        private System.Windows.Forms.Label etiquetteAideLimite;
        private System.Windows.Forms.DataGridView grilleFiltres;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFiltreColonne;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFiltreOperateur;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFiltreValeur;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFiltreValeur2;
        private System.Windows.Forms.DataGridViewComboBoxColumn colFiltreFormat;
        private System.Windows.Forms.Label etiquetteFiltres;
        private System.Windows.Forms.TabPage ongletLigne;
        private System.Windows.Forms.DataGridView grilleValeurs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVNom;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVLibelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVCle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVActuelle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVNouvelle;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colVNull;
        private System.Windows.Forms.DataGridViewComboBoxColumn colVFormat;
        private System.Windows.Forms.Panel panneauModes;
        private System.Windows.Forms.RadioButton radioAjouter;
        private System.Windows.Forms.RadioButton radioModifier;
        private System.Windows.Forms.RadioButton radioSupprimer;
        private System.Windows.Forms.Label etiquetteRrn;
        private System.Windows.Forms.TextBox champRrn;
        private System.Windows.Forms.Button boutonChargerLigne;
        private System.Windows.Forms.Label etiquetteAideLigne;
        private System.Windows.Forms.Panel panneauBas;
        private System.Windows.Forms.TextBox apercuSql;
        private System.Windows.Forms.Label etiquetteApercu;
        private System.Windows.Forms.Panel panneauBoutons;
        private System.Windows.Forms.Label etiquetteErreurs;
        private System.Windows.Forms.Button boutonInserer;
        private System.Windows.Forms.Button boutonFermer;
    }
}

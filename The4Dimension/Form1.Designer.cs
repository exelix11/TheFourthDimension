namespace The4Dimension
{
    partial class Form1
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.pasteValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.copyPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyRotationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyScaleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ModImpZ_Rot = new System.Windows.Forms.NumericUpDown();
            this.ModImpY_Rot = new System.Windows.Forms.NumericUpDown();
            this.ModImpX_Rot = new System.Windows.Forms.NumericUpDown();
            this.ModImpZ_Scale = new System.Windows.Forms.NumericUpDown();
            this.ModImpY_Scale = new System.Windows.Forms.NumericUpDown();
            this.ModImpX_Scale = new System.Windows.Forms.NumericUpDown();
            this.ModImpZ = new System.Windows.Forms.NumericUpDown();
            this.ModImpY = new System.Windows.Forms.NumericUpDown();
            this.ModImpX = new System.Windows.Forms.NumericUpDown();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsBymlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bymlConverterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bymlXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.xmlBymlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.ObjectsListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.BtnAddObj = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hotkeysListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ_Rot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY_Rot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX_Rot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ_Scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY_Scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX_Scale)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.ContextMenuStrip = this.contextMenuStrip1;
            this.propertyGrid1.Location = new System.Drawing.Point(4, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(230, 228);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridChange);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteValueToolStripMenuItem,
            this.toolStripSeparator1,
            this.copyPositionToolStripMenuItem,
            this.copyRotationToolStripMenuItem,
            this.copyScaleToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(149, 98);
            // 
            // pasteValueToolStripMenuItem
            // 
            this.pasteValueToolStripMenuItem.Name = "pasteValueToolStripMenuItem";
            this.pasteValueToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.pasteValueToolStripMenuItem.Text = "Paste value";
            this.pasteValueToolStripMenuItem.Click += new System.EventHandler(this.pasteValueToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(145, 6);
            // 
            // copyPositionToolStripMenuItem
            // 
            this.copyPositionToolStripMenuItem.Name = "copyPositionToolStripMenuItem";
            this.copyPositionToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.copyPositionToolStripMenuItem.Text = "Copy position";
            this.copyPositionToolStripMenuItem.Click += new System.EventHandler(this.copyPositionToolStripMenuItem_Click);
            // 
            // copyRotationToolStripMenuItem
            // 
            this.copyRotationToolStripMenuItem.Name = "copyRotationToolStripMenuItem";
            this.copyRotationToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.copyRotationToolStripMenuItem.Text = "Copy rotation";
            this.copyRotationToolStripMenuItem.Click += new System.EventHandler(this.copyRotationToolStripMenuItem_Click);
            // 
            // copyScaleToolStripMenuItem
            // 
            this.copyScaleToolStripMenuItem.Name = "copyScaleToolStripMenuItem";
            this.copyScaleToolStripMenuItem.Size = new System.Drawing.Size(148, 22);
            this.copyScaleToolStripMenuItem.Text = "Copy scale";
            this.copyScaleToolStripMenuItem.Click += new System.EventHandler(this.copyScaleToolStripMenuItem_Click);
            // 
            // elementHost1
            // 
            this.elementHost1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.elementHost1.Location = new System.Drawing.Point(244, 36);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(583, 529);
            this.elementHost1.TabIndex = 3;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = null;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "LoadModelAt";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.groupBox1.Controls.Add(this.ModImpZ_Rot);
            this.groupBox1.Controls.Add(this.ModImpY_Rot);
            this.groupBox1.Controls.Add(this.ModImpX_Rot);
            this.groupBox1.Controls.Add(this.ModImpZ_Scale);
            this.groupBox1.Controls.Add(this.ModImpY_Scale);
            this.groupBox1.Controls.Add(this.ModImpX_Scale);
            this.groupBox1.Controls.Add(this.ModImpZ);
            this.groupBox1.Controls.Add(this.ModImpY);
            this.groupBox1.Controls.Add(this.ModImpX);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(244, 36);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(314, 69);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "DebugModelLoader";
            // 
            // ModImpZ_Rot
            // 
            this.ModImpZ_Rot.Location = new System.Drawing.Point(238, 49);
            this.ModImpZ_Rot.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpZ_Rot.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpZ_Rot.Name = "ModImpZ_Rot";
            this.ModImpZ_Rot.Size = new System.Drawing.Size(38, 20);
            this.ModImpZ_Rot.TabIndex = 13;
            // 
            // ModImpY_Rot
            // 
            this.ModImpY_Rot.Location = new System.Drawing.Point(194, 49);
            this.ModImpY_Rot.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpY_Rot.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpY_Rot.Name = "ModImpY_Rot";
            this.ModImpY_Rot.Size = new System.Drawing.Size(38, 20);
            this.ModImpY_Rot.TabIndex = 12;
            // 
            // ModImpX_Rot
            // 
            this.ModImpX_Rot.Location = new System.Drawing.Point(150, 49);
            this.ModImpX_Rot.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpX_Rot.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpX_Rot.Name = "ModImpX_Rot";
            this.ModImpX_Rot.Size = new System.Drawing.Size(38, 20);
            this.ModImpX_Rot.TabIndex = 11;
            // 
            // ModImpZ_Scale
            // 
            this.ModImpZ_Scale.Location = new System.Drawing.Point(94, 49);
            this.ModImpZ_Scale.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpZ_Scale.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpZ_Scale.Name = "ModImpZ_Scale";
            this.ModImpZ_Scale.Size = new System.Drawing.Size(38, 20);
            this.ModImpZ_Scale.TabIndex = 10;
            this.ModImpZ_Scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ModImpY_Scale
            // 
            this.ModImpY_Scale.Location = new System.Drawing.Point(50, 49);
            this.ModImpY_Scale.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpY_Scale.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpY_Scale.Name = "ModImpY_Scale";
            this.ModImpY_Scale.Size = new System.Drawing.Size(38, 20);
            this.ModImpY_Scale.TabIndex = 9;
            this.ModImpY_Scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ModImpX_Scale
            // 
            this.ModImpX_Scale.Location = new System.Drawing.Point(6, 49);
            this.ModImpX_Scale.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpX_Scale.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpX_Scale.Name = "ModImpX_Scale";
            this.ModImpX_Scale.Size = new System.Drawing.Size(38, 20);
            this.ModImpX_Scale.TabIndex = 8;
            this.ModImpX_Scale.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ModImpZ
            // 
            this.ModImpZ.Location = new System.Drawing.Point(211, 19);
            this.ModImpZ.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpZ.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpZ.Name = "ModImpZ";
            this.ModImpZ.Size = new System.Drawing.Size(38, 20);
            this.ModImpZ.TabIndex = 7;
            // 
            // ModImpY
            // 
            this.ModImpY.Location = new System.Drawing.Point(167, 19);
            this.ModImpY.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpY.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpY.Name = "ModImpY";
            this.ModImpY.Size = new System.Drawing.Size(38, 20);
            this.ModImpY.TabIndex = 6;
            // 
            // ModImpX
            // 
            this.ModImpX.Location = new System.Drawing.Point(123, 19);
            this.ModImpX.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ModImpX.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ModImpX.Name = "ModImpX";
            this.ModImpX.Size = new System.Drawing.Size(38, 20);
            this.ModImpX.TabIndex = 5;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.bymlConverterToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(832, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsBymlToolStripMenuItem,
            this.saveAsXmlToolStripMenuItem});
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(98, 22);
            this.saveAsToolStripMenuItem.Text = "Save";
            // 
            // saveAsBymlToolStripMenuItem
            // 
            this.saveAsBymlToolStripMenuItem.Name = "saveAsBymlToolStripMenuItem";
            this.saveAsBymlToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.saveAsBymlToolStripMenuItem.Text = "Save as Byml";
            this.saveAsBymlToolStripMenuItem.Click += new System.EventHandler(this.saveAsBymlToolStripMenuItem_Click);
            // 
            // saveAsXmlToolStripMenuItem
            // 
            this.saveAsXmlToolStripMenuItem.Name = "saveAsXmlToolStripMenuItem";
            this.saveAsXmlToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.saveAsXmlToolStripMenuItem.Text = "Save as Xml";
            this.saveAsXmlToolStripMenuItem.Click += new System.EventHandler(this.saveAsXmlToolStripMenuItem_Click);
            // 
            // bymlConverterToolStripMenuItem
            // 
            this.bymlConverterToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bymlXmlToolStripMenuItem,
            this.xmlBymlToolStripMenuItem});
            this.bymlConverterToolStripMenuItem.Name = "bymlConverterToolStripMenuItem";
            this.bymlConverterToolStripMenuItem.Size = new System.Drawing.Size(99, 20);
            this.bymlConverterToolStripMenuItem.Text = "Byml converter";
            // 
            // bymlXmlToolStripMenuItem
            // 
            this.bymlXmlToolStripMenuItem.Name = "bymlXmlToolStripMenuItem";
            this.bymlXmlToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.bymlXmlToolStripMenuItem.Text = "Byml -> Xml";
            this.bymlXmlToolStripMenuItem.Click += new System.EventHandler(this.bymlXmlToolStripMenuItem_Click);
            // 
            // xmlBymlToolStripMenuItem
            // 
            this.xmlBymlToolStripMenuItem.Name = "xmlBymlToolStripMenuItem";
            this.xmlBymlToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            this.xmlBymlToolStripMenuItem.Text = "Xml -> Byml";
            this.xmlBymlToolStripMenuItem.Click += new System.EventHandler(this.xmlBymlToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.splitContainer1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.splitContainer1.Location = new System.Drawing.Point(0, 36);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.ObjectsListBox);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox1);
            this.splitContainer1.Panel1.Controls.Add(this.BtnAddObj);
            this.splitContainer1.Panel1.Controls.Add(this.button3);
            this.splitContainer1.Panel1.Controls.Add(this.button2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel2.Controls.Add(this.button5);
            this.splitContainer1.Panel2.Controls.Add(this.button4);
            this.splitContainer1.Panel2.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Size = new System.Drawing.Size(244, 529);
            this.splitContainer1.SplitterDistance = 264;
            this.splitContainer1.TabIndex = 7;
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(7, 219);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(92, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "Hide this layer";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // ObjectsListBox
            // 
            this.ObjectsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ObjectsListBox.FormattingEnabled = true;
            this.ObjectsListBox.Location = new System.Drawing.Point(7, 30);
            this.ObjectsListBox.Name = "ObjectsListBox";
            this.ObjectsListBox.Size = new System.Drawing.Size(229, 186);
            this.ObjectsListBox.TabIndex = 8;
            this.ObjectsListBox.SelectedIndexChanged += new System.EventHandler(this.ObjectsListBox_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Object type:";
            // 
            // comboBox1
            // 
            this.comboBox1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.comboBox1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(73, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(160, 21);
            this.comboBox1.TabIndex = 6;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // BtnAddObj
            // 
            this.BtnAddObj.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnAddObj.Location = new System.Drawing.Point(3, 238);
            this.BtnAddObj.Name = "BtnAddObj";
            this.BtnAddObj.Size = new System.Drawing.Size(29, 23);
            this.BtnAddObj.TabIndex = 5;
            this.BtnAddObj.Text = "+";
            this.BtnAddObj.UseVisualStyleBackColor = true;
            this.BtnAddObj.Click += new System.EventHandler(this.BtnAddObj_Click);
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button3.Location = new System.Drawing.Point(137, 238);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(96, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Delete Object";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button2.Location = new System.Drawing.Point(35, 238);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(96, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Duplicate Object";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(163, 235);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 2;
            this.button5.Text = "-";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button4.Location = new System.Drawing.Point(3, 235);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 1;
            this.button4.Text = "+";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.SystemColors.ControlDark;
            this.label2.Location = new System.Drawing.Point(793, 555);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Credits";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hotkeysListToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // hotkeysListToolStripMenuItem
            // 
            this.hotkeysListToolStripMenuItem.Name = "hotkeysListToolStripMenuItem";
            this.hotkeysListToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.hotkeysListToolStripMenuItem.Text = "Hotkeys list";
            this.hotkeysListToolStripMenuItem.Click += new System.EventHandler(this.hotkeysListToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 568);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "The Fourth Dimension - by Exelix11";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ_Rot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY_Rot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX_Rot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ_Scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY_Scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX_Scale)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpZ)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ModImpX)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown ModImpZ_Rot;
        private System.Windows.Forms.NumericUpDown ModImpY_Rot;
        private System.Windows.Forms.NumericUpDown ModImpX_Rot;
        private System.Windows.Forms.NumericUpDown ModImpZ_Scale;
        private System.Windows.Forms.NumericUpDown ModImpY_Scale;
        private System.Windows.Forms.NumericUpDown ModImpX_Scale;
        private System.Windows.Forms.NumericUpDown ModImpZ;
        private System.Windows.Forms.NumericUpDown ModImpY;
        private System.Windows.Forms.NumericUpDown ModImpX;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsBymlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsXmlToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button BtnAddObj;
        private System.Windows.Forms.ToolStripMenuItem bymlConverterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bymlXmlToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem xmlBymlToolStripMenuItem;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem pasteValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem copyPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyRotationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyScaleToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ListBox ObjectsListBox;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hotkeysListToolStripMenuItem;
    }
}


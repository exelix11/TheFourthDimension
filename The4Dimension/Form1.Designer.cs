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
            this.ClipBoardMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ClipBoardMenu_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ClipBoardMenu_CopyPos = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyRot = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyScale = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyArgs = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyFull = new System.Windows.Forms.ToolStripMenuItem();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsSZSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAsBymlToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsXmlToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.bymlConverterToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.bymlXmlToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.xmlBymlToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.UndoMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OtherLevelDataMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectByIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectByCameraIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectBySwitchToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchAToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchAppearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchKillToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchDeadOnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectByViewIdToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hotkeysListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gbatempThreadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.AddTypeBtn = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.ObjectsListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.BtnAddObj = new System.Windows.Forms.Button();
            this.DelObjBtn = new System.Windows.Forms.Button();
            this.DeleteMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteEveryObjectInTheListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DuplicateObjBtn = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.ClipBoardMenu.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.DeleteMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertyGrid1.ContextMenuStrip = this.ClipBoardMenu;
            this.propertyGrid1.Location = new System.Drawing.Point(4, 3);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(230, 228);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGridChange);
            // 
            // ClipBoardMenu
            // 
            this.ClipBoardMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClipBoardMenu_Paste,
            this.toolStripSeparator1,
            this.ClipBoardMenu_CopyPos,
            this.ClipBoardMenu_CopyRot,
            this.ClipBoardMenu_CopyScale,
            this.ClipBoardMenu_CopyArgs,
            this.ClipBoardMenu_CopyFull});
            this.ClipBoardMenu.Name = "contextMenuStrip1";
            this.ClipBoardMenu.Size = new System.Drawing.Size(159, 142);
            this.ClipBoardMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ClipBoardMenu_Opening);
            // 
            // ClipBoardMenu_Paste
            // 
            this.ClipBoardMenu_Paste.DoubleClickEnabled = true;
            this.ClipBoardMenu_Paste.Name = "ClipBoardMenu_Paste";
            this.ClipBoardMenu_Paste.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_Paste.Text = "Paste value";
            this.ClipBoardMenu_Paste.DoubleClick += new System.EventHandler(this.pasteValueToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(155, 6);
            // 
            // ClipBoardMenu_CopyPos
            // 
            this.ClipBoardMenu_CopyPos.Name = "ClipBoardMenu_CopyPos";
            this.ClipBoardMenu_CopyPos.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_CopyPos.Text = "Copy position";
            this.ClipBoardMenu_CopyPos.Click += new System.EventHandler(this.copyPositionToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyRot
            // 
            this.ClipBoardMenu_CopyRot.Name = "ClipBoardMenu_CopyRot";
            this.ClipBoardMenu_CopyRot.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_CopyRot.Text = "Copy rotation";
            this.ClipBoardMenu_CopyRot.Click += new System.EventHandler(this.copyRotationToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyScale
            // 
            this.ClipBoardMenu_CopyScale.Name = "ClipBoardMenu_CopyScale";
            this.ClipBoardMenu_CopyScale.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_CopyScale.Text = "Copy scale";
            this.ClipBoardMenu_CopyScale.Click += new System.EventHandler(this.copyScaleToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyArgs
            // 
            this.ClipBoardMenu_CopyArgs.Name = "ClipBoardMenu_CopyArgs";
            this.ClipBoardMenu_CopyArgs.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_CopyArgs.Text = "Copy args";
            this.ClipBoardMenu_CopyArgs.Click += new System.EventHandler(this.ClipBoardMenu_CopyArgs_Click);
            // 
            // ClipBoardMenu_CopyFull
            // 
            this.ClipBoardMenu_CopyFull.Name = "ClipBoardMenu_CopyFull";
            this.ClipBoardMenu_CopyFull.Size = new System.Drawing.Size(158, 22);
            this.ClipBoardMenu_CopyFull.Text = "Copy full object";
            this.ClipBoardMenu_CopyFull.Click += new System.EventHandler(this.ClipBoardMenu_CopyFull_Click);
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
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.UndoMenu,
            this.OtherLevelDataMenu,
            this.findToolStripMenuItem,
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
            this.newToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator2,
            this.bymlConverterToolStripMenuItem1});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAsSZSToolStripMenuItem,
            this.toolStripSeparator3,
            this.saveAsBymlToolStripMenuItem1,
            this.saveAsXmlToolStripMenuItem});
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.saveAsToolStripMenuItem.Text = "Save";
            // 
            // saveAsSZSToolStripMenuItem
            // 
            this.saveAsSZSToolStripMenuItem.Name = "saveAsSZSToolStripMenuItem";
            this.saveAsSZSToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.saveAsSZSToolStripMenuItem.Text = "Save as Szs";
            this.saveAsSZSToolStripMenuItem.Click += new System.EventHandler(this.saveAsBymlToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(139, 6);
            // 
            // saveAsBymlToolStripMenuItem1
            // 
            this.saveAsBymlToolStripMenuItem1.Name = "saveAsBymlToolStripMenuItem1";
            this.saveAsBymlToolStripMenuItem1.Size = new System.Drawing.Size(142, 22);
            this.saveAsBymlToolStripMenuItem1.Text = "Save as Byml";
            this.saveAsBymlToolStripMenuItem1.Click += new System.EventHandler(this.saveAsBymlToolStripMenuItem1_Click);
            // 
            // saveAsXmlToolStripMenuItem
            // 
            this.saveAsXmlToolStripMenuItem.Name = "saveAsXmlToolStripMenuItem";
            this.saveAsXmlToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.saveAsXmlToolStripMenuItem.Text = "Save as Xml";
            this.saveAsXmlToolStripMenuItem.Click += new System.EventHandler(this.saveAsXmlToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(151, 6);
            // 
            // bymlConverterToolStripMenuItem1
            // 
            this.bymlConverterToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bymlXmlToolStripMenuItem1,
            this.xmlBymlToolStripMenuItem1});
            this.bymlConverterToolStripMenuItem1.Name = "bymlConverterToolStripMenuItem1";
            this.bymlConverterToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.bymlConverterToolStripMenuItem1.Text = "Byml converter";
            // 
            // bymlXmlToolStripMenuItem1
            // 
            this.bymlXmlToolStripMenuItem1.Name = "bymlXmlToolStripMenuItem1";
            this.bymlXmlToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
            this.bymlXmlToolStripMenuItem1.Text = "Byml -> Xml";
            this.bymlXmlToolStripMenuItem1.Click += new System.EventHandler(this.bymlXmlToolStripMenuItem_Click);
            // 
            // xmlBymlToolStripMenuItem1
            // 
            this.xmlBymlToolStripMenuItem1.Name = "xmlBymlToolStripMenuItem1";
            this.xmlBymlToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
            this.xmlBymlToolStripMenuItem1.Text = "Xml -> Byml";
            this.xmlBymlToolStripMenuItem1.Click += new System.EventHandler(this.xmlBymlToolStripMenuItem_Click);
            // 
            // UndoMenu
            // 
            this.UndoMenu.Name = "UndoMenu";
            this.UndoMenu.Size = new System.Drawing.Size(48, 20);
            this.UndoMenu.Text = "Undo";
            this.UndoMenu.DropDownOpening += new System.EventHandler(this.Undo_loading);
            // 
            // OtherLevelDataMenu
            // 
            this.OtherLevelDataMenu.Name = "OtherLevelDataMenu";
            this.OtherLevelDataMenu.Size = new System.Drawing.Size(70, 20);
            this.OtherLevelDataMenu.Text = "Level files";
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.objectByIdToolStripMenuItem,
            this.objectByCameraIdToolStripMenuItem,
            this.objectBySwitchToolStripMenuItem,
            this.objectByViewIdToolStripMenuItem});
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(42, 20);
            this.findToolStripMenuItem.Text = "Find";
            // 
            // objectByIdToolStripMenuItem
            // 
            this.objectByIdToolStripMenuItem.Name = "objectByIdToolStripMenuItem";
            this.objectByIdToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.objectByIdToolStripMenuItem.Text = "Object by Id";
            this.objectByIdToolStripMenuItem.Click += new System.EventHandler(this.objectByIdToolStripMenuItem_Click);
            // 
            // objectByCameraIdToolStripMenuItem
            // 
            this.objectByCameraIdToolStripMenuItem.Name = "objectByCameraIdToolStripMenuItem";
            this.objectByCameraIdToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.objectByCameraIdToolStripMenuItem.Text = "Object by CameraId";
            this.objectByCameraIdToolStripMenuItem.Click += new System.EventHandler(this.objectByCameraIdToolStripMenuItem_Click);
            // 
            // objectBySwitchToolStripMenuItem
            // 
            this.objectBySwitchToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.switchAToolStripMenuItem,
            this.switchBToolStripMenuItem,
            this.switchAppearToolStripMenuItem,
            this.switchKillToolStripMenuItem,
            this.switchDeadOnToolStripMenuItem});
            this.objectBySwitchToolStripMenuItem.Name = "objectBySwitchToolStripMenuItem";
            this.objectBySwitchToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.objectBySwitchToolStripMenuItem.Text = "Object by switch";
            // 
            // switchAToolStripMenuItem
            // 
            this.switchAToolStripMenuItem.Name = "switchAToolStripMenuItem";
            this.switchAToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.switchAToolStripMenuItem.Text = "SwitchA";
            this.switchAToolStripMenuItem.Click += new System.EventHandler(this.Switch___FindClick);
            // 
            // switchBToolStripMenuItem
            // 
            this.switchBToolStripMenuItem.Name = "switchBToolStripMenuItem";
            this.switchBToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.switchBToolStripMenuItem.Text = "SwitchB";
            this.switchBToolStripMenuItem.Click += new System.EventHandler(this.Switch___FindClick);
            // 
            // switchAppearToolStripMenuItem
            // 
            this.switchAppearToolStripMenuItem.Name = "switchAppearToolStripMenuItem";
            this.switchAppearToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.switchAppearToolStripMenuItem.Text = "SwitchAppear";
            this.switchAppearToolStripMenuItem.Click += new System.EventHandler(this.Switch___FindClick);
            // 
            // switchKillToolStripMenuItem
            // 
            this.switchKillToolStripMenuItem.Name = "switchKillToolStripMenuItem";
            this.switchKillToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.switchKillToolStripMenuItem.Text = "SwitchKill";
            this.switchKillToolStripMenuItem.Click += new System.EventHandler(this.Switch___FindClick);
            // 
            // switchDeadOnToolStripMenuItem
            // 
            this.switchDeadOnToolStripMenuItem.Name = "switchDeadOnToolStripMenuItem";
            this.switchDeadOnToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.switchDeadOnToolStripMenuItem.Text = "SwitchDeadOn";
            this.switchDeadOnToolStripMenuItem.Click += new System.EventHandler(this.Switch___FindClick);
            // 
            // objectByViewIdToolStripMenuItem
            // 
            this.objectByViewIdToolStripMenuItem.Name = "objectByViewIdToolStripMenuItem";
            this.objectByViewIdToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.objectByViewIdToolStripMenuItem.Text = "Object by ViewId";
            this.objectByViewIdToolStripMenuItem.Click += new System.EventHandler(this.objectByViewIdToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hotkeysListToolStripMenuItem,
            this.gbatempThreadToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // hotkeysListToolStripMenuItem
            // 
            this.hotkeysListToolStripMenuItem.Name = "hotkeysListToolStripMenuItem";
            this.hotkeysListToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.hotkeysListToolStripMenuItem.Text = "Hotkeys list";
            this.hotkeysListToolStripMenuItem.Click += new System.EventHandler(this.hotkeysListToolStripMenuItem_Click);
            // 
            // gbatempThreadToolStripMenuItem
            // 
            this.gbatempThreadToolStripMenuItem.Name = "gbatempThreadToolStripMenuItem";
            this.gbatempThreadToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.gbatempThreadToolStripMenuItem.Text = "Gbatemp thread";
            this.gbatempThreadToolStripMenuItem.Click += new System.EventHandler(this.gbatempThreadToolStripMenuItem_Click);
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
            this.splitContainer1.Panel1.Controls.Add(this.AddTypeBtn);
            this.splitContainer1.Panel1.Controls.Add(this.checkBox1);
            this.splitContainer1.Panel1.Controls.Add(this.ObjectsListBox);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.comboBox1);
            this.splitContainer1.Panel1.Controls.Add(this.BtnAddObj);
            this.splitContainer1.Panel1.Controls.Add(this.DelObjBtn);
            this.splitContainer1.Panel1.Controls.Add(this.DuplicateObjBtn);
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
            // AddTypeBtn
            // 
            this.AddTypeBtn.Location = new System.Drawing.Point(215, 3);
            this.AddTypeBtn.Name = "AddTypeBtn";
            this.AddTypeBtn.Size = new System.Drawing.Size(23, 21);
            this.AddTypeBtn.TabIndex = 10;
            this.AddTypeBtn.Text = "+";
            this.AddTypeBtn.UseVisualStyleBackColor = true;
            this.AddTypeBtn.Click += new System.EventHandler(this.button1_Click);
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
            this.ObjectsListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Listbox_keyDown);
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
            this.comboBox1.Size = new System.Drawing.Size(136, 21);
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
            // DelObjBtn
            // 
            this.DelObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DelObjBtn.ContextMenuStrip = this.DeleteMenuStrip;
            this.DelObjBtn.Location = new System.Drawing.Point(137, 238);
            this.DelObjBtn.Name = "DelObjBtn";
            this.DelObjBtn.Size = new System.Drawing.Size(96, 23);
            this.DelObjBtn.TabIndex = 4;
            this.DelObjBtn.Text = "Delete Object";
            this.DelObjBtn.UseVisualStyleBackColor = true;
            this.DelObjBtn.Click += new System.EventHandler(this.button3_Click);
            // 
            // DeleteMenuStrip
            // 
            this.DeleteMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteEveryObjectInTheListToolStripMenuItem});
            this.DeleteMenuStrip.Name = "DeleteMenuStrip";
            this.DeleteMenuStrip.Size = new System.Drawing.Size(226, 26);
            // 
            // deleteEveryObjectInTheListToolStripMenuItem
            // 
            this.deleteEveryObjectInTheListToolStripMenuItem.Name = "deleteEveryObjectInTheListToolStripMenuItem";
            this.deleteEveryObjectInTheListToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.deleteEveryObjectInTheListToolStripMenuItem.Text = "Delete every object in the list";
            this.deleteEveryObjectInTheListToolStripMenuItem.Click += new System.EventHandler(this.deleteEveryObjectInTheListToolStripMenuItem_Click);
            // 
            // DuplicateObjBtn
            // 
            this.DuplicateObjBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DuplicateObjBtn.Location = new System.Drawing.Point(35, 238);
            this.DuplicateObjBtn.Name = "DuplicateObjBtn";
            this.DuplicateObjBtn.Size = new System.Drawing.Size(96, 23);
            this.DuplicateObjBtn.TabIndex = 3;
            this.DuplicateObjBtn.Text = "Duplicate Object";
            this.DuplicateObjBtn.UseVisualStyleBackColor = true;
            this.DuplicateObjBtn.Click += new System.EventHandler(this.button2_Click);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(832, 568);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.elementHost1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "Form1";
            this.Text = "The Fourth Dimension - by Exelix11";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ClipBoardMenu.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.DeleteMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsSZSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsXmlToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button DelObjBtn;
        private System.Windows.Forms.Button DuplicateObjBtn;
        private System.Windows.Forms.Button BtnAddObj;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.ContextMenuStrip ClipBoardMenu;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_Paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyPos;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyRot;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyScale;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hotkeysListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyArgs;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyFull;
        private System.Windows.Forms.ToolStripMenuItem UndoMenu;
        public System.Windows.Forms.ComboBox comboBox1;
        public System.Windows.Forms.ListBox ObjectsListBox;
        private System.Windows.Forms.ToolStripMenuItem OtherLevelDataMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem bymlConverterToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem bymlXmlToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem xmlBymlToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem gbatempThreadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectByIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectByCameraIdToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectBySwitchToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchAToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchBToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchAppearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchKillToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchDeadOnToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectByViewIdToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip DeleteMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem deleteEveryObjectInTheListToolStripMenuItem;
        private System.Windows.Forms.Button AddTypeBtn;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem saveAsBymlToolStripMenuItem1;
    }
}


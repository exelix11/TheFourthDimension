namespace The4Dimension
{
    partial class FrmObjEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.ClipBoardMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ClipBoardMenu_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ClipBoardMenu_CopyPos = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyRot = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyScale = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu_CopyArgs = new System.Windows.Forms.ToolStripMenuItem();
            this.ClipBoardMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.ContextMenuStrip = this.ClipBoardMenu;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 6);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(271, 278);
            this.propertyGrid1.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(3, 290);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(99, 27);
            this.button3.TabIndex = 1;
            this.button3.Text = "Add value";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(175, 290);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(99, 27);
            this.button4.TabIndex = 2;
            this.button4.Text = "Remove";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // ClipBoardMenu
            // 
            this.ClipBoardMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClipBoardMenu_Paste,
            this.toolStripSeparator1,
            this.ClipBoardMenu_CopyPos,
            this.ClipBoardMenu_CopyRot,
            this.ClipBoardMenu_CopyScale,
            this.ClipBoardMenu_CopyArgs});
            this.ClipBoardMenu.Name = "contextMenuStrip1";
            this.ClipBoardMenu.Size = new System.Drawing.Size(153, 142);
            this.ClipBoardMenu.Opening += new System.ComponentModel.CancelEventHandler(this.ClipBoardMenu_opening);
            // 
            // ClipBoardMenu_Paste
            // 
            this.ClipBoardMenu_Paste.DoubleClickEnabled = true;
            this.ClipBoardMenu_Paste.Name = "ClipBoardMenu_Paste";
            this.ClipBoardMenu_Paste.Size = new System.Drawing.Size(148, 22);
            this.ClipBoardMenu_Paste.Text = "Paste value";
            this.ClipBoardMenu_Paste.DoubleClick += new System.EventHandler(this.pasteValueToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // ClipBoardMenu_CopyPos
            // 
            this.ClipBoardMenu_CopyPos.Name = "ClipBoardMenu_CopyPos";
            this.ClipBoardMenu_CopyPos.Size = new System.Drawing.Size(148, 22);
            this.ClipBoardMenu_CopyPos.Text = "Copy position";
            this.ClipBoardMenu_CopyPos.Click += new System.EventHandler(this.copyPositionToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyRot
            // 
            this.ClipBoardMenu_CopyRot.Name = "ClipBoardMenu_CopyRot";
            this.ClipBoardMenu_CopyRot.Size = new System.Drawing.Size(148, 22);
            this.ClipBoardMenu_CopyRot.Text = "Copy rotation";
            this.ClipBoardMenu_CopyRot.Click += new System.EventHandler(this.copyRotationToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyScale
            // 
            this.ClipBoardMenu_CopyScale.Name = "ClipBoardMenu_CopyScale";
            this.ClipBoardMenu_CopyScale.Size = new System.Drawing.Size(148, 22);
            this.ClipBoardMenu_CopyScale.Text = "Copy scale";
            this.ClipBoardMenu_CopyScale.Click += new System.EventHandler(this.copyScaleToolStripMenuItem_Click);
            // 
            // ClipBoardMenu_CopyArgs
            // 
            this.ClipBoardMenu_CopyArgs.Name = "ClipBoardMenu_CopyArgs";
            this.ClipBoardMenu_CopyArgs.Size = new System.Drawing.Size(152, 22);
            this.ClipBoardMenu_CopyArgs.Text = "Copy args";
            this.ClipBoardMenu_CopyArgs.Click += new System.EventHandler(this.ClipBoardMenu_CopyArgs_Click);
            // 
            // FrmObjEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 319);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.propertyGrid1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmObjEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmObjEditor";
            this.Load += new System.EventHandler(this.FrmObjEditor_Load);
            this.ClipBoardMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ContextMenuStrip ClipBoardMenu;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_Paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyPos;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyRot;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyScale;
        private System.Windows.Forms.ToolStripMenuItem ClipBoardMenu_CopyArgs;
    }
}
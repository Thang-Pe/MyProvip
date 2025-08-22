namespace MyProvip
{
    partial class btnBranches
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
            this.imageListBlocks = new System.Windows.Forms.ImageList(this.components);
            this.listViewBranches = new System.Windows.Forms.ListView();
            this.listViewElementOfBranch = new System.Windows.Forms.ListView();
            this.btnLoadBranches = new System.Windows.Forms.Button();
            this.btnTest1 = new System.Windows.Forms.Button();
            this.listViewPipelines = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.drawingPanel = new System.Windows.Forms.Panel();
            this.btnDrawLine = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lvCad = new System.Windows.Forms.ListView();
            this.btnShowAll = new System.Windows.Forms.Button();
            this.cbbBlockDiag = new System.Windows.Forms.ComboBox();
            this.lbDiag = new System.Windows.Forms.ListBox();
            this.btnBrowserCad = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnSaveAsJson = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // imageListBlocks
            // 
            this.imageListBlocks.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListBlocks.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListBlocks.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // listViewBranches
            // 
            this.listViewBranches.HideSelection = false;
            this.listViewBranches.Location = new System.Drawing.Point(149, 81);
            this.listViewBranches.Name = "listViewBranches";
            this.listViewBranches.Size = new System.Drawing.Size(87, 693);
            this.listViewBranches.TabIndex = 9;
            this.listViewBranches.UseCompatibleStateImageBehavior = false;
            this.listViewBranches.SelectedIndexChanged += new System.EventHandler(this.listViewBranches_SelectedIndexChanged);
            // 
            // listViewElementOfBranch
            // 
            this.listViewElementOfBranch.AllowDrop = true;
            this.listViewElementOfBranch.HideSelection = false;
            this.listViewElementOfBranch.Location = new System.Drawing.Point(242, 81);
            this.listViewElementOfBranch.Name = "listViewElementOfBranch";
            this.listViewElementOfBranch.Size = new System.Drawing.Size(253, 693);
            this.listViewElementOfBranch.TabIndex = 10;
            this.listViewElementOfBranch.UseCompatibleStateImageBehavior = false;
            this.listViewElementOfBranch.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listViewElementOfBranch_ItemDrag);
            this.listViewElementOfBranch.SelectedIndexChanged += new System.EventHandler(this.listViewElementOfBranch_SelectedIndexChanged);
            this.listViewElementOfBranch.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewElementOfBranch_DragDrop);
            this.listViewElementOfBranch.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewElementOfBranch_DragEnter);
            this.listViewElementOfBranch.DragOver += new System.Windows.Forms.DragEventHandler(this.listViewElementOfBranch_DragOver);
            // 
            // btnLoadBranches
            // 
            this.btnLoadBranches.Location = new System.Drawing.Point(127, 12);
            this.btnLoadBranches.Name = "btnLoadBranches";
            this.btnLoadBranches.Size = new System.Drawing.Size(95, 30);
            this.btnLoadBranches.TabIndex = 12;
            this.btnLoadBranches.Text = "Load Branches";
            this.btnLoadBranches.UseVisualStyleBackColor = true;
            this.btnLoadBranches.Click += new System.EventHandler(this.btnLoadBranches_Click);
            // 
            // btnTest1
            // 
            this.btnTest1.Location = new System.Drawing.Point(1220, 15);
            this.btnTest1.Name = "btnTest1";
            this.btnTest1.Size = new System.Drawing.Size(95, 30);
            this.btnTest1.TabIndex = 13;
            this.btnTest1.Text = "Export to XML";
            this.btnTest1.UseVisualStyleBackColor = true;
            this.btnTest1.Click += new System.EventHandler(this.btnTest1_Click);
            // 
            // listViewPipelines
            // 
            this.listViewPipelines.HideSelection = false;
            this.listViewPipelines.Location = new System.Drawing.Point(17, 80);
            this.listViewPipelines.Name = "listViewPipelines";
            this.listViewPipelines.Size = new System.Drawing.Size(126, 693);
            this.listViewPipelines.TabIndex = 15;
            this.listViewPipelines.UseCompatibleStateImageBehavior = false;
            this.listViewPipelines.SelectedIndexChanged += new System.EventHandler(this.listViewPipelines_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Pipe Lines";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(146, 65);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 19;
            this.label5.Text = "Branches";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(239, 64);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(57, 13);
            this.label6.TabIndex = 20;
            this.label6.Text = "Members";
            // 
            // drawingPanel
            // 
            this.drawingPanel.BackColor = System.Drawing.Color.White;
            this.drawingPanel.Location = new System.Drawing.Point(514, 79);
            this.drawingPanel.Name = "drawingPanel";
            this.drawingPanel.Size = new System.Drawing.Size(801, 694);
            this.drawingPanel.TabIndex = 21;
            this.drawingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.drawingPanel_Paint);
            this.drawingPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.drawingPanel_MouseClick);
            this.drawingPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.drawingPanel_MouseMove);
            this.drawingPanel.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.drawingPanel_MouseWheel);
            // 
            // btnDrawLine
            // 
            this.btnDrawLine.Location = new System.Drawing.Point(539, 13);
            this.btnDrawLine.Name = "btnDrawLine";
            this.btnDrawLine.Size = new System.Drawing.Size(95, 30);
            this.btnDrawLine.TabIndex = 24;
            this.btnDrawLine.Text = "Draw Line";
            this.btnDrawLine.UseVisualStyleBackColor = true;
            this.btnDrawLine.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(1428, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 13);
            this.label3.TabIndex = 32;
            this.label3.Text = "All Blocks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(1330, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 31;
            this.label2.Text = "Block Types";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(1428, 783);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Assign Block Type";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1333, 788);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(92, 32);
            this.button1.TabIndex = 29;
            this.button1.Text = "Process";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lvCad
            // 
            this.lvCad.HideSelection = false;
            this.lvCad.LargeImageList = this.imageListBlocks;
            this.lvCad.Location = new System.Drawing.Point(1431, 81);
            this.lvCad.Name = "lvCad";
            this.lvCad.Size = new System.Drawing.Size(244, 693);
            this.lvCad.TabIndex = 28;
            this.lvCad.UseCompatibleStateImageBehavior = false;
            this.lvCad.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lvCad_ItemSelectionChanged);
            // 
            // btnShowAll
            // 
            this.btnShowAll.Location = new System.Drawing.Point(1600, 56);
            this.btnShowAll.Name = "btnShowAll";
            this.btnShowAll.Size = new System.Drawing.Size(75, 23);
            this.btnShowAll.TabIndex = 27;
            this.btnShowAll.Text = "Show All";
            this.btnShowAll.UseVisualStyleBackColor = true;
            this.btnShowAll.Click += new System.EventHandler(this.btnShowAll_Click);
            // 
            // cbbBlockDiag
            // 
            this.cbbBlockDiag.FormattingEnabled = true;
            this.cbbBlockDiag.Location = new System.Drawing.Point(1431, 799);
            this.cbbBlockDiag.Name = "cbbBlockDiag";
            this.cbbBlockDiag.Size = new System.Drawing.Size(244, 21);
            this.cbbBlockDiag.TabIndex = 26;
            this.cbbBlockDiag.SelectedIndexChanged += new System.EventHandler(this.cbbBlockDiag_SelectedIndexChanged);
            // 
            // lbDiag
            // 
            this.lbDiag.FormattingEnabled = true;
            this.lbDiag.Location = new System.Drawing.Point(1333, 81);
            this.lbDiag.Name = "lbDiag";
            this.lbDiag.Size = new System.Drawing.Size(92, 693);
            this.lbDiag.TabIndex = 25;
            this.lbDiag.SelectedIndexChanged += new System.EventHandler(this.lbDiag_SelectedIndexChanged);
            // 
            // btnBrowserCad
            // 
            this.btnBrowserCad.Location = new System.Drawing.Point(17, 12);
            this.btnBrowserCad.Name = "btnBrowserCad";
            this.btnBrowserCad.Size = new System.Drawing.Size(87, 30);
            this.btnBrowserCad.TabIndex = 33;
            this.btnBrowserCad.Text = "Browser";
            this.btnBrowserCad.UseVisualStyleBackColor = true;
            this.btnBrowserCad.Click += new System.EventHandler(this.btnBrowserCad_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(350, 21);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(171, 21);
            this.comboBox1.TabIndex = 34;
            // 
            // btnSaveAsJson
            // 
            this.btnSaveAsJson.Location = new System.Drawing.Point(1109, 15);
            this.btnSaveAsJson.Name = "btnSaveAsJson";
            this.btnSaveAsJson.Size = new System.Drawing.Size(95, 30);
            this.btnSaveAsJson.TabIndex = 35;
            this.btnSaveAsJson.Text = "Save Work";
            this.btnSaveAsJson.UseVisualStyleBackColor = true;
            this.btnSaveAsJson.Click += new System.EventHandler(this.btnSaveAsJson_Click);
            // 
            // btnBranches
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1699, 835);
            this.Controls.Add(this.btnSaveAsJson);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.btnBrowserCad);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lvCad);
            this.Controls.Add(this.btnShowAll);
            this.Controls.Add(this.cbbBlockDiag);
            this.Controls.Add(this.lbDiag);
            this.Controls.Add(this.btnDrawLine);
            this.Controls.Add(this.drawingPanel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listViewPipelines);
            this.Controls.Add(this.btnTest1);
            this.Controls.Add(this.btnLoadBranches);
            this.Controls.Add(this.listViewElementOfBranch);
            this.Controls.Add(this.listViewBranches);
            this.Name = "btnBranches";
            this.Text = "Config Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ConfigBlock_FormClosed);
            this.Load += new System.EventHandler(this.ConfigBlock_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ImageList imageListBlocks;
        private System.Windows.Forms.ListView listViewBranches;
        private System.Windows.Forms.ListView listViewElementOfBranch;
        private System.Windows.Forms.Button btnLoadBranches;
        private System.Windows.Forms.Button btnTest1;
        private System.Windows.Forms.ListView listViewPipelines;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Panel drawingPanel;
        private System.Windows.Forms.Button btnDrawLine;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView lvCad;
        private System.Windows.Forms.Button btnShowAll;
        private System.Windows.Forms.ComboBox cbbBlockDiag;
        private System.Windows.Forms.ListBox lbDiag;
        private System.Windows.Forms.Button btnBrowserCad;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnSaveAsJson;
    }
}
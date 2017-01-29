using System.Drawing;

namespace ManagementApp
{
    partial class MainWindow
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle37 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle38 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle39 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle40 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.containerPictureBox = new System.Windows.Forms.PictureBox();
            this.clientNodeBtn = new System.Windows.Forms.Button();
            this.netNodeBtn = new System.Windows.Forms.Button();
            this.connectionBtn = new System.Windows.Forms.Button();
            this.consoleTextBox = new System.Windows.Forms.TextBox();
            this.domainBtn = new System.Windows.Forms.Button();
            this.deleteBtn = new System.Windows.Forms.Button();
            this.deleteListBox = new System.Windows.Forms.ListBox();
            this.cursorBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.resendInfoBtn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxConUp = new System.Windows.Forms.TextBox();
            this.textBoxConDown = new System.Windows.Forms.TextBox();
            this.commitConBtn = new System.Windows.Forms.Button();
            this.saveConfBtn = new System.Windows.Forms.Button();
            this.readConfBtn = new System.Windows.Forms.Button();
            this.subNetworkBtn = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.zapiszToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wczytajToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.operacjeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.węzełKlienckiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.węzełSieciowyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.domenaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.podsiećToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.połączenieToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.usuńElementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.kursorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.widokToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logiToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.opcjeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoAgregacjaPortówToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scenariuszeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scenariusz1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label3 = new System.Windows.Forms.Label();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.wyczyśćScenariuszToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.scenariusz2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scenariusz15ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.containerPictureBox)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerPictureBox
            // 
            this.containerPictureBox.Location = new System.Drawing.Point(0, 27);
            this.containerPictureBox.Name = "containerPictureBox";
            this.containerPictureBox.Size = new System.Drawing.Size(1054, 514);
            this.containerPictureBox.TabIndex = 0;
            this.containerPictureBox.TabStop = false;
            this.containerPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.containerPictureBox_Paint);
            this.containerPictureBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.containerPictureBox_MouseClick);
            this.containerPictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.containerPictureBox_MouseDown);
            this.containerPictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.containerPictureBox_MouseMove);
            this.containerPictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.containerPictureBox_MouseUp);
            // 
            // clientNodeBtn
            // 
            this.clientNodeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.clientNodeBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.clientNodeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.clientNodeBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.clientNodeBtn.Location = new System.Drawing.Point(614, 27);
            this.clientNodeBtn.Name = "clientNodeBtn";
            this.clientNodeBtn.Size = new System.Drawing.Size(140, 22);
            this.clientNodeBtn.TabIndex = 1;
            this.clientNodeBtn.Text = "Węzeł kliencki";
            this.clientNodeBtn.UseVisualStyleBackColor = false;
            this.clientNodeBtn.Visible = false;
            this.clientNodeBtn.Click += new System.EventHandler(this.clientNodeBtn_Click);
            // 
            // netNodeBtn
            // 
            this.netNodeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.netNodeBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.netNodeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.netNodeBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.netNodeBtn.Location = new System.Drawing.Point(614, 55);
            this.netNodeBtn.Name = "netNodeBtn";
            this.netNodeBtn.Size = new System.Drawing.Size(140, 22);
            this.netNodeBtn.TabIndex = 2;
            this.netNodeBtn.Text = "Węzeł sieciowy";
            this.netNodeBtn.UseVisualStyleBackColor = false;
            this.netNodeBtn.Visible = false;
            this.netNodeBtn.Click += new System.EventHandler(this.networkNodeBtn_Click);
            // 
            // connectionBtn
            // 
            this.connectionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.connectionBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.connectionBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.connectionBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.connectionBtn.Location = new System.Drawing.Point(614, 139);
            this.connectionBtn.Name = "connectionBtn";
            this.connectionBtn.Size = new System.Drawing.Size(140, 22);
            this.connectionBtn.TabIndex = 3;
            this.connectionBtn.Text = "Połączenie";
            this.connectionBtn.UseVisualStyleBackColor = false;
            this.connectionBtn.Visible = false;
            this.connectionBtn.Click += new System.EventHandler(this.connectionBtn_Click);
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.consoleTextBox.ForeColor = System.Drawing.Color.Gainsboro;
            this.consoleTextBox.Location = new System.Drawing.Point(0, 0);
            this.consoleTextBox.Multiline = true;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.Size = new System.Drawing.Size(278, 492);
            this.consoleTextBox.TabIndex = 4;
            // 
            // domainBtn
            // 
            this.domainBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.domainBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.domainBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.domainBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.domainBtn.Location = new System.Drawing.Point(614, 83);
            this.domainBtn.Name = "domainBtn";
            this.domainBtn.Size = new System.Drawing.Size(140, 22);
            this.domainBtn.TabIndex = 5;
            this.domainBtn.Text = "Domena";
            this.domainBtn.UseVisualStyleBackColor = false;
            this.domainBtn.Visible = false;
            this.domainBtn.Click += new System.EventHandler(this.domainBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.deleteBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.deleteBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.deleteBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.deleteBtn.Location = new System.Drawing.Point(614, 481);
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.Size = new System.Drawing.Size(140, 22);
            this.deleteBtn.TabIndex = 6;
            this.deleteBtn.Text = "Usuń element";
            this.deleteBtn.UseVisualStyleBackColor = false;
            this.deleteBtn.Visible = false;
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            // 
            // deleteListBox
            // 
            this.deleteListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.deleteListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.deleteListBox.Enabled = false;
            this.deleteListBox.ForeColor = System.Drawing.Color.Gainsboro;
            this.deleteListBox.FormattingEnabled = true;
            this.deleteListBox.Location = new System.Drawing.Point(614, 380);
            this.deleteListBox.Name = "deleteListBox";
            this.deleteListBox.Size = new System.Drawing.Size(140, 67);
            this.deleteListBox.TabIndex = 7;
            this.deleteListBox.Visible = false;
            this.deleteListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.deleteListBox_MouseDoubleClick);
            // 
            // cursorBtn
            // 
            this.cursorBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.cursorBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.cursorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cursorBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.cursorBtn.Location = new System.Drawing.Point(614, 509);
            this.cursorBtn.Name = "cursorBtn";
            this.cursorBtn.Size = new System.Drawing.Size(140, 23);
            this.cursorBtn.TabIndex = 8;
            this.cursorBtn.Text = "Kursor";
            this.cursorBtn.UseVisualStyleBackColor = false;
            this.cursorBtn.Visible = false;
            this.cursorBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(768, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(286, 514);
            this.tabControl1.TabIndex = 9;
            this.tabControl1.Visible = false;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Maroon;
            this.tabPage1.Controls.Add(this.consoleTextBox);
            this.tabPage1.ForeColor = System.Drawing.Color.Gainsboro;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(278, 488);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dataGridView1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(278, 488);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Nodes";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridViewCellStyle37.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle37.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle37.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            dataGridViewCellStyle37.ForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle37.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle37.SelectionForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle37.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle37;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle38.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle38.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle38.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle38.ForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle38.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle38.SelectionForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle38.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle38;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle39.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle39.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            dataGridViewCellStyle39.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle39.ForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle39.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle39.SelectionForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle39.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle39;
            this.dataGridView1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(63)))), ((int)(((byte)(70)))));
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle40.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle40.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            dataGridViewCellStyle40.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            dataGridViewCellStyle40.ForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle40.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dataGridViewCellStyle40.SelectionForeColor = System.Drawing.Color.Gainsboro;
            dataGridViewCellStyle40.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.RowHeadersDefaultCellStyle = dataGridViewCellStyle40;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(278, 494);
            this.dataGridView1.TabIndex = 0;
            // 
            // resendInfoBtn
            // 
            this.resendInfoBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.resendInfoBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.resendInfoBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resendInfoBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.resendInfoBtn.Location = new System.Drawing.Point(614, 453);
            this.resendInfoBtn.Name = "resendInfoBtn";
            this.resendInfoBtn.Size = new System.Drawing.Size(140, 22);
            this.resendInfoBtn.TabIndex = 10;
            this.resendInfoBtn.Text = "Resend Info";
            this.resendInfoBtn.UseVisualStyleBackColor = false;
            this.resendInfoBtn.Visible = false;
            this.resendInfoBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Gainsboro;
            this.label1.Location = new System.Drawing.Point(12, 455);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "CC0 port:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Gainsboro;
            this.label2.Location = new System.Drawing.Point(12, 485);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "CC0 port:";
            // 
            // textBoxConUp
            // 
            this.textBoxConUp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.textBoxConUp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxConUp.ForeColor = System.Drawing.Color.Gainsboro;
            this.textBoxConUp.Location = new System.Drawing.Point(85, 453);
            this.textBoxConUp.Name = "textBoxConUp";
            this.textBoxConUp.Size = new System.Drawing.Size(67, 20);
            this.textBoxConUp.TabIndex = 13;
            // 
            // textBoxConDown
            // 
            this.textBoxConDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.textBoxConDown.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxConDown.ForeColor = System.Drawing.Color.Gainsboro;
            this.textBoxConDown.Location = new System.Drawing.Point(85, 483);
            this.textBoxConDown.Name = "textBoxConDown";
            this.textBoxConDown.Size = new System.Drawing.Size(67, 20);
            this.textBoxConDown.TabIndex = 14;
            // 
            // commitConBtn
            // 
            this.commitConBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.commitConBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.commitConBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.commitConBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.commitConBtn.Location = new System.Drawing.Point(12, 509);
            this.commitConBtn.Name = "commitConBtn";
            this.commitConBtn.Size = new System.Drawing.Size(140, 22);
            this.commitConBtn.TabIndex = 15;
            this.commitConBtn.Text = "Commit";
            this.commitConBtn.UseVisualStyleBackColor = false;
            this.commitConBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // saveConfBtn
            // 
            this.saveConfBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.saveConfBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.saveConfBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveConfBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.saveConfBtn.Location = new System.Drawing.Point(614, 167);
            this.saveConfBtn.Name = "saveConfBtn";
            this.saveConfBtn.Size = new System.Drawing.Size(67, 22);
            this.saveConfBtn.TabIndex = 17;
            this.saveConfBtn.Text = "Zapisz";
            this.saveConfBtn.UseVisualStyleBackColor = false;
            this.saveConfBtn.Visible = false;
            this.saveConfBtn.Click += new System.EventHandler(this.saveConfBtn_Click);
            // 
            // readConfBtn
            // 
            this.readConfBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.readConfBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.readConfBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.readConfBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.readConfBtn.Location = new System.Drawing.Point(687, 167);
            this.readConfBtn.Name = "readConfBtn";
            this.readConfBtn.Size = new System.Drawing.Size(67, 22);
            this.readConfBtn.TabIndex = 18;
            this.readConfBtn.Text = "Wczytaj";
            this.readConfBtn.UseVisualStyleBackColor = false;
            this.readConfBtn.Visible = false;
            this.readConfBtn.Click += new System.EventHandler(this.readConfBtn_Click);
            // 
            // subNetworkBtn
            // 
            this.subNetworkBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.subNetworkBtn.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(67)))), ((int)(((byte)(67)))), ((int)(((byte)(70)))));
            this.subNetworkBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.subNetworkBtn.ForeColor = System.Drawing.Color.Gainsboro;
            this.subNetworkBtn.Location = new System.Drawing.Point(614, 111);
            this.subNetworkBtn.Name = "subNetworkBtn";
            this.subNetworkBtn.Size = new System.Drawing.Size(140, 22);
            this.subNetworkBtn.TabIndex = 19;
            this.subNetworkBtn.Text = "Podsieć";
            this.subNetworkBtn.UseVisualStyleBackColor = false;
            this.subNetworkBtn.Visible = false;
            this.subNetworkBtn.Click += new System.EventHandler(this.subNetworkBtn_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.operacjeToolStripMenuItem,
            this.widokToolStripMenuItem,
            this.opcjeToolStripMenuItem,
            this.scenariuszeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1058, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.zapiszToolStripMenuItem,
            this.wczytajToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(38, 20);
            this.toolStripMenuItem1.Text = "Plik";
            // 
            // zapiszToolStripMenuItem
            // 
            this.zapiszToolStripMenuItem.Name = "zapiszToolStripMenuItem";
            this.zapiszToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.zapiszToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.zapiszToolStripMenuItem.Text = "Zapisz";
            this.zapiszToolStripMenuItem.Click += new System.EventHandler(this.zapiszToolStripMenuItem_Click);
            // 
            // wczytajToolStripMenuItem
            // 
            this.wczytajToolStripMenuItem.Name = "wczytajToolStripMenuItem";
            this.wczytajToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.wczytajToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.wczytajToolStripMenuItem.Text = "Wczytaj";
            this.wczytajToolStripMenuItem.Click += new System.EventHandler(this.wczytajToolStripMenuItem_Click);
            // 
            // operacjeToolStripMenuItem
            // 
            this.operacjeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.węzełKlienckiToolStripMenuItem,
            this.węzełSieciowyToolStripMenuItem,
            this.domenaToolStripMenuItem,
            this.podsiećToolStripMenuItem,
            this.połączenieToolStripMenuItem,
            this.toolStripSeparator1,
            this.usuńElementToolStripMenuItem,
            this.kursorToolStripMenuItem});
            this.operacjeToolStripMenuItem.Name = "operacjeToolStripMenuItem";
            this.operacjeToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.operacjeToolStripMenuItem.Text = "Operacje";
            // 
            // węzełKlienckiToolStripMenuItem
            // 
            this.węzełKlienckiToolStripMenuItem.Name = "węzełKlienckiToolStripMenuItem";
            this.węzełKlienckiToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.węzełKlienckiToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.węzełKlienckiToolStripMenuItem.Text = "Węzeł kliencki";
            this.węzełKlienckiToolStripMenuItem.Click += new System.EventHandler(this.węzełKlienckiToolStripMenuItem_Click);
            // 
            // węzełSieciowyToolStripMenuItem
            // 
            this.węzełSieciowyToolStripMenuItem.Name = "węzełSieciowyToolStripMenuItem";
            this.węzełSieciowyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.węzełSieciowyToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.węzełSieciowyToolStripMenuItem.Text = "Węzeł sieciowy";
            this.węzełSieciowyToolStripMenuItem.Click += new System.EventHandler(this.węzełSieciowyToolStripMenuItem_Click);
            // 
            // domenaToolStripMenuItem
            // 
            this.domenaToolStripMenuItem.Name = "domenaToolStripMenuItem";
            this.domenaToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.domenaToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.domenaToolStripMenuItem.Text = "Domena";
            this.domenaToolStripMenuItem.Click += new System.EventHandler(this.domenaToolStripMenuItem_Click);
            // 
            // podsiećToolStripMenuItem
            // 
            this.podsiećToolStripMenuItem.Name = "podsiećToolStripMenuItem";
            this.podsiećToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.podsiećToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.podsiećToolStripMenuItem.Text = "Podsieć";
            this.podsiećToolStripMenuItem.Click += new System.EventHandler(this.podsiećToolStripMenuItem_Click);
            // 
            // połączenieToolStripMenuItem
            // 
            this.połączenieToolStripMenuItem.Name = "połączenieToolStripMenuItem";
            this.połączenieToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.połączenieToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.połączenieToolStripMenuItem.Text = "Połączenie";
            this.połączenieToolStripMenuItem.Click += new System.EventHandler(this.połączenieToolStripMenuItem_Click);
            // 
            // usuńElementToolStripMenuItem
            // 
            this.usuńElementToolStripMenuItem.Name = "usuńElementToolStripMenuItem";
            this.usuńElementToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.usuńElementToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.usuńElementToolStripMenuItem.Text = "Usuń element";
            this.usuńElementToolStripMenuItem.Click += new System.EventHandler(this.usuńElementToolStripMenuItem_Click);
            // 
            // kursorToolStripMenuItem
            // 
            this.kursorToolStripMenuItem.Name = "kursorToolStripMenuItem";
            this.kursorToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.A)));
            this.kursorToolStripMenuItem.Size = new System.Drawing.Size(198, 22);
            this.kursorToolStripMenuItem.Text = "Kursor";
            this.kursorToolStripMenuItem.Click += new System.EventHandler(this.kursorToolStripMenuItem_Click);
            // 
            // widokToolStripMenuItem
            // 
            this.widokToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logiToolStripMenuItem});
            this.widokToolStripMenuItem.Name = "widokToolStripMenuItem";
            this.widokToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.widokToolStripMenuItem.Text = "Widok";
            // 
            // logiToolStripMenuItem
            // 
            this.logiToolStripMenuItem.Name = "logiToolStripMenuItem";
            this.logiToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.logiToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.logiToolStripMenuItem.Text = "Logi";
            this.logiToolStripMenuItem.Click += new System.EventHandler(this.logiToolStripMenuItem_Click);
            // 
            // opcjeToolStripMenuItem
            // 
            this.opcjeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoAgregacjaPortówToolStripMenuItem});
            this.opcjeToolStripMenuItem.Name = "opcjeToolStripMenuItem";
            this.opcjeToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.opcjeToolStripMenuItem.Text = "Opcje";
            // 
            // autoAgregacjaPortówToolStripMenuItem
            // 
            this.autoAgregacjaPortówToolStripMenuItem.Name = "autoAgregacjaPortówToolStripMenuItem";
            this.autoAgregacjaPortówToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.autoAgregacjaPortówToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
            this.autoAgregacjaPortówToolStripMenuItem.Text = "Auto agregacja portów";
            this.autoAgregacjaPortówToolStripMenuItem.Click += new System.EventHandler(this.autoAgregacjaPortówToolStripMenuItem_Click);
            // 
            // scenariuszeToolStripMenuItem
            // 
            this.scenariuszeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scenariusz1ToolStripMenuItem,
            this.scenariusz15ToolStripMenuItem,
            this.scenariusz2ToolStripMenuItem,
            this.toolStripSeparator2,
            this.wyczyśćScenariuszToolStripMenuItem});
            this.scenariuszeToolStripMenuItem.Name = "scenariuszeToolStripMenuItem";
            this.scenariuszeToolStripMenuItem.Size = new System.Drawing.Size(80, 20);
            this.scenariuszeToolStripMenuItem.Text = "Scenariusze";
            // 
            // scenariusz1ToolStripMenuItem
            // 
            this.scenariusz1ToolStripMenuItem.Name = "scenariusz1ToolStripMenuItem";
            this.scenariusz1ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.scenariusz1ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.scenariusz1ToolStripMenuItem.Text = "Scenariusz 1";
            this.scenariusz1ToolStripMenuItem.Click += new System.EventHandler(this.scenariusz1ToolStripMenuItem_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.ForeColor = System.Drawing.Color.Red;
            this.label3.Location = new System.Drawing.Point(0, 28);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 24);
            this.label3.TabIndex = 21;
            this.label3.Text = "Done!";
            this.label3.Visible = false;
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(150, 6);
            // 
            // wyczyśćScenariuszToolStripMenuItem
            // 
            this.wyczyśćScenariuszToolStripMenuItem.Name = "wyczyśćScenariuszToolStripMenuItem";
            this.wyczyśćScenariuszToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.wyczyśćScenariuszToolStripMenuItem.Text = "Wyczyść scenariusz";
            this.wyczyśćScenariuszToolStripMenuItem.Click += new System.EventHandler(this.wyczyśćScenariuszToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(173, 6);
            // 
            // scenariusz2ToolStripMenuItem
            // 
            this.scenariusz2ToolStripMenuItem.Name = "scenariusz2ToolStripMenuItem";
            this.scenariusz2ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F2)));
            this.scenariusz2ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.scenariusz2ToolStripMenuItem.Text = "Scenariusz 2";
            this.scenariusz2ToolStripMenuItem.Click += new System.EventHandler(this.scenariusz2ToolStripMenuItem_Click);
            // 
            // scenariusz15ToolStripMenuItem
            // 
            this.scenariusz15ToolStripMenuItem.Name = "scenariusz15ToolStripMenuItem";
            this.scenariusz15ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.scenariusz15ToolStripMenuItem.Text = "Scenariusz 1.5";
            this.scenariusz15ToolStripMenuItem.Click += new System.EventHandler(this.scenariusz15ToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.ClientSize = new System.Drawing.Size(1058, 544);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.subNetworkBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.readConfBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.saveConfBtn);
            this.Controls.Add(this.commitConBtn);
            this.Controls.Add(this.resendInfoBtn);
            this.Controls.Add(this.textBoxConUp);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.textBoxConDown);
            this.Controls.Add(this.cursorBtn);
            this.Controls.Add(this.deleteListBox);
            this.Controls.Add(this.deleteBtn);
            this.Controls.Add(this.domainBtn);
            this.Controls.Add(this.connectionBtn);
            this.Controls.Add(this.netNodeBtn);
            this.Controls.Add(this.clientNodeBtn);
            this.Controls.Add(this.containerPictureBox);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "Oversight Application";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.containerPictureBox)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox containerPictureBox;
        private System.Windows.Forms.Button clientNodeBtn;
        private System.Windows.Forms.Button netNodeBtn;
        private System.Windows.Forms.Button connectionBtn;
        private System.Windows.Forms.TextBox consoleTextBox;
        private System.Windows.Forms.Button domainBtn;
        private System.Windows.Forms.Button deleteBtn;
        private System.Windows.Forms.ListBox deleteListBox;
        private System.Windows.Forms.Button cursorBtn;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.Button resendInfoBtn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxConUp;
        private System.Windows.Forms.TextBox textBoxConDown;
        private System.Windows.Forms.Button commitConBtn;
        private System.Windows.Forms.Button saveConfBtn;
        private System.Windows.Forms.Button readConfBtn;
        private System.Windows.Forms.Button subNetworkBtn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem zapiszToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wczytajToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem operacjeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem węzełKlienckiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem węzełSieciowyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem domenaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem podsiećToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem połączenieToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem usuńElementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem kursorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem widokToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logiToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem opcjeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem autoAgregacjaPortówToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scenariuszeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scenariusz1ToolStripMenuItem;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem wyczyśćScenariuszToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scenariusz2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scenariusz15ToolStripMenuItem;
    }
}


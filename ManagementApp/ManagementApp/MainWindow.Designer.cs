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
            this.containerPictureBox = new System.Windows.Forms.PictureBox();
            this.clientNodeBtn = new System.Windows.Forms.Button();
            this.netNodeBtn = new System.Windows.Forms.Button();
            this.connectionBtn = new System.Windows.Forms.Button();
            this.consoleTextBox = new System.Windows.Forms.TextBox();
            this.domainBtn = new System.Windows.Forms.Button();
            this.deleteBtn = new System.Windows.Forms.Button();
            this.deleteListBox = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.containerPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // containerPictureBox
            // 
            this.containerPictureBox.Location = new System.Drawing.Point(10, 12);
            this.containerPictureBox.Name = "containerPictureBox";
            this.containerPictureBox.Size = new System.Drawing.Size(593, 520);
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
            this.clientNodeBtn.Location = new System.Drawing.Point(10, 545);
            this.clientNodeBtn.Name = "clientNodeBtn";
            this.clientNodeBtn.Size = new System.Drawing.Size(140, 22);
            this.clientNodeBtn.TabIndex = 1;
            this.clientNodeBtn.Text = "Węzeł kliencki";
            this.clientNodeBtn.UseVisualStyleBackColor = true;
            this.clientNodeBtn.Click += new System.EventHandler(this.clientNodeBtn_Click);
            // 
            // netNodeBtn
            // 
            this.netNodeBtn.Location = new System.Drawing.Point(156, 545);
            this.netNodeBtn.Name = "netNodeBtn";
            this.netNodeBtn.Size = new System.Drawing.Size(140, 22);
            this.netNodeBtn.TabIndex = 2;
            this.netNodeBtn.Text = "Węzeł sieciowy";
            this.netNodeBtn.UseVisualStyleBackColor = true;
            this.netNodeBtn.Click += new System.EventHandler(this.networkNodeBtn_Click);
            // 
            // connectionBtn
            // 
            this.connectionBtn.Location = new System.Drawing.Point(404, 545);
            this.connectionBtn.Name = "connectionBtn";
            this.connectionBtn.Size = new System.Drawing.Size(140, 22);
            this.connectionBtn.TabIndex = 3;
            this.connectionBtn.Text = "Połączenie";
            this.connectionBtn.UseVisualStyleBackColor = true;
            this.connectionBtn.Click += new System.EventHandler(this.connectionBtn_Click);
            // 
            // consoleTextBox
            // 
            this.consoleTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.consoleTextBox.Location = new System.Drawing.Point(609, 12);
            this.consoleTextBox.Multiline = true;
            this.consoleTextBox.Name = "consoleTextBox";
            this.consoleTextBox.ReadOnly = true;
            this.consoleTextBox.Size = new System.Drawing.Size(293, 520);
            this.consoleTextBox.TabIndex = 4;
            // 
            // domainBtn
            // 
            this.domainBtn.Location = new System.Drawing.Point(561, 545);
            this.domainBtn.Name = "domainBtn";
            this.domainBtn.Size = new System.Drawing.Size(140, 22);
            this.domainBtn.TabIndex = 5;
            this.domainBtn.Text = "Domena";
            this.domainBtn.UseVisualStyleBackColor = true;
            this.domainBtn.Click += new System.EventHandler(this.domainBtn_Click);
            // 
            // deleteBtn
            // 
            this.deleteBtn.Location = new System.Drawing.Point(740, 545);
            this.deleteBtn.Name = "deleteBtn";
            this.deleteBtn.Size = new System.Drawing.Size(98, 22);
            this.deleteBtn.TabIndex = 6;
            this.deleteBtn.Text = "Usuń element";
            this.deleteBtn.UseVisualStyleBackColor = true;
            this.deleteBtn.Click += new System.EventHandler(this.deleteBtn_Click);
            // 
            // deleteListBox
            // 
            this.deleteListBox.Enabled = false;
            this.deleteListBox.FormattingEnabled = true;
            this.deleteListBox.Location = new System.Drawing.Point(501, 450);
            this.deleteListBox.Name = "deleteListBox";
            this.deleteListBox.Size = new System.Drawing.Size(89, 69);
            this.deleteListBox.TabIndex = 7;
            this.deleteListBox.Visible = false;
            this.deleteListBox.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.deleteListBox_MouseDoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(303, 545);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(95, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Kursor";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 574);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.deleteListBox);
            this.Controls.Add(this.deleteBtn);
            this.Controls.Add(this.domainBtn);
            this.Controls.Add(this.consoleTextBox);
            this.Controls.Add(this.connectionBtn);
            this.Controls.Add(this.netNodeBtn);
            this.Controls.Add(this.clientNodeBtn);
            this.Controls.Add(this.containerPictureBox);
            this.Name = "MainWindow";
            this.Text = "ManagementApplication";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.containerPictureBox)).EndInit();
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
        private System.Windows.Forms.Button button1;
    }
}


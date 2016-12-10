namespace ClientNode
{
    partial class ClientWindow
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
            this.logPanel = new System.Windows.Forms.Panel();
            this.errorLogTextBox = new System.Windows.Forms.TextBox();
            this.errorLogLbl = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.logLbl = new System.Windows.Forms.Label();
            this.sendingGroupBox = new System.Windows.Forms.GroupBox();
            this.stopSendingBtn = new System.Windows.Forms.Button();
            this.sendingTextBox = new System.Windows.Forms.TextBox();
            this.sendComboBox = new System.Windows.Forms.ComboBox();
            this.sendPeriodicallyBtn = new System.Windows.Forms.Button();
            this.timeLbl = new System.Windows.Forms.Label();
            this.sendBtn = new System.Windows.Forms.Button();
            this.timeTextBox = new System.Windows.Forms.TextBox();
            this.logPanel.SuspendLayout();
            this.sendingGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // logPanel
            // 
            this.logPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.logPanel.Controls.Add(this.errorLogTextBox);
            this.logPanel.Controls.Add(this.errorLogLbl);
            this.logPanel.Controls.Add(this.logTextBox);
            this.logPanel.Controls.Add(this.logLbl);
            this.logPanel.ForeColor = System.Drawing.Color.Red;
            this.logPanel.Location = new System.Drawing.Point(582, 13);
            this.logPanel.Name = "logPanel";
            this.logPanel.Size = new System.Drawing.Size(335, 219);
            this.logPanel.TabIndex = 1;
            // 
            // errorLogTextBox
            // 
            this.errorLogTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.errorLogTextBox.ForeColor = System.Drawing.Color.Red;
            this.errorLogTextBox.Location = new System.Drawing.Point(20, 154);
            this.errorLogTextBox.Multiline = true;
            this.errorLogTextBox.Name = "errorLogTextBox";
            this.errorLogTextBox.Size = new System.Drawing.Size(301, 54);
            this.errorLogTextBox.TabIndex = 3;
            // 
            // errorLogLbl
            // 
            this.errorLogLbl.AutoSize = true;
            this.errorLogLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.errorLogLbl.Location = new System.Drawing.Point(13, 130);
            this.errorLogLbl.Name = "errorLogLbl";
            this.errorLogLbl.Size = new System.Drawing.Size(63, 17);
            this.errorLogLbl.TabIndex = 2;
            this.errorLogLbl.Text = "Error log";
            // 
            // logTextBox
            // 
            this.logTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.logTextBox.ForeColor = System.Drawing.Color.Red;
            this.logTextBox.Location = new System.Drawing.Point(20, 35);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.Size = new System.Drawing.Size(301, 85);
            this.logTextBox.TabIndex = 1;
            // 
            // logLbl
            // 
            this.logLbl.AutoSize = true;
            this.logLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.logLbl.Location = new System.Drawing.Point(13, 10);
            this.logLbl.Name = "logLbl";
            this.logLbl.Size = new System.Drawing.Size(32, 17);
            this.logLbl.TabIndex = 0;
            this.logLbl.Text = "Log";
            // 
            // sendingGroupBox
            // 
            this.sendingGroupBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.sendingGroupBox.Controls.Add(this.stopSendingBtn);
            this.sendingGroupBox.Controls.Add(this.sendingTextBox);
            this.sendingGroupBox.Controls.Add(this.sendComboBox);
            this.sendingGroupBox.Controls.Add(this.sendPeriodicallyBtn);
            this.sendingGroupBox.Controls.Add(this.timeLbl);
            this.sendingGroupBox.Controls.Add(this.sendBtn);
            this.sendingGroupBox.Controls.Add(this.timeTextBox);
            this.sendingGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.sendingGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.sendingGroupBox.ForeColor = System.Drawing.Color.Red;
            this.sendingGroupBox.Location = new System.Drawing.Point(17, 13);
            this.sendingGroupBox.Name = "sendingGroupBox";
            this.sendingGroupBox.Size = new System.Drawing.Size(559, 219);
            this.sendingGroupBox.TabIndex = 7;
            this.sendingGroupBox.TabStop = false;
            this.sendingGroupBox.Text = "SENDING";
            // 
            // stopSendingBtn
            // 
            this.stopSendingBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.stopSendingBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stopSendingBtn.Location = new System.Drawing.Point(303, 184);
            this.stopSendingBtn.Name = "stopSendingBtn";
            this.stopSendingBtn.Size = new System.Drawing.Size(125, 24);
            this.stopSendingBtn.TabIndex = 6;
            this.stopSendingBtn.Text = "Stop sending";
            this.stopSendingBtn.UseVisualStyleBackColor = false;
            this.stopSendingBtn.Click += new System.EventHandler(this.stopSendingBtn_Click);
            // 
            // sendingTextBox
            // 
            this.sendingTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sendingTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.sendingTextBox.ForeColor = System.Drawing.Color.Red;
            this.sendingTextBox.Location = new System.Drawing.Point(16, 35);
            this.sendingTextBox.Multiline = true;
            this.sendingTextBox.Name = "sendingTextBox";
            this.sendingTextBox.Size = new System.Drawing.Size(516, 83);
            this.sendingTextBox.TabIndex = 0;
            // 
            // sendComboBox
            // 
            this.sendComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.sendComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sendComboBox.ForeColor = System.Drawing.Color.Red;
            this.sendComboBox.FormattingEnabled = true;
            this.sendComboBox.Location = new System.Drawing.Point(18, 124);
            this.sendComboBox.Name = "sendComboBox";
            this.sendComboBox.Size = new System.Drawing.Size(125, 21);
            this.sendComboBox.TabIndex = 3;
            // 
            // sendPeriodicallyBtn
            // 
            this.sendPeriodicallyBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.sendPeriodicallyBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sendPeriodicallyBtn.Location = new System.Drawing.Point(18, 184);
            this.sendPeriodicallyBtn.Name = "sendPeriodicallyBtn";
            this.sendPeriodicallyBtn.Size = new System.Drawing.Size(125, 24);
            this.sendPeriodicallyBtn.TabIndex = 2;
            this.sendPeriodicallyBtn.Text = "Send periodically";
            this.sendPeriodicallyBtn.UseVisualStyleBackColor = false;
            this.sendPeriodicallyBtn.Click += new System.EventHandler(this.sendPeriodicallyBtn_Click);
            // 
            // timeLbl
            // 
            this.timeLbl.AutoSize = true;
            this.timeLbl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.timeLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.25F);
            this.timeLbl.Location = new System.Drawing.Point(161, 187);
            this.timeLbl.Name = "timeLbl";
            this.timeLbl.Size = new System.Drawing.Size(38, 17);
            this.timeLbl.TabIndex = 5;
            this.timeLbl.Text = "time:";
            // 
            // sendBtn
            // 
            this.sendBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.sendBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.sendBtn.Location = new System.Drawing.Point(18, 151);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(125, 24);
            this.sendBtn.TabIndex = 1;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = false;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // timeTextBox
            // 
            this.timeTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.timeTextBox.ForeColor = System.Drawing.Color.Red;
            this.timeTextBox.Location = new System.Drawing.Point(205, 184);
            this.timeTextBox.Multiline = true;
            this.timeTextBox.Name = "timeTextBox";
            this.timeTextBox.Size = new System.Drawing.Size(92, 24);
            this.timeTextBox.TabIndex = 4;
            // 
            // ClientWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(934, 242);
            this.Controls.Add(this.sendingGroupBox);
            this.Controls.Add(this.logPanel);
            this.Name = "ClientWindow";
            this.Text = "ClientWindow";
            this.logPanel.ResumeLayout(false);
            this.logPanel.PerformLayout();
            this.sendingGroupBox.ResumeLayout(false);
            this.sendingGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel logPanel;
        private System.Windows.Forms.Label logLbl;
        private System.Windows.Forms.Label errorLogLbl;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.TextBox errorLogTextBox;
        private System.Windows.Forms.GroupBox sendingGroupBox;
        private System.Windows.Forms.TextBox sendingTextBox;
        private System.Windows.Forms.ComboBox sendComboBox;
        private System.Windows.Forms.Button sendPeriodicallyBtn;
        private System.Windows.Forms.Label timeLbl;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.TextBox timeTextBox;
        private System.Windows.Forms.Button stopSendingBtn;
    }
}
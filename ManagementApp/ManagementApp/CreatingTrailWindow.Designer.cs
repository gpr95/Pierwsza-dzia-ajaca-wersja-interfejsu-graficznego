namespace ManagementApp
{
    partial class CreatingTrailWindow
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.startLbl = new System.Windows.Forms.Label();
            this.stopLbl = new System.Windows.Forms.Label();
            this.trailCreationGroupBox = new System.Windows.Forms.GroupBox();
            this.startComboBox = new System.Windows.Forms.ComboBox();
            this.stopComboBox = new System.Windows.Forms.ComboBox();
            this.calculateBtn = new System.Windows.Forms.Button();
            this.createBtn = new System.Windows.Forms.Button();
            this.connectionsDataGridView = new System.Windows.Forms.DataGridView();
            this.connectionName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.slot = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.trailCreationGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.connectionsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // startLbl
            // 
            this.startLbl.AutoSize = true;
            this.startLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.startLbl.Location = new System.Drawing.Point(13, 36);
            this.startLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.startLbl.Name = "startLbl";
            this.startLbl.Size = new System.Drawing.Size(38, 17);
            this.startLbl.TabIndex = 0;
            this.startLbl.Text = "Start";
            // 
            // stopLbl
            // 
            this.stopLbl.AutoSize = true;
            this.stopLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.stopLbl.Location = new System.Drawing.Point(13, 69);
            this.stopLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.stopLbl.Name = "stopLbl";
            this.stopLbl.Size = new System.Drawing.Size(37, 17);
            this.stopLbl.TabIndex = 1;
            this.stopLbl.Text = "Stop";
            // 
            // trailCreationGroupBox
            // 
            this.trailCreationGroupBox.Controls.Add(this.createBtn);
            this.trailCreationGroupBox.Controls.Add(this.calculateBtn);
            this.trailCreationGroupBox.Controls.Add(this.stopComboBox);
            this.trailCreationGroupBox.Controls.Add(this.startComboBox);
            this.trailCreationGroupBox.Controls.Add(this.stopLbl);
            this.trailCreationGroupBox.Controls.Add(this.startLbl);
            this.trailCreationGroupBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.trailCreationGroupBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.trailCreationGroupBox.Location = new System.Drawing.Point(13, 16);
            this.trailCreationGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trailCreationGroupBox.Name = "trailCreationGroupBox";
            this.trailCreationGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trailCreationGroupBox.Size = new System.Drawing.Size(209, 182);
            this.trailCreationGroupBox.TabIndex = 2;
            this.trailCreationGroupBox.TabStop = false;
            this.trailCreationGroupBox.Text = "TRAIL CREATION";
            // 
            // startComboBox
            // 
            this.startComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.startComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startComboBox.FormattingEnabled = true;
            this.startComboBox.Location = new System.Drawing.Point(80, 33);
            this.startComboBox.Name = "startComboBox";
            this.startComboBox.Size = new System.Drawing.Size(121, 24);
            this.startComboBox.TabIndex = 2;
            // 
            // stopComboBox
            // 
            this.stopComboBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.stopComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stopComboBox.FormattingEnabled = true;
            this.stopComboBox.Location = new System.Drawing.Point(80, 66);
            this.stopComboBox.Name = "stopComboBox";
            this.stopComboBox.Size = new System.Drawing.Size(121, 24);
            this.stopComboBox.TabIndex = 3;
            // 
            // calculateBtn
            // 
            this.calculateBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.calculateBtn.Location = new System.Drawing.Point(20, 104);
            this.calculateBtn.Name = "calculateBtn";
            this.calculateBtn.Size = new System.Drawing.Size(180, 29);
            this.calculateBtn.TabIndex = 4;
            this.calculateBtn.Text = "CALCULATE";
            this.calculateBtn.UseVisualStyleBackColor = true;
            this.calculateBtn.Click += new System.EventHandler(this.calculateBtn_Click);
            // 
            // createBtn
            // 
            this.createBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.createBtn.Location = new System.Drawing.Point(20, 139);
            this.createBtn.Name = "createBtn";
            this.createBtn.Size = new System.Drawing.Size(180, 29);
            this.createBtn.TabIndex = 5;
            this.createBtn.Text = "CREATE";
            this.createBtn.UseVisualStyleBackColor = true;
            this.createBtn.Click += new System.EventHandler(this.createBtn_Click);
            // 
            // connectionsDataGridView
            // 
            this.connectionsDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.connectionsDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.connectionsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.connectionsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.connectionName,
            this.slot});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.connectionsDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            this.connectionsDataGridView.Location = new System.Drawing.Point(13, 205);
            this.connectionsDataGridView.Name = "connectionsDataGridView";
            this.connectionsDataGridView.RowHeadersVisible = false;
            this.connectionsDataGridView.Size = new System.Drawing.Size(209, 190);
            this.connectionsDataGridView.TabIndex = 3;
            // 
            // connectionName
            // 
            this.connectionName.HeaderText = "Connection";
            this.connectionName.Name = "connectionName";
            // 
            // slot
            // 
            this.slot.HeaderText = "Slot";
            this.slot.Name = "slot";
            // 
            // CreatingTrailWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(55)))));
            this.ClientSize = new System.Drawing.Size(234, 421);
            this.Controls.Add(this.connectionsDataGridView);
            this.Controls.Add(this.trailCreationGroupBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "CreatingTrailWindow";
            this.Text = "CreatingTrailWindow";
            this.trailCreationGroupBox.ResumeLayout(false);
            this.trailCreationGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.connectionsDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label startLbl;
        private System.Windows.Forms.Label stopLbl;
        private System.Windows.Forms.GroupBox trailCreationGroupBox;
        private System.Windows.Forms.Button createBtn;
        private System.Windows.Forms.Button calculateBtn;
        private System.Windows.Forms.ComboBox stopComboBox;
        private System.Windows.Forms.ComboBox startComboBox;
        private System.Windows.Forms.DataGridView connectionsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn connectionName;
        private System.Windows.Forms.DataGridViewTextBoxColumn slot;
    }
}
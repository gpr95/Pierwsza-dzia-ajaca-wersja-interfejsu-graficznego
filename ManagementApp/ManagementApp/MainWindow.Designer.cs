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
            this.container = new System.Windows.Forms.PictureBox();
            this.clientNode = new System.Windows.Forms.Button();
            this.netNode = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.container)).BeginInit();
            this.SuspendLayout();
            // 
            // container
            // 
            this.container.Location = new System.Drawing.Point(10, 12);
            this.container.Name = "container";
            this.container.Size = new System.Drawing.Size(893, 520);
            this.container.TabIndex = 0;
            this.container.TabStop = false;
            this.container.Paint += new System.Windows.Forms.PaintEventHandler(this.container_Paint_1);
            this.container.MouseClick += new System.Windows.Forms.MouseEventHandler(this.container_MouseClick);
            // 
            // clientNode
            // 
            this.clientNode.Location = new System.Drawing.Point(10, 545);
            this.clientNode.Name = "clientNode";
            this.clientNode.Size = new System.Drawing.Size(140, 22);
            this.clientNode.TabIndex = 1;
            this.clientNode.Text = "Węzeł kliencki";
            this.clientNode.UseVisualStyleBackColor = true;
            this.clientNode.Click += new System.EventHandler(this.button1_Click);
            // 
            // netNode
            // 
            this.netNode.Location = new System.Drawing.Point(156, 545);
            this.netNode.Name = "netNode";
            this.netNode.Size = new System.Drawing.Size(140, 22);
            this.netNode.TabIndex = 2;
            this.netNode.Text = "Węzeł sieciowy";
            this.netNode.UseVisualStyleBackColor = true;
            this.netNode.Click += new System.EventHandler(this.button2_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 574);
            this.Controls.Add(this.netNode);
            this.Controls.Add(this.clientNode);
            this.Controls.Add(this.container);
            this.Name = "MainWindow";
            this.Text = "ManagementApplication";
            ((System.ComponentModel.ISupportInitialize)(this.container)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox container;
        private System.Windows.Forms.Button clientNode;
        private System.Windows.Forms.Button netNode;
    }
}


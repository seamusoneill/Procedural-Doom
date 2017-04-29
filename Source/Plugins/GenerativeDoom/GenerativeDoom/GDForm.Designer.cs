namespace GenerativeDoom
{
    partial class GDForm
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
            this.BtnClose = new System.Windows.Forms.Button();
            this.btnDoMagic = new System.Windows.Forms.Button();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.lbCategories = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // BtnClose
            // 
            this.BtnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnClose.Location = new System.Drawing.Point(100, 85);
            this.BtnClose.Margin = new System.Windows.Forms.Padding(2);
            this.BtnClose.Name = "BtnClose";
            this.BtnClose.Size = new System.Drawing.Size(93, 33);
            this.BtnClose.TabIndex = 0;
            this.BtnClose.Text = "Close";
            this.BtnClose.UseVisualStyleBackColor = true;
            this.BtnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnDoMagic
            // 
            this.btnDoMagic.Location = new System.Drawing.Point(37, 10);
            this.btnDoMagic.Margin = new System.Windows.Forms.Padding(1);
            this.btnDoMagic.Name = "btnDoMagic";
            this.btnDoMagic.Size = new System.Drawing.Size(115, 35);
            this.btnDoMagic.TabIndex = 1;
            this.btnDoMagic.Text = "Generate your Doom!";
            this.btnDoMagic.UseVisualStyleBackColor = true;
            this.btnDoMagic.Click += new System.EventHandler(this.btnDoMagic_Click);
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.Location = new System.Drawing.Point(10, 84);
            this.btnAnalysis.Margin = new System.Windows.Forms.Padding(1);
            this.btnAnalysis.Name = "btnAnalysis";
            this.btnAnalysis.Size = new System.Drawing.Size(81, 33);
            this.btnAnalysis.TabIndex = 2;
            this.btnAnalysis.Text = "Analysis";
            this.btnAnalysis.UseVisualStyleBackColor = true;
            this.btnAnalysis.Click += new System.EventHandler(this.btnAnalysis_Click);
            // 
            // lbCategories
            // 
            this.lbCategories.FormattingEnabled = true;
            this.lbCategories.ItemHeight = 14;
            this.lbCategories.Location = new System.Drawing.Point(10, 124);
            this.lbCategories.Margin = new System.Windows.Forms.Padding(1);
            this.lbCategories.Name = "lbCategories";
            this.lbCategories.Size = new System.Drawing.Size(178, 116);
            this.lbCategories.TabIndex = 4;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(37, 47);
            this.button1.Margin = new System.Windows.Forms.Padding(1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(115, 35);
            this.button1.TabIndex = 5;
            this.button1.Text = "Civiilization";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // GDForm
            // 
            this.AcceptButton = this.BtnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.BtnClose;
            this.ClientSize = new System.Drawing.Size(204, 250);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lbCategories);
            this.Controls.Add(this.btnAnalysis);
            this.Controls.Add(this.btnDoMagic);
            this.Controls.Add(this.BtnClose);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial Narrow", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GDForm";
            this.Text = "Generative Doom";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GDForm_FormClosing);
            this.Load += new System.EventHandler(this.GDForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BtnClose;
        private System.Windows.Forms.Button btnDoMagic;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.ListBox lbCategories;
        private System.Windows.Forms.Button button1;
    }
}
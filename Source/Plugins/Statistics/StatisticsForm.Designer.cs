namespace CodeImp.DoomBuilder.Statistics
{
	partial class StatisticsForm
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
			if(disposing && (components != null))
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.verticescount = new System.Windows.Forms.Label();
            this.linedefscount = new System.Windows.Forms.Label();
            this.sidedefscount = new System.Windows.Forms.Label();
            this.sectorscount = new System.Windows.Forms.Label();
            this.thingscount = new System.Windows.Forms.Label();
            this.closebutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(41, 31);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vertices:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(40, 59);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Linedefs:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(39, 86);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Sidedefs:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 114);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Sectors:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(52, 141);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Things:";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // verticescount
            // 
            this.verticescount.AutoSize = true;
            this.verticescount.Location = new System.Drawing.Point(126, 31);
            this.verticescount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.verticescount.Name = "verticescount";
            this.verticescount.Size = new System.Drawing.Size(64, 16);
            this.verticescount.TabIndex = 5;
            this.verticescount.Text = "0000000";
            // 
            // linedefscount
            // 
            this.linedefscount.AutoSize = true;
            this.linedefscount.Location = new System.Drawing.Point(126, 59);
            this.linedefscount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linedefscount.Name = "linedefscount";
            this.linedefscount.Size = new System.Drawing.Size(64, 16);
            this.linedefscount.TabIndex = 6;
            this.linedefscount.Text = "0000000";
            // 
            // sidedefscount
            // 
            this.sidedefscount.AutoSize = true;
            this.sidedefscount.Location = new System.Drawing.Point(126, 86);
            this.sidedefscount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sidedefscount.Name = "sidedefscount";
            this.sidedefscount.Size = new System.Drawing.Size(64, 16);
            this.sidedefscount.TabIndex = 7;
            this.sidedefscount.Text = "0000000";
            // 
            // sectorscount
            // 
            this.sectorscount.AutoSize = true;
            this.sectorscount.Location = new System.Drawing.Point(126, 114);
            this.sectorscount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.sectorscount.Name = "sectorscount";
            this.sectorscount.Size = new System.Drawing.Size(64, 16);
            this.sectorscount.TabIndex = 8;
            this.sectorscount.Text = "0000000";
            // 
            // thingscount
            // 
            this.thingscount.AutoSize = true;
            this.thingscount.Location = new System.Drawing.Point(126, 141);
            this.thingscount.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.thingscount.Name = "thingscount";
            this.thingscount.Size = new System.Drawing.Size(64, 16);
            this.thingscount.TabIndex = 9;
            this.thingscount.Text = "0000000";
            // 
            // closebutton
            // 
            this.closebutton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closebutton.Location = new System.Drawing.Point(42, 208);
            this.closebutton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.closebutton.Name = "closebutton";
            this.closebutton.Size = new System.Drawing.Size(145, 32);
            this.closebutton.TabIndex = 10;
            this.closebutton.Text = "Close";
            this.closebutton.UseVisualStyleBackColor = true;
            this.closebutton.Click += new System.EventHandler(this.closebutton_Click);
            // 
            // StatisticsForm
            // 
            this.AcceptButton = this.closebutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.closebutton;
            this.ClientSize = new System.Drawing.Size(230, 261);
            this.Controls.Add(this.closebutton);
            this.Controls.Add(this.thingscount);
            this.Controls.Add(this.sectorscount);
            this.Controls.Add(this.sidedefscount);
            this.Controls.Add(this.linedefscount);
            this.Controls.Add(this.verticescount);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StatisticsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Statistics";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StatisticsForm_FormClosing);
            this.Load += new System.EventHandler(this.StatisticsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label verticescount;
		private System.Windows.Forms.Label linedefscount;
		private System.Windows.Forms.Label sidedefscount;
		private System.Windows.Forms.Label sectorscount;
		private System.Windows.Forms.Label thingscount;
		private System.Windows.Forms.Button closebutton;
	}
}
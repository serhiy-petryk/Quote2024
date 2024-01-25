
namespace Quote2024
{
    partial class MainForm
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLoader = new System.Windows.Forms.TabPage();
            this.lblEoddataLogged = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnTest = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpLoader.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 388);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(976, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(67, 17);
            this.StatusLabel.Text = "StatusLabel";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpLoader);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(976, 388);
            this.tabControl1.TabIndex = 1;
            // 
            // tpLoader
            // 
            this.tpLoader.Controls.Add(this.btnTest);
            this.tpLoader.Controls.Add(this.lblEoddataLogged);
            this.tpLoader.Location = new System.Drawing.Point(4, 24);
            this.tpLoader.Name = "tpLoader";
            this.tpLoader.Padding = new System.Windows.Forms.Padding(3);
            this.tpLoader.Size = new System.Drawing.Size(968, 360);
            this.tpLoader.TabIndex = 0;
            this.tpLoader.Text = "Loader";
            this.tpLoader.UseVisualStyleBackColor = true;
            // 
            // lblEoddataLogged
            // 
            this.lblEoddataLogged.AutoSize = true;
            this.lblEoddataLogged.BackColor = System.Drawing.Color.Red;
            this.lblEoddataLogged.ForeColor = System.Drawing.Color.White;
            this.lblEoddataLogged.Location = new System.Drawing.Point(160, 28);
            this.lblEoddataLogged.Name = "lblEoddataLogged";
            this.lblEoddataLogged.Size = new System.Drawing.Size(153, 15);
            this.lblEoddataLogged.TabIndex = 65;
            this.lblEoddataLogged.Text = "Not logged in eoddata.com";
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(968, 360);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(762, 265);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 66;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(976, 410);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpLoader.ResumeLayout(false);
            this.tpLoader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLoader;
        private System.Windows.Forms.Label lblEoddataLogged;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnTest;
    }
}
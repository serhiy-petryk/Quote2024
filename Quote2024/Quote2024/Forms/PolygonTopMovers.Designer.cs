
namespace Quote2024.Forms
{
    partial class PolygonTopMovers
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
            this.label1 = new System.Windows.Forms.Label();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTickCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 15);
            this.label1.TabIndex = 13;
            this.label1.Text = "Timer interval (seconds):";
            // 
            // numInterval
            // 
            this.numInterval.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numInterval.Location = new System.Drawing.Point(152, 61);
            this.numInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(57, 23);
            this.numInterval.TabIndex = 12;
            this.numInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 11;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(324, 97);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 10;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(324, 59);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 9;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(277, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "Label TickCount:";
            // 
            // lblTickCount
            // 
            this.lblTickCount.AutoSize = true;
            this.lblTickCount.Location = new System.Drawing.Point(379, 5);
            this.lblTickCount.Name = "lblTickCount";
            this.lblTickCount.Size = new System.Drawing.Size(75, 15);
            this.lblTickCount.TabIndex = 7;
            this.lblTickCount.Text = "lblTickCount";
            // 
            // PolygonTopMovers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numInterval);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblTickCount);
            this.Name = "PolygonTopMovers";
            this.Text = "PolygonTopMovers";
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblTickCount;
    }
}
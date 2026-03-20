
namespace Quote2024.Forms
{
    partial class PolygonMarketSnapshotForm
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
            this.lblTickCount = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTickCount
            // 
            this.lblTickCount.AutoSize = true;
            this.lblTickCount.Location = new System.Drawing.Point(379, 9);
            this.lblTickCount.Name = "lblTickCount";
            this.lblTickCount.Size = new System.Drawing.Size(75, 15);
            this.lblTickCount.TabIndex = 0;
            this.lblTickCount.Text = "lblTickCount";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(277, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Label TickCount:";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(324, 63);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(324, 101);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // numInterval
            // 
            this.numInterval.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numInterval.Location = new System.Drawing.Point(152, 65);
            this.numInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(57, 23);
            this.numInterval.TabIndex = 5;
            this.numInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Timer interval (seconds):";
            // 
            // PolygonMarketSnapshotForm
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
            this.Name = "PolygonMarketSnapshotForm";
            this.Text = "PolygonMarketSnapshotForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTickCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label label1;
    }
}
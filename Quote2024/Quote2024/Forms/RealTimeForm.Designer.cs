
namespace Quote2024.Forms
{
    partial class RealTimeForm
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
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblTickCount = new System.Windows.Forms.Label();
            this.btnUpdateTickerList = new System.Windows.Forms.Button();
            this.txtTickerList = new System.Windows.Forms.TextBox();
            this.lblTickerList = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 394);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(750, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(202, 12);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 33);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(283, 12);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 33);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lblTickCount
            // 
            this.lblTickCount.AutoSize = true;
            this.lblTickCount.Location = new System.Drawing.Point(364, 20);
            this.lblTickCount.Name = "lblTickCount";
            this.lblTickCount.Size = new System.Drawing.Size(38, 15);
            this.lblTickCount.TabIndex = 3;
            this.lblTickCount.Text = "label1";
            // 
            // btnUpdateTickerList
            // 
            this.btnUpdateTickerList.Location = new System.Drawing.Point(86, 12);
            this.btnUpdateTickerList.Name = "btnUpdateTickerList";
            this.btnUpdateTickerList.Size = new System.Drawing.Size(110, 33);
            this.btnUpdateTickerList.TabIndex = 6;
            this.btnUpdateTickerList.Text = "Update ticker list";
            this.btnUpdateTickerList.UseVisualStyleBackColor = true;
            this.btnUpdateTickerList.Click += new System.EventHandler(this.btnUpdateTickerList_Click);
            // 
            // txtTickerList
            // 
            this.txtTickerList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTickerList.Location = new System.Drawing.Point(3, 27);
            this.txtTickerList.Multiline = true;
            this.txtTickerList.Name = "txtTickerList";
            this.txtTickerList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTickerList.Size = new System.Drawing.Size(76, 364);
            this.txtTickerList.TabIndex = 7;
            this.txtTickerList.TextChanged += new System.EventHandler(this.txtTickerList_TextChanged);
            // 
            // lblTickerList
            // 
            this.lblTickerList.AutoSize = true;
            this.lblTickerList.Location = new System.Drawing.Point(3, 9);
            this.lblTickerList.Name = "lblTickerList";
            this.lblTickerList.Size = new System.Drawing.Size(47, 15);
            this.lblTickerList.TabIndex = 8;
            this.lblTickerList.Text = "Tickers:";
            // 
            // RealTimeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 416);
            this.Controls.Add(this.lblTickerList);
            this.Controls.Add(this.txtTickerList);
            this.Controls.Add(this.btnUpdateTickerList);
            this.Controls.Add(this.lblTickCount);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.statusStrip1);
            this.Name = "RealTimeForm";
            this.Text = "RealTimeForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblTickCount;
        private System.Windows.Forms.Button btnUpdateTickerList;
        private System.Windows.Forms.TextBox txtTickerList;
        private System.Windows.Forms.Label lblTickerList;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
    }
}
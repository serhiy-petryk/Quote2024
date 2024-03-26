
namespace Quote2024.Forms
{
    partial class TimeSalesNasdaqForm
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.btnUpdateList = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbIncludeIndices = new System.Windows.Forms.CheckBox();
            this.numMaxClose = new System.Windows.Forms.NumericUpDown();
            this.numMinClose = new System.Windows.Forms.NumericUpDown();
            this.numMinTradeCount = new System.Windows.Forms.NumericUpDown();
            this.numMaxTradeValue = new System.Windows.Forms.NumericUpDown();
            this.numMinTradeValue = new System.Windows.Forms.NumericUpDown();
            this.numPreviousDays = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblTickerList = new System.Windows.Forms.Label();
            this.txtTickerList = new System.Windows.Forms.TextBox();
            this.btnUpdateTickerList = new System.Windows.Forms.Button();
            this.lblTickCount = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxTradeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviousDays)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 386);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(806, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(118, 17);
            this.lblStatus.Text = "toolStripStatusLabel1";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer1.Panel1.Controls.Add(this.btnUpdateList);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.cbIncludeIndices);
            this.splitContainer1.Panel1.Controls.Add(this.numMaxClose);
            this.splitContainer1.Panel1.Controls.Add(this.numMinClose);
            this.splitContainer1.Panel1.Controls.Add(this.numMinTradeCount);
            this.splitContainer1.Panel1.Controls.Add(this.numMaxTradeValue);
            this.splitContainer1.Panel1.Controls.Add(this.numMinTradeValue);
            this.splitContainer1.Panel1.Controls.Add(this.numPreviousDays);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.label5);
            this.splitContainer1.Panel1.Controls.Add(this.label4);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.splitContainer1.Panel2.Controls.Add(this.lblTickerList);
            this.splitContainer1.Panel2.Controls.Add(this.txtTickerList);
            this.splitContainer1.Panel2.Controls.Add(this.btnUpdateTickerList);
            this.splitContainer1.Panel2.Controls.Add(this.lblTickCount);
            this.splitContainer1.Panel2.Controls.Add(this.btnStop);
            this.splitContainer1.Panel2.Controls.Add(this.btnStart);
            this.splitContainer1.Size = new System.Drawing.Size(806, 386);
            this.splitContainer1.SplitterDistance = 239;
            this.splitContainer1.TabIndex = 16;
            // 
            // btnUpdateList
            // 
            this.btnUpdateList.Location = new System.Drawing.Point(66, 257);
            this.btnUpdateList.Name = "btnUpdateList";
            this.btnUpdateList.Size = new System.Drawing.Size(96, 32);
            this.btnUpdateList.TabIndex = 33;
            this.btnUpdateList.Text = "Update list";
            this.btnUpdateList.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(3, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 43);
            this.label3.TabIndex = 32;
            this.label3.Text = "Maximum trade values (Close*Volume) in mln:";
            // 
            // cbIncludeIndices
            // 
            this.cbIncludeIndices.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbIncludeIndices.Checked = true;
            this.cbIncludeIndices.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeIndices.Location = new System.Drawing.Point(4, 216);
            this.cbIncludeIndices.Name = "cbIncludeIndices";
            this.cbIncludeIndices.Size = new System.Drawing.Size(222, 23);
            this.cbIncludeIndices.TabIndex = 31;
            this.cbIncludeIndices.Text = "Include indices (DJI, SP500):";
            this.cbIncludeIndices.UseVisualStyleBackColor = true;
            // 
            // numMaxClose
            // 
            this.numMaxClose.Location = new System.Drawing.Point(152, 187);
            this.numMaxClose.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMaxClose.Name = "numMaxClose";
            this.numMaxClose.Size = new System.Drawing.Size(74, 23);
            this.numMaxClose.TabIndex = 30;
            this.numMaxClose.ThousandsSeparator = true;
            this.numMaxClose.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // numMinClose
            // 
            this.numMinClose.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numMinClose.Location = new System.Drawing.Point(152, 151);
            this.numMinClose.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMinClose.Name = "numMinClose";
            this.numMinClose.Size = new System.Drawing.Size(74, 23);
            this.numMinClose.TabIndex = 29;
            this.numMinClose.ThousandsSeparator = true;
            this.numMinClose.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numMinTradeCount
            // 
            this.numMinTradeCount.Location = new System.Drawing.Point(152, 120);
            this.numMinTradeCount.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMinTradeCount.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numMinTradeCount.Name = "numMinTradeCount";
            this.numMinTradeCount.Size = new System.Drawing.Size(74, 23);
            this.numMinTradeCount.TabIndex = 28;
            this.numMinTradeCount.ThousandsSeparator = true;
            this.numMinTradeCount.Value = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            // 
            // numMaxTradeValue
            // 
            this.numMaxTradeValue.Location = new System.Drawing.Point(152, 82);
            this.numMaxTradeValue.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMaxTradeValue.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMaxTradeValue.Name = "numMaxTradeValue";
            this.numMaxTradeValue.Size = new System.Drawing.Size(74, 23);
            this.numMaxTradeValue.TabIndex = 27;
            this.numMaxTradeValue.ThousandsSeparator = true;
            this.numMaxTradeValue.Value = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            // 
            // numMinTradeValue
            // 
            this.numMinTradeValue.Location = new System.Drawing.Point(152, 36);
            this.numMinTradeValue.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMinTradeValue.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numMinTradeValue.Name = "numMinTradeValue";
            this.numMinTradeValue.Size = new System.Drawing.Size(46, 23);
            this.numMinTradeValue.TabIndex = 26;
            this.numMinTradeValue.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // numPreviousDays
            // 
            this.numPreviousDays.Location = new System.Drawing.Point(152, 7);
            this.numPreviousDays.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numPreviousDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numPreviousDays.Name = "numPreviousDays";
            this.numPreviousDays.Size = new System.Drawing.Size(46, 23);
            this.numPreviousDays.TabIndex = 25;
            this.numPreviousDays.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 187);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 15);
            this.label6.TabIndex = 24;
            this.label6.Text = "Maximum Close ($):";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 153);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 15);
            this.label5.TabIndex = 23;
            this.label5.Text = "Minimum Close ($):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 15);
            this.label4.TabIndex = 22;
            this.label4.Text = "Minimum trade count:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 43);
            this.label2.TabIndex = 21;
            this.label2.Text = "Minimum trade values (Close*Volume) in mln:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 15);
            this.label1.TabIndex = 20;
            this.label1.Text = "Number of previous days:";
            // 
            // lblTickerList
            // 
            this.lblTickerList.AutoSize = true;
            this.lblTickerList.Location = new System.Drawing.Point(6, 15);
            this.lblTickerList.Name = "lblTickerList";
            this.lblTickerList.Size = new System.Drawing.Size(47, 15);
            this.lblTickerList.TabIndex = 21;
            this.lblTickerList.Text = "Tickers:";
            // 
            // txtTickerList
            // 
            this.txtTickerList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtTickerList.Location = new System.Drawing.Point(6, 33);
            this.txtTickerList.Multiline = true;
            this.txtTickerList.Name = "txtTickerList";
            this.txtTickerList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTickerList.Size = new System.Drawing.Size(76, 334);
            this.txtTickerList.TabIndex = 20;
            // 
            // btnUpdateTickerList
            // 
            this.btnUpdateTickerList.Location = new System.Drawing.Point(89, 18);
            this.btnUpdateTickerList.Name = "btnUpdateTickerList";
            this.btnUpdateTickerList.Size = new System.Drawing.Size(110, 33);
            this.btnUpdateTickerList.TabIndex = 19;
            this.btnUpdateTickerList.Text = "Update ticker list";
            this.btnUpdateTickerList.UseVisualStyleBackColor = true;
            // 
            // lblTickCount
            // 
            this.lblTickCount.AutoSize = true;
            this.lblTickCount.Location = new System.Drawing.Point(367, 26);
            this.lblTickCount.Name = "lblTickCount";
            this.lblTickCount.Size = new System.Drawing.Size(38, 15);
            this.lblTickCount.TabIndex = 18;
            this.lblTickCount.Text = "label1";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(286, 18);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 33);
            this.btnStop.TabIndex = 17;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(205, 18);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 33);
            this.btnStart.TabIndex = 16;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // TimeSalesNasdaqForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 408);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "TimeSalesNasdaqForm";
            this.Text = "TimeSalesNasdaqForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviousDays)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblTickerList;
        private System.Windows.Forms.TextBox txtTickerList;
        private System.Windows.Forms.Button btnUpdateTickerList;
        private System.Windows.Forms.Label lblTickCount;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox cbIncludeIndices;
        private System.Windows.Forms.NumericUpDown numMaxClose;
        private System.Windows.Forms.NumericUpDown numMinClose;
        private System.Windows.Forms.NumericUpDown numMinTradeCount;
        private System.Windows.Forms.NumericUpDown numMaxTradeValue;
        private System.Windows.Forms.NumericUpDown numMinTradeValue;
        private System.Windows.Forms.NumericUpDown numPreviousDays;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnUpdateList;
    }
}
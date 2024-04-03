
namespace Quote2024.Forms
{
    partial class TradesFinageForm
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
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.txtTickerList = new System.Windows.Forms.TextBox();
            this.lblTickerList = new System.Windows.Forms.Label();
            this.btnDownloadForAllDay = new System.Windows.Forms.Button();
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
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 353);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(709, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(52, 17);
            this.lblStatus.Text = "lblStatus";
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
            this.splitContainer1.Panel1.Controls.Add(this.btnUpdateList);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
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
            this.splitContainer1.Panel1.Cursor = System.Windows.Forms.Cursors.Default;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(709, 353);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // btnUpdateList
            // 
            this.btnUpdateList.Location = new System.Drawing.Point(50, 242);
            this.btnUpdateList.Name = "btnUpdateList";
            this.btnUpdateList.Size = new System.Drawing.Size(96, 32);
            this.btnUpdateList.TabIndex = 33;
            this.btnUpdateList.Text = "Update list";
            this.btnUpdateList.UseVisualStyleBackColor = true;
            this.btnUpdateList.Click += new System.EventHandler(this.btnUpdateList_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(136, 33);
            this.label3.TabIndex = 32;
            this.label3.Text = "Maximum trade values (Close*Volume) in mln:";
            // 
            // numMaxClose
            // 
            this.numMaxClose.Location = new System.Drawing.Point(142, 197);
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
            this.numMinClose.Location = new System.Drawing.Point(143, 161);
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
            this.numMinTradeCount.Location = new System.Drawing.Point(142, 130);
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
            this.numMaxTradeValue.Location = new System.Drawing.Point(142, 94);
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
            this.numMinTradeValue.Location = new System.Drawing.Point(143, 46);
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
            this.numPreviousDays.Location = new System.Drawing.Point(143, 7);
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
            this.label6.Location = new System.Drawing.Point(0, 197);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(113, 15);
            this.label6.TabIndex = 24;
            this.label6.Text = "Maximum Close ($):";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 161);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 15);
            this.label5.TabIndex = 23;
            this.label5.Text = "Minimum Close ($):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(127, 15);
            this.label4.TabIndex = 22;
            this.label4.Text = "Minimum trade count:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(3, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 32);
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
            // splitContainer2
            // 
            this.splitContainer2.Cursor = System.Windows.Forms.Cursors.VSplit;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.txtTickerList);
            this.splitContainer2.Panel1.Controls.Add(this.lblTickerList);
            this.splitContainer2.Panel1.Cursor = System.Windows.Forms.Cursors.Default;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnDownloadForAllDay);
            this.splitContainer2.Panel2.Controls.Add(this.lblTickCount);
            this.splitContainer2.Panel2.Controls.Add(this.btnStop);
            this.splitContainer2.Panel2.Controls.Add(this.btnStart);
            this.splitContainer2.Panel2.Cursor = System.Windows.Forms.Cursors.Default;
            this.splitContainer2.Size = new System.Drawing.Size(480, 353);
            this.splitContainer2.SplitterDistance = 373;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // txtTickerList
            // 
            this.txtTickerList.AcceptsTab = true;
            this.txtTickerList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTickerList.Location = new System.Drawing.Point(3, 18);
            this.txtTickerList.Multiline = true;
            this.txtTickerList.Name = "txtTickerList";
            this.txtTickerList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTickerList.Size = new System.Drawing.Size(367, 332);
            this.txtTickerList.TabIndex = 1;
            this.txtTickerList.TabStop = false;
            this.txtTickerList.TextChanged += new System.EventHandler(this.txtTickerList_TextChanged);
            // 
            // lblTickerList
            // 
            this.lblTickerList.AutoSize = true;
            this.lblTickerList.Location = new System.Drawing.Point(3, 0);
            this.lblTickerList.Name = "lblTickerList";
            this.lblTickerList.Size = new System.Drawing.Size(180, 15);
            this.lblTickerList.TabIndex = 0;
            this.lblTickerList.Text = "Tickers (divided by space or tab):";
            // 
            // btnDownloadForAllDay
            // 
            this.btnDownloadForAllDay.Enabled = false;
            this.btnDownloadForAllDay.Location = new System.Drawing.Point(14, 197);
            this.btnDownloadForAllDay.Name = "btnDownloadForAllDay";
            this.btnDownloadForAllDay.Size = new System.Drawing.Size(75, 67);
            this.btnDownloadForAllDay.TabIndex = 3;
            this.btnDownloadForAllDay.Text = "Download data for all day";
            this.btnDownloadForAllDay.UseVisualStyleBackColor = true;
            this.btnDownloadForAllDay.Click += new System.EventHandler(this.btnDownloadForAllDay_Click);
            // 
            // lblTickCount
            // 
            this.lblTickCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTickCount.AutoSize = true;
            this.lblTickCount.Location = new System.Drawing.Point(-1, 9);
            this.lblTickCount.Name = "lblTickCount";
            this.lblTickCount.Size = new System.Drawing.Size(75, 15);
            this.lblTickCount.TabIndex = 2;
            this.lblTickCount.Text = "lblTickCount";
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(12, 84);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 42);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(12, 34);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 42);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // TradesFinageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(709, 375);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "TradesFinageForm";
            this.Text = "TradesFinage Form";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviousDays)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label3;
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
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txtTickerList;
        private System.Windows.Forms.Label lblTickerList;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblTickCount;
        private System.Windows.Forms.Button btnDownloadForAllDay;
    }
}
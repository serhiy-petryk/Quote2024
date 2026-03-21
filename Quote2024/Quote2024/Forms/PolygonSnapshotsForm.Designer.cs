
namespace Quote2024.Forms
{
    partial class PolygonSnapshotsForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.numTopMoversInterval = new System.Windows.Forms.NumericUpDown();
            this.btnTopMoversStop = new System.Windows.Forms.Button();
            this.btnTopMoversStart = new System.Windows.Forms.Button();
            this.lblTopMoversTickCount = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.numMarketSnapshotInterval = new System.Windows.Forms.NumericUpDown();
            this.btnMarketSnapshotStop = new System.Windows.Forms.Button();
            this.btnMarketSnapshotStart = new System.Windows.Forms.Button();
            this.lblMarketSnapshotTickCount = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTopMoversInterval)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarketSnapshotInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(642, 122);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numTopMoversInterval);
            this.groupBox1.Controls.Add(this.btnTopMoversStop);
            this.groupBox1.Controls.Add(this.btnTopMoversStart);
            this.groupBox1.Controls.Add(this.lblTopMoversTickCount);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(324, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(315, 116);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Gainers and Loisers";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(137, 15);
            this.label3.TabIndex = 18;
            this.label3.Text = "Timer interval (seconds):";
            // 
            // numTopMoversInterval
            // 
            this.numTopMoversInterval.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numTopMoversInterval.Location = new System.Drawing.Point(152, 72);
            this.numTopMoversInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numTopMoversInterval.Name = "numTopMoversInterval";
            this.numTopMoversInterval.Size = new System.Drawing.Size(57, 23);
            this.numTopMoversInterval.TabIndex = 17;
            this.numTopMoversInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // btnTopMoversStop
            // 
            this.btnTopMoversStop.Location = new System.Drawing.Point(227, 72);
            this.btnTopMoversStop.Name = "btnTopMoversStop";
            this.btnTopMoversStop.Size = new System.Drawing.Size(75, 23);
            this.btnTopMoversStop.TabIndex = 16;
            this.btnTopMoversStop.Text = "Stop";
            this.btnTopMoversStop.UseVisualStyleBackColor = true;
            this.btnTopMoversStop.Click += new System.EventHandler(this.btnTopMoversStop_Click);
            // 
            // btnTopMoversStart
            // 
            this.btnTopMoversStart.Location = new System.Drawing.Point(227, 34);
            this.btnTopMoversStart.Name = "btnTopMoversStart";
            this.btnTopMoversStart.Size = new System.Drawing.Size(75, 23);
            this.btnTopMoversStart.TabIndex = 15;
            this.btnTopMoversStart.Text = "Start";
            this.btnTopMoversStart.UseVisualStyleBackColor = true;
            this.btnTopMoversStart.Click += new System.EventHandler(this.btnTopMoversStart_Click);
            // 
            // lblTopMoversTickCount
            // 
            this.lblTopMoversTickCount.AutoSize = true;
            this.lblTopMoversTickCount.Location = new System.Drawing.Point(12, 37);
            this.lblTopMoversTickCount.Name = "lblTopMoversTickCount";
            this.lblTopMoversTickCount.Size = new System.Drawing.Size(75, 15);
            this.lblTopMoversTickCount.TabIndex = 13;
            this.lblTopMoversTickCount.Text = "lblTickCount";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.numMarketSnapshotInterval);
            this.groupBox2.Controls.Add(this.btnMarketSnapshotStop);
            this.groupBox2.Controls.Add(this.btnMarketSnapshotStart);
            this.groupBox2.Controls.Add(this.lblMarketSnapshotTickCount);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(315, 116);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Full Market Snapshot";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 15);
            this.label1.TabIndex = 12;
            this.label1.Text = "Timer interval (seconds):";
            // 
            // numMarketSnapshotInterval
            // 
            this.numMarketSnapshotInterval.Increment = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.numMarketSnapshotInterval.Location = new System.Drawing.Point(149, 71);
            this.numMarketSnapshotInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numMarketSnapshotInterval.Name = "numMarketSnapshotInterval";
            this.numMarketSnapshotInterval.Size = new System.Drawing.Size(57, 23);
            this.numMarketSnapshotInterval.TabIndex = 11;
            this.numMarketSnapshotInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // btnMarketSnapshotStop
            // 
            this.btnMarketSnapshotStop.Location = new System.Drawing.Point(224, 71);
            this.btnMarketSnapshotStop.Name = "btnMarketSnapshotStop";
            this.btnMarketSnapshotStop.Size = new System.Drawing.Size(75, 23);
            this.btnMarketSnapshotStop.TabIndex = 10;
            this.btnMarketSnapshotStop.Text = "Stop";
            this.btnMarketSnapshotStop.UseVisualStyleBackColor = true;
            this.btnMarketSnapshotStop.Click += new System.EventHandler(this.btnMarketSnapshotStop_Click);
            // 
            // btnMarketSnapshotStart
            // 
            this.btnMarketSnapshotStart.Location = new System.Drawing.Point(224, 33);
            this.btnMarketSnapshotStart.Name = "btnMarketSnapshotStart";
            this.btnMarketSnapshotStart.Size = new System.Drawing.Size(75, 23);
            this.btnMarketSnapshotStart.TabIndex = 9;
            this.btnMarketSnapshotStart.Text = "Start";
            this.btnMarketSnapshotStart.UseVisualStyleBackColor = true;
            this.btnMarketSnapshotStart.Click += new System.EventHandler(this.btnMarketSnapshotStart_Click);
            // 
            // lblMarketSnapshotTickCount
            // 
            this.lblMarketSnapshotTickCount.AutoSize = true;
            this.lblMarketSnapshotTickCount.Location = new System.Drawing.Point(9, 38);
            this.lblMarketSnapshotTickCount.Name = "lblMarketSnapshotTickCount";
            this.lblMarketSnapshotTickCount.Size = new System.Drawing.Size(75, 15);
            this.lblMarketSnapshotTickCount.TabIndex = 7;
            this.lblMarketSnapshotTickCount.Text = "lblTickCount";
            // 
            // PolygonSnapshotsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(642, 122);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MinimumSize = new System.Drawing.Size(630, 150);
            this.Name = "PolygonSnapshotsForm";
            this.Text = "PolygonSnapshotsForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numTopMoversInterval)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMarketSnapshotInterval)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numMarketSnapshotInterval;
        private System.Windows.Forms.Button btnMarketSnapshotStop;
        private System.Windows.Forms.Button btnMarketSnapshotStart;
        private System.Windows.Forms.Label lblMarketSnapshotTickCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numTopMoversInterval;
        private System.Windows.Forms.Button btnTopMoversStop;
        private System.Windows.Forms.Button btnTopMoversStart;
        private System.Windows.Forms.Label lblTopMoversTickCount;
    }
}
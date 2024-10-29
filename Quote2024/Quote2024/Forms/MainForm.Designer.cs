
namespace Quote2024.Forms
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
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpLoader = new System.Windows.Forms.TabPage();
            this.btnDailyBy5Minutes = new System.Windows.Forms.Button();
            this.btnYahooWebSocketForm = new System.Windows.Forms.Button();
            this.btnFinageTradesForm = new System.Windows.Forms.Button();
            this.btnFinageMinuteForm = new System.Windows.Forms.Button();
            this.btnOpenTestForm = new System.Windows.Forms.Button();
            this.btnOpenTimeSalesNasdaq = new System.Windows.Forms.Button();
            this.btnOpenRealTimeYahoo = new System.Windows.Forms.Button();
            this.btnOpenWebSocket = new System.Windows.Forms.Button();
            this.btnMinuteYahooSaveLogToDb = new System.Windows.Forms.Button();
            this.btnMinuteYahooErrorCheck = new System.Windows.Forms.Button();
            this.btnMinuteYahooLog = new System.Windows.Forms.Button();
            this.btnRunMultiItemsLoader = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.checkedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.Image = new System.Windows.Forms.DataGridViewImageColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Started = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Duration = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.loaderItemBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.btnTest = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnIntradayBy5Minutes = new System.Windows.Forms.Button();
            this.statusStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpLoader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.loaderItemBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 388);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(804, 22);
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
            this.tabControl1.Size = new System.Drawing.Size(804, 388);
            this.tabControl1.TabIndex = 1;
            // 
            // tpLoader
            // 
            this.tpLoader.AutoScroll = true;
            this.tpLoader.Controls.Add(this.btnIntradayBy5Minutes);
            this.tpLoader.Controls.Add(this.btnDailyBy5Minutes);
            this.tpLoader.Controls.Add(this.btnYahooWebSocketForm);
            this.tpLoader.Controls.Add(this.btnFinageTradesForm);
            this.tpLoader.Controls.Add(this.btnFinageMinuteForm);
            this.tpLoader.Controls.Add(this.btnOpenTestForm);
            this.tpLoader.Controls.Add(this.btnOpenTimeSalesNasdaq);
            this.tpLoader.Controls.Add(this.btnOpenRealTimeYahoo);
            this.tpLoader.Controls.Add(this.btnOpenWebSocket);
            this.tpLoader.Controls.Add(this.btnMinuteYahooSaveLogToDb);
            this.tpLoader.Controls.Add(this.btnMinuteYahooErrorCheck);
            this.tpLoader.Controls.Add(this.btnMinuteYahooLog);
            this.tpLoader.Controls.Add(this.btnRunMultiItemsLoader);
            this.tpLoader.Controls.Add(this.dataGridView1);
            this.tpLoader.Controls.Add(this.btnTest);
            this.tpLoader.Location = new System.Drawing.Point(4, 24);
            this.tpLoader.Name = "tpLoader";
            this.tpLoader.Padding = new System.Windows.Forms.Padding(3);
            this.tpLoader.Size = new System.Drawing.Size(796, 360);
            this.tpLoader.TabIndex = 0;
            this.tpLoader.Text = "Loader";
            this.tpLoader.UseVisualStyleBackColor = true;
            // 
            // btnDailyBy5Minutes
            // 
            this.btnDailyBy5Minutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDailyBy5Minutes.Location = new System.Drawing.Point(398, 156);
            this.btnDailyBy5Minutes.Name = "btnDailyBy5Minutes";
            this.btnDailyBy5Minutes.Size = new System.Drawing.Size(171, 36);
            this.btnDailyBy5Minutes.TabIndex = 79;
            this.btnDailyBy5Minutes.Text = "DailyBy5Minutes create";
            this.btnDailyBy5Minutes.UseVisualStyleBackColor = true;
            this.btnDailyBy5Minutes.Click += new System.EventHandler(this.btnDailyBy5Minutes_Click);
            // 
            // btnYahooWebSocketForm
            // 
            this.btnYahooWebSocketForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnYahooWebSocketForm.Location = new System.Drawing.Point(398, 57);
            this.btnYahooWebSocketForm.Name = "btnYahooWebSocketForm";
            this.btnYahooWebSocketForm.Size = new System.Drawing.Size(186, 36);
            this.btnYahooWebSocketForm.TabIndex = 78;
            this.btnYahooWebSocketForm.Text = "Open Yahoo WebSocket form";
            this.btnYahooWebSocketForm.UseVisualStyleBackColor = true;
            this.btnYahooWebSocketForm.Click += new System.EventHandler(this.btnYahooWebSocketForm_Click);
            // 
            // btnFinageTradesForm
            // 
            this.btnFinageTradesForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFinageTradesForm.Location = new System.Drawing.Point(591, 262);
            this.btnFinageTradesForm.Name = "btnFinageTradesForm";
            this.btnFinageTradesForm.Size = new System.Drawing.Size(196, 33);
            this.btnFinageTradesForm.TabIndex = 77;
            this.btnFinageTradesForm.Text = "Open Finage Trades form";
            this.btnFinageTradesForm.UseVisualStyleBackColor = true;
            this.btnFinageTradesForm.Click += new System.EventHandler(this.btnFinageTradesForm_Click);
            // 
            // btnFinageMinuteForm
            // 
            this.btnFinageMinuteForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFinageMinuteForm.Location = new System.Drawing.Point(591, 223);
            this.btnFinageMinuteForm.Name = "btnFinageMinuteForm";
            this.btnFinageMinuteForm.Size = new System.Drawing.Size(196, 33);
            this.btnFinageMinuteForm.TabIndex = 76;
            this.btnFinageMinuteForm.Text = "Open Finage Minute form";
            this.btnFinageMinuteForm.UseVisualStyleBackColor = true;
            this.btnFinageMinuteForm.Click += new System.EventHandler(this.btnFinageMinuteForm_Click);
            // 
            // btnOpenTestForm
            // 
            this.btnOpenTestForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenTestForm.Location = new System.Drawing.Point(591, 318);
            this.btnOpenTestForm.Name = "btnOpenTestForm";
            this.btnOpenTestForm.Size = new System.Drawing.Size(196, 33);
            this.btnOpenTestForm.TabIndex = 75;
            this.btnOpenTestForm.Text = "Open Test form";
            this.btnOpenTestForm.UseVisualStyleBackColor = true;
            this.btnOpenTestForm.Click += new System.EventHandler(this.btnOpenTestForm_Click);
            // 
            // btnOpenTimeSalesNasdaq
            // 
            this.btnOpenTimeSalesNasdaq.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenTimeSalesNasdaq.Location = new System.Drawing.Point(591, 184);
            this.btnOpenTimeSalesNasdaq.Name = "btnOpenTimeSalesNasdaq";
            this.btnOpenTimeSalesNasdaq.Size = new System.Drawing.Size(196, 33);
            this.btnOpenTimeSalesNasdaq.TabIndex = 74;
            this.btnOpenTimeSalesNasdaq.Text = "Open TimeSales Nasdaq form";
            this.btnOpenTimeSalesNasdaq.UseVisualStyleBackColor = true;
            this.btnOpenTimeSalesNasdaq.Click += new System.EventHandler(this.btnOpenTimeSalesNasdaq_Click);
            // 
            // btnOpenRealTimeYahoo
            // 
            this.btnOpenRealTimeYahoo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenRealTimeYahoo.Location = new System.Drawing.Point(591, 145);
            this.btnOpenRealTimeYahoo.Name = "btnOpenRealTimeYahoo";
            this.btnOpenRealTimeYahoo.Size = new System.Drawing.Size(198, 33);
            this.btnOpenRealTimeYahoo.TabIndex = 73;
            this.btnOpenRealTimeYahoo.Text = "Open RealTime Yahoo form";
            this.btnOpenRealTimeYahoo.UseVisualStyleBackColor = true;
            this.btnOpenRealTimeYahoo.Click += new System.EventHandler(this.btnOpenRealTimeYahoo_Click);
            // 
            // btnOpenWebSocket
            // 
            this.btnOpenWebSocket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenWebSocket.Location = new System.Drawing.Point(398, 279);
            this.btnOpenWebSocket.Name = "btnOpenWebSocket";
            this.btnOpenWebSocket.Size = new System.Drawing.Size(171, 33);
            this.btnOpenWebSocket.TabIndex = 72;
            this.btnOpenWebSocket.Text = "Open WebSocketClient form";
            this.btnOpenWebSocket.UseVisualStyleBackColor = true;
            this.btnOpenWebSocket.Click += new System.EventHandler(this.btnOpenWebSocket_Click);
            // 
            // btnMinuteYahooSaveLogToDb
            // 
            this.btnMinuteYahooSaveLogToDb.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinuteYahooSaveLogToDb.Location = new System.Drawing.Point(591, 98);
            this.btnMinuteYahooSaveLogToDb.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnMinuteYahooSaveLogToDb.Name = "btnMinuteYahooSaveLogToDb";
            this.btnMinuteYahooSaveLogToDb.Size = new System.Drawing.Size(196, 36);
            this.btnMinuteYahooSaveLogToDb.TabIndex = 71;
            this.btnMinuteYahooSaveLogToDb.Text = "Minute Yahoo Save Log to DB";
            this.btnMinuteYahooSaveLogToDb.UseVisualStyleBackColor = true;
            this.btnMinuteYahooSaveLogToDb.Click += new System.EventHandler(this.btnMinuteYahooSaveLogToDb_Click);
            // 
            // btnMinuteYahooErrorCheck
            // 
            this.btnMinuteYahooErrorCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinuteYahooErrorCheck.Location = new System.Drawing.Point(617, 51);
            this.btnMinuteYahooErrorCheck.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnMinuteYahooErrorCheck.Name = "btnMinuteYahooErrorCheck";
            this.btnMinuteYahooErrorCheck.Size = new System.Drawing.Size(170, 41);
            this.btnMinuteYahooErrorCheck.TabIndex = 70;
            this.btnMinuteYahooErrorCheck.Text = "Minute Yahoo Error Check";
            this.btnMinuteYahooErrorCheck.UseVisualStyleBackColor = true;
            this.btnMinuteYahooErrorCheck.Click += new System.EventHandler(this.btnMinuteYahooErrorCheck_Click);
            // 
            // btnMinuteYahooLog
            // 
            this.btnMinuteYahooLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinuteYahooLog.Location = new System.Drawing.Point(617, 6);
            this.btnMinuteYahooLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnMinuteYahooLog.Name = "btnMinuteYahooLog";
            this.btnMinuteYahooLog.Size = new System.Drawing.Size(170, 39);
            this.btnMinuteYahooLog.TabIndex = 69;
            this.btnMinuteYahooLog.Text = "Minute Yahoo Log (for zip)";
            this.btnMinuteYahooLog.UseVisualStyleBackColor = true;
            this.btnMinuteYahooLog.Click += new System.EventHandler(this.btnMinuteYahooLog_Click);
            // 
            // btnRunMultiItemsLoader
            // 
            this.btnRunMultiItemsLoader.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunMultiItemsLoader.Location = new System.Drawing.Point(398, 6);
            this.btnRunMultiItemsLoader.Name = "btnRunMultiItemsLoader";
            this.btnRunMultiItemsLoader.Size = new System.Drawing.Size(144, 37);
            this.btnRunMultiItemsLoader.TabIndex = 68;
            this.btnRunMultiItemsLoader.Text = "Run multiItems loader";
            this.btnRunMultiItemsLoader.UseVisualStyleBackColor = true;
            this.btnRunMultiItemsLoader.Click += new System.EventHandler(this.btnRunMultiItemsLoader_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.ColumnHeadersVisible = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.checkedDataGridViewCheckBoxColumn,
            this.Image,
            this.nameDataGridViewTextBoxColumn,
            this.Started,
            this.Duration});
            this.dataGridView1.DataSource = this.loaderItemBindingSource;
            this.dataGridView1.Location = new System.Drawing.Point(6, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 18;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(385, 348);
            this.dataGridView1.TabIndex = 67;
            this.dataGridView1.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // checkedDataGridViewCheckBoxColumn
            // 
            this.checkedDataGridViewCheckBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.checkedDataGridViewCheckBoxColumn.DataPropertyName = "Checked";
            this.checkedDataGridViewCheckBoxColumn.HeaderText = "Checked";
            this.checkedDataGridViewCheckBoxColumn.Name = "checkedDataGridViewCheckBoxColumn";
            this.checkedDataGridViewCheckBoxColumn.Width = 5;
            // 
            // Image
            // 
            this.Image.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Image.DataPropertyName = "Image";
            this.Image.HeaderText = "Image";
            this.Image.Name = "Image";
            this.Image.ReadOnly = true;
            this.Image.Width = 5;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Name";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // Started
            // 
            this.Started.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Started.DataPropertyName = "Started";
            this.Started.HeaderText = "Started";
            this.Started.MinimumWidth = 2;
            this.Started.Name = "Started";
            this.Started.ReadOnly = true;
            this.Started.Width = 2;
            // 
            // Duration
            // 
            this.Duration.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
            this.Duration.DataPropertyName = "Duration";
            this.Duration.HeaderText = "Duration";
            this.Duration.MinimumWidth = 24;
            this.Duration.Name = "Duration";
            this.Duration.ReadOnly = true;
            this.Duration.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Duration.Width = 24;
            // 
            // loaderItemBindingSource
            // 
            this.loaderItemBindingSource.DataSource = typeof(Data.Models.LoaderItem);
            // 
            // btnTest
            // 
            this.btnTest.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTest.Location = new System.Drawing.Point(435, 318);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 36);
            this.btnTest.TabIndex = 66;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(796, 360);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnIntradayBy5Minutes
            // 
            this.btnIntradayBy5Minutes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIntradayBy5Minutes.Location = new System.Drawing.Point(397, 198);
            this.btnIntradayBy5Minutes.Name = "btnIntradayBy5Minutes";
            this.btnIntradayBy5Minutes.Size = new System.Drawing.Size(171, 36);
            this.btnIntradayBy5Minutes.TabIndex = 80;
            this.btnIntradayBy5Minutes.Text = "IntradayBy5Minutes create";
            this.btnIntradayBy5Minutes.UseVisualStyleBackColor = true;
            this.btnIntradayBy5Minutes.Click += new System.EventHandler(this.btnIntradayBy5Minutes_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 410);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpLoader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.loaderItemBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLoader;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource loaderItemBindingSource;
        private System.Windows.Forms.DataGridViewCheckBoxColumn checkedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewImageColumn Image;
        private System.Windows.Forms.DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn Started;
        private System.Windows.Forms.DataGridViewTextBoxColumn Duration;
        private System.Windows.Forms.Button btnRunMultiItemsLoader;
        private System.Windows.Forms.Button btnMinuteYahooLog;
        private System.Windows.Forms.Button btnMinuteYahooSaveLogToDb;
        private System.Windows.Forms.Button btnMinuteYahooErrorCheck;
        private System.Windows.Forms.Button btnOpenWebSocket;
        private System.Windows.Forms.Button btnOpenRealTimeYahoo;
        private System.Windows.Forms.Button btnOpenTimeSalesNasdaq;
        private System.Windows.Forms.Button btnOpenTestForm;
        private System.Windows.Forms.Button btnFinageMinuteForm;
        private System.Windows.Forms.Button btnFinageTradesForm;
        private System.Windows.Forms.Button btnYahooWebSocketForm;
        private System.Windows.Forms.Button btnDailyBy5Minutes;
        private System.Windows.Forms.Button btnIntradayBy5Minutes;
    }
}

namespace Quote2024.Forms
{
    partial class TickerListParameterForm
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
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.numPreviousDays = new System.Windows.Forms.NumericUpDown();
            this.numMinTradeValue = new System.Windows.Forms.NumericUpDown();
            this.numMaxTradeValue = new System.Windows.Forms.NumericUpDown();
            this.numMinTradeCount = new System.Windows.Forms.NumericUpDown();
            this.numMinClose = new System.Windows.Forms.NumericUpDown();
            this.numMaxClose = new System.Windows.Forms.NumericUpDown();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cbIncludeIndices = new System.Windows.Forms.CheckBox();
            this.txtTickerList = new System.Windows.Forms.TextBox();
            this.btnUpdateList = new System.Windows.Forms.Button();
            this.lblTickerList = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPreviousDays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxTradeValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeCount)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClose)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(166, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Number of previous days:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 43);
            this.label2.TabIndex = 1;
            this.label2.Text = "Minimum trade values (Close*Volume) in mln:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 164);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(146, 19);
            this.label4.TabIndex = 3;
            this.label4.Text = "Minimum trade count:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 201);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 19);
            this.label5.TabIndex = 4;
            this.label5.Text = "Minimum Close ($):";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 238);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(130, 19);
            this.label6.TabIndex = 5;
            this.label6.Text = "Maximum Close ($):";
            // 
            // numPreviousDays
            // 
            this.numPreviousDays.Location = new System.Drawing.Point(220, 10);
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
            this.numPreviousDays.Size = new System.Drawing.Size(46, 25);
            this.numPreviousDays.TabIndex = 6;
            this.numPreviousDays.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // numMinTradeValue
            // 
            this.numMinTradeValue.Location = new System.Drawing.Point(220, 65);
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
            this.numMinTradeValue.Size = new System.Drawing.Size(46, 25);
            this.numMinTradeValue.TabIndex = 7;
            this.numMinTradeValue.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // numMaxTradeValue
            // 
            this.numMaxTradeValue.Location = new System.Drawing.Point(220, 121);
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
            this.numMaxTradeValue.Size = new System.Drawing.Size(74, 25);
            this.numMaxTradeValue.TabIndex = 8;
            this.numMaxTradeValue.ThousandsSeparator = true;
            this.numMaxTradeValue.Value = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            // 
            // numMinTradeCount
            // 
            this.numMinTradeCount.Location = new System.Drawing.Point(220, 164);
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
            this.numMinTradeCount.Size = new System.Drawing.Size(74, 25);
            this.numMinTradeCount.TabIndex = 9;
            this.numMinTradeCount.ThousandsSeparator = true;
            this.numMinTradeCount.Value = new decimal(new int[] {
            10000,
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
            this.numMinClose.Location = new System.Drawing.Point(220, 201);
            this.numMinClose.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMinClose.Name = "numMinClose";
            this.numMinClose.Size = new System.Drawing.Size(74, 25);
            this.numMinClose.TabIndex = 10;
            this.numMinClose.ThousandsSeparator = true;
            this.numMinClose.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // numMaxClose
            // 
            this.numMaxClose.Location = new System.Drawing.Point(220, 238);
            this.numMaxClose.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.numMaxClose.Name = "numMaxClose";
            this.numMaxClose.Size = new System.Drawing.Size(74, 25);
            this.numMaxClose.TabIndex = 11;
            this.numMaxClose.ThousandsSeparator = true;
            this.numMaxClose.Value = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(209, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 32);
            this.btnOK.TabIndex = 12;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(209, 346);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 32);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cbIncludeIndices
            // 
            this.cbIncludeIndices.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbIncludeIndices.Checked = true;
            this.cbIncludeIndices.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbIncludeIndices.Location = new System.Drawing.Point(12, 279);
            this.cbIncludeIndices.Name = "cbIncludeIndices";
            this.cbIncludeIndices.Size = new System.Drawing.Size(222, 23);
            this.cbIncludeIndices.TabIndex = 15;
            this.cbIncludeIndices.Text = "Include indices (DJI, SP500):";
            this.cbIncludeIndices.UseVisualStyleBackColor = true;
            // 
            // txtTickerList
            // 
            this.txtTickerList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTickerList.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtTickerList.Location = new System.Drawing.Point(321, 28);
            this.txtTickerList.Multiline = true;
            this.txtTickerList.Name = "txtTickerList";
            this.txtTickerList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTickerList.Size = new System.Drawing.Size(328, 363);
            this.txtTickerList.TabIndex = 16;
            this.txtTickerList.TextChanged += new System.EventHandler(this.txtTickerList_TextChanged);
            // 
            // btnUpdateList
            // 
            this.btnUpdateList.Location = new System.Drawing.Point(15, 317);
            this.btnUpdateList.Name = "btnUpdateList";
            this.btnUpdateList.Size = new System.Drawing.Size(96, 32);
            this.btnUpdateList.TabIndex = 17;
            this.btnUpdateList.Text = "Update list";
            this.btnUpdateList.UseVisualStyleBackColor = true;
            this.btnUpdateList.Click += new System.EventHandler(this.btnUpdateList_Click);
            // 
            // lblTickerList
            // 
            this.lblTickerList.AutoSize = true;
            this.lblTickerList.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTickerList.Location = new System.Drawing.Point(321, 10);
            this.lblTickerList.Name = "lblTickerList";
            this.lblTickerList.Size = new System.Drawing.Size(47, 15);
            this.lblTickerList.TabIndex = 18;
            this.lblTickerList.Text = "Tickers:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(13, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 43);
            this.label3.TabIndex = 19;
            this.label3.Text = "Maximum trade values (Close*Volume) in mln:";
            // 
            // TickerListParameterForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(661, 403);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTickerList);
            this.Controls.Add(this.btnUpdateList);
            this.Controls.Add(this.txtTickerList);
            this.Controls.Add(this.cbIncludeIndices);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.numMaxClose);
            this.Controls.Add(this.numMinClose);
            this.Controls.Add(this.numMinTradeCount);
            this.Controls.Add(this.numMaxTradeValue);
            this.Controls.Add(this.numMinTradeValue);
            this.Controls.Add(this.numPreviousDays);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimizeBox = false;
            this.Name = "TickerListParameterForm";
            this.Text = "TickerListParameters";
            ((System.ComponentModel.ISupportInitialize)(this.numPreviousDays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxTradeValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinTradeCount)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMinClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numMaxClose)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numPreviousDays;
        private System.Windows.Forms.NumericUpDown numMinTradeValue;
        private System.Windows.Forms.NumericUpDown numMaxTradeValue;
        private System.Windows.Forms.NumericUpDown numMinTradeCount;
        private System.Windows.Forms.NumericUpDown numMinClose;
        private System.Windows.Forms.NumericUpDown numMaxClose;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox cbIncludeIndices;
        private System.Windows.Forms.TextBox txtTickerList;
        private System.Windows.Forms.Button btnUpdateList;
        private System.Windows.Forms.Label lblTickerList;
        private System.Windows.Forms.Label label3;
    }
}
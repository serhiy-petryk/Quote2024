
namespace WebSocketClientApp
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
            this.SendMessageText = new System.Windows.Forms.TextBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.DisconnectButton = new System.Windows.Forms.Button();
            this.ClearLogButton = new System.Windows.Forms.Button();
            this.cbLogMessages = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLogFileName = new System.Windows.Forms.TextBox();
            this.cbSaveLogToFile = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cbSocketList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // SendMessageText
            // 
            this.SendMessageText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SendMessageText.Location = new System.Drawing.Point(12, 41);
            this.SendMessageText.Multiline = true;
            this.SendMessageText.Name = "SendMessageText";
            this.SendMessageText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SendMessageText.Size = new System.Drawing.Size(542, 69);
            this.SendMessageText.TabIndex = 0;
            // 
            // SendButton
            // 
            this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SendButton.Location = new System.Drawing.Point(560, 66);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(75, 27);
            this.SendButton.TabIndex = 1;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(12, 116);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(623, 319);
            this.listBox1.TabIndex = 2;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(704, 91);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 50);
            this.ConnectButton.TabIndex = 3;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // DisconnectButton
            // 
            this.DisconnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DisconnectButton.Location = new System.Drawing.Point(704, 147);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(75, 46);
            this.DisconnectButton.TabIndex = 4;
            this.DisconnectButton.Text = "Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // ClearLogButton
            // 
            this.ClearLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearLogButton.Location = new System.Drawing.Point(704, 264);
            this.ClearLogButton.Name = "ClearLogButton";
            this.ClearLogButton.Size = new System.Drawing.Size(75, 46);
            this.ClearLogButton.TabIndex = 9;
            this.ClearLogButton.Text = "Clear Log";
            this.ClearLogButton.UseVisualStyleBackColor = true;
            this.ClearLogButton.Click += new System.EventHandler(this.ClearLogButton_Click);
            // 
            // cbLogMessages
            // 
            this.cbLogMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbLogMessages.AutoSize = true;
            this.cbLogMessages.Checked = true;
            this.cbLogMessages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbLogMessages.Location = new System.Drawing.Point(688, 239);
            this.cbLogMessages.Name = "cbLogMessages";
            this.cbLogMessages.Size = new System.Drawing.Size(105, 19);
            this.cbLogMessages.TabIndex = 10;
            this.cbLogMessages.Text = "Log messages?";
            this.cbLogMessages.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Log file name";
            // 
            // txtLogFileName
            // 
            this.txtLogFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogFileName.Location = new System.Drawing.Point(97, 9);
            this.txtLogFileName.Name = "txtLogFileName";
            this.txtLogFileName.Size = new System.Drawing.Size(381, 23);
            this.txtLogFileName.TabIndex = 12;
            // 
            // cbSaveLogToFile
            // 
            this.cbSaveLogToFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSaveLogToFile.AutoSize = true;
            this.cbSaveLogToFile.Location = new System.Drawing.Point(484, 11);
            this.cbSaveLogToFile.Name = "cbSaveLogToFile";
            this.cbSaveLogToFile.Size = new System.Drawing.Size(141, 19);
            this.cbSaveLogToFile.TabIndex = 13;
            this.cbSaveLogToFile.Text = "Save messages in file?";
            this.cbSaveLogToFile.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(679, 389);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(114, 46);
            this.button1.TabIndex = 14;
            this.button1.Text = "Open new form";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cbSocketList
            // 
            this.cbSocketList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbSocketList.Items.AddRange(new object[] {
            "wss://echo.websocket.org",
            "wss://delayed.polygon.io/stocks",
            "wss://socket.polygon.io/stocks",
            "wss://streamer.finance.yahoo.com"});
            this.cbSocketList.Location = new System.Drawing.Point(631, 30);
            this.cbSocketList.Name = "cbSocketList";
            this.cbSocketList.Size = new System.Drawing.Size(200, 23);
            this.cbSocketList.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(631, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 15);
            this.label2.TabIndex = 17;
            this.label2.Text = "Web socket url:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(838, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbSocketList);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cbSaveLogToFile);
            this.Controls.Add(this.txtLogFileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbLogMessages);
            this.Controls.Add(this.ClearLogButton);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.SendMessageText);
            this.Name = "MainForm";
            this.Text = "TestMainForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox SendMessageText;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.Button DisconnectButton;
        private System.Windows.Forms.Button ClearLogButton;
        private System.Windows.Forms.CheckBox cbLogMessages;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLogFileName;
        private System.Windows.Forms.CheckBox cbSaveLogToFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cbSocketList;
        private System.Windows.Forms.Label label2;
    }
}
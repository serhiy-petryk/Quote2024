
namespace Tests
{
    partial class TestMainForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.WebServerUri = new System.Windows.Forms.TextBox();
            this.ClearLogButton = new System.Windows.Forms.Button();
            this.cbLogMessages = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // SendMessageText
            // 
            this.SendMessageText.Location = new System.Drawing.Point(25, 48);
            this.SendMessageText.Multiline = true;
            this.SendMessageText.Name = "SendMessageText";
            this.SendMessageText.Size = new System.Drawing.Size(529, 62);
            this.SendMessageText.TabIndex = 0;
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(560, 67);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(75, 43);
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
            this.listBox1.Location = new System.Drawing.Point(25, 116);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(623, 319);
            this.listBox1.TabIndex = 2;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(697, 48);
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
            this.DisconnectButton.Location = new System.Drawing.Point(697, 116);
            this.DisconnectButton.Name = "DisconnectButton";
            this.DisconnectButton.Size = new System.Drawing.Size(75, 46);
            this.DisconnectButton.TabIndex = 4;
            this.DisconnectButton.Text = "Disconnect";
            this.DisconnectButton.UseVisualStyleBackColor = true;
            this.DisconnectButton.Click += new System.EventHandler(this.DisconnectButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(177, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(185, 23);
            this.textBox1.TabIndex = 5;
            this.textBox1.Text = "wss://delayed.polygon.io/stocks";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(25, 12);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(146, 23);
            this.textBox2.TabIndex = 6;
            this.textBox2.Text = "wss://echo.websocket.org";
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(368, 12);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(176, 23);
            this.textBox3.TabIndex = 7;
            this.textBox3.Text = "wss://socket.polygon.io/stocks";
            // 
            // WebServerUri
            // 
            this.WebServerUri.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.WebServerUri.Location = new System.Drawing.Point(578, 14);
            this.WebServerUri.Name = "WebServerUri";
            this.WebServerUri.Size = new System.Drawing.Size(194, 23);
            this.WebServerUri.TabIndex = 8;
            this.WebServerUri.Text = "wss://echo.websocket.org";
            // 
            // ClearLogButton
            // 
            this.ClearLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearLogButton.Location = new System.Drawing.Point(697, 208);
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
            this.cbLogMessages.Location = new System.Drawing.Point(690, 285);
            this.cbLogMessages.Name = "cbLogMessages";
            this.cbLogMessages.Size = new System.Drawing.Size(105, 19);
            this.cbLogMessages.TabIndex = 10;
            this.cbLogMessages.Text = "Log messages?";
            this.cbLogMessages.UseVisualStyleBackColor = true;
            // 
            // TestMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.cbLogMessages);
            this.Controls.Add(this.ClearLogButton);
            this.Controls.Add(this.WebServerUri);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.DisconnectButton);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.SendMessageText);
            this.Name = "TestMainForm";
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
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox WebServerUri;
        private System.Windows.Forms.Button ClearLogButton;
        private System.Windows.Forms.CheckBox cbLogMessages;
    }
}
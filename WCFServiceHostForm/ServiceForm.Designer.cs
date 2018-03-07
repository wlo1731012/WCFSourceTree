namespace WCFServiceHostForm
{
    partial class ServiceForm
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
            this.btnBroadCast = new System.Windows.Forms.Button();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.lsbUserList = new System.Windows.Forms.ListBox();
            this.rtbHistory = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnBroadCast
            // 
            this.btnBroadCast.BackColor = System.Drawing.Color.Black;
            this.btnBroadCast.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnBroadCast.Location = new System.Drawing.Point(416, 352);
            this.btnBroadCast.Name = "btnBroadCast";
            this.btnBroadCast.Size = new System.Drawing.Size(75, 23);
            this.btnBroadCast.TabIndex = 0;
            this.btnBroadCast.Text = "CHAT";
            this.btnBroadCast.UseVisualStyleBackColor = false;
            this.btnBroadCast.Click += new System.EventHandler(this.btnBroadCast_Click);
            // 
            // txtChat
            // 
            this.txtChat.BackColor = System.Drawing.SystemColors.InfoText;
            this.txtChat.ForeColor = System.Drawing.SystemColors.Info;
            this.txtChat.Location = new System.Drawing.Point(138, 353);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(272, 22);
            this.txtChat.TabIndex = 1;
            // 
            // lsbUserList
            // 
            this.lsbUserList.BackColor = System.Drawing.SystemColors.InfoText;
            this.lsbUserList.ForeColor = System.Drawing.SystemColors.Info;
            this.lsbUserList.FormattingEnabled = true;
            this.lsbUserList.ItemHeight = 12;
            this.lsbUserList.Location = new System.Drawing.Point(12, 12);
            this.lsbUserList.Name = "lsbUserList";
            this.lsbUserList.Size = new System.Drawing.Size(120, 364);
            this.lsbUserList.TabIndex = 2;
            this.lsbUserList.SelectedIndexChanged += new System.EventHandler(this.lsbUserList_SelectedIndexChanged);
            // 
            // rtbHistory
            // 
            this.rtbHistory.BackColor = System.Drawing.SystemColors.InfoText;
            this.rtbHistory.ForeColor = System.Drawing.SystemColors.Info;
            this.rtbHistory.Location = new System.Drawing.Point(138, 12);
            this.rtbHistory.Name = "rtbHistory";
            this.rtbHistory.ShortcutsEnabled = false;
            this.rtbHistory.Size = new System.Drawing.Size(353, 335);
            this.rtbHistory.TabIndex = 3;
            this.rtbHistory.Text = "";
            // 
            // ServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(503, 382);
            this.Controls.Add(this.rtbHistory);
            this.Controls.Add(this.lsbUserList);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.btnBroadCast);
            this.Name = "ServiceForm";
            this.Text = "ServiceForm";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBroadCast;
        private System.Windows.Forms.TextBox txtChat;
        private System.Windows.Forms.ListBox lsbUserList;
        private System.Windows.Forms.RichTextBox rtbHistory;
    }
}


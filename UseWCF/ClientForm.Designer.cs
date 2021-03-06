﻿namespace WCFClient
{
    partial class ClientForm
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
            this.btnChat = new System.Windows.Forms.Button();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.rtbHistory = new System.Windows.Forms.RichTextBox();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lsbUserList = new System.Windows.Forms.ListBox();
            this.btnUpload = new System.Windows.Forms.Button();
            this.pgbReadFile = new System.Windows.Forms.ProgressBar();
            this.btnDownload = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnChat
            // 
            this.btnChat.Location = new System.Drawing.Point(306, 545);
            this.btnChat.Name = "btnChat";
            this.btnChat.Size = new System.Drawing.Size(74, 27);
            this.btnChat.TabIndex = 0;
            this.btnChat.Text = "Chat";
            this.btnChat.UseVisualStyleBackColor = true;
            this.btnChat.Click += new System.EventHandler(this.btnChat_Click);
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(12, 12);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(100, 22);
            this.txtUserName.TabIndex = 1;
            this.txtUserName.TextChanged += new System.EventHandler(this.txtUserName_TextChanged);
            // 
            // rtbHistory
            // 
            this.rtbHistory.Font = new System.Drawing.Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtbHistory.Location = new System.Drawing.Point(119, 50);
            this.rtbHistory.Name = "rtbHistory";
            this.rtbHistory.Size = new System.Drawing.Size(422, 489);
            this.rtbHistory.TabIndex = 11;
            this.rtbHistory.Text = "";
            this.rtbHistory.TextChanged += new System.EventHandler(this.rtbHistory_TextChanged);
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(118, 545);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(182, 22);
            this.txtChat.TabIndex = 9;
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(118, 12);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(72, 22);
            this.btnLogin.TabIndex = 7;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lsbUserList
            // 
            this.lsbUserList.FormattingEnabled = true;
            this.lsbUserList.ItemHeight = 12;
            this.lsbUserList.Location = new System.Drawing.Point(12, 50);
            this.lsbUserList.Name = "lsbUserList";
            this.lsbUserList.Size = new System.Drawing.Size(100, 544);
            this.lsbUserList.TabIndex = 12;
            this.lsbUserList.SelectedIndexChanged += new System.EventHandler(this.lsbUserList_SelectedIndexChanged);
            // 
            // btnUpload
            // 
            this.btnUpload.Location = new System.Drawing.Point(386, 545);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(74, 27);
            this.btnUpload.TabIndex = 13;
            this.btnUpload.Text = "Upload";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);
            // 
            // pgbReadFile
            // 
            this.pgbReadFile.Location = new System.Drawing.Point(118, 573);
            this.pgbReadFile.Name = "pgbReadFile";
            this.pgbReadFile.Size = new System.Drawing.Size(421, 23);
            this.pgbReadFile.TabIndex = 14;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(466, 545);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(74, 27);
            this.btnDownload.TabIndex = 15;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 607);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.pgbReadFile);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.lsbUserList);
            this.Controls.Add(this.rtbHistory);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.txtUserName);
            this.Controls.Add(this.btnChat);
            this.Name = "ClientForm";
            this.Text = "Client1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnChat;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.RichTextBox rtbHistory;
        private System.Windows.Forms.TextBox txtChat;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.ListBox lsbUserList;
        private System.Windows.Forms.Button btnUpload;
        private System.Windows.Forms.ProgressBar pgbReadFile;
        private System.Windows.Forms.Button btnDownload;
    }
}


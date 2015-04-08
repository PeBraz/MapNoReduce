using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using PADIMapNoReduce;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Serialization.Formatters;
using System.IO;

namespace API {
    public class FormChatClient : System.Windows.Forms.Form {
        /// Required designer variable.
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tb_Conversation;
        private System.Windows.Forms.TextBox tb_InputPath;
        private System.Windows.Forms.Button bt_Submit;
        private System.Windows.Forms.TextBox tb_Port;
        private System.Windows.Forms.Label lb_Port;
        private System.Windows.Forms.Button bt_Connect;
        private System.Windows.Forms.Label lb_NumSplits;
        private System.Windows.Forms.TextBox tb_numSplits;
        private System.Windows.Forms.OpenFileDialog of_Browse;
        private System.Windows.Forms.Button bt_browse;
        private System.Windows.Forms.Label lb_separate;
        private Label lb_inputPath;
        
        private Client client;
        public FormChatClient() {
            //this.client = new Client();
            // Required for Windows Form Designer support
            InitializeComponent();
            // TODO: Add any constructor code after InitializeComponent call
        }

        /// Clean up any resources being used.
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        // Required method for Designer support - do not modify the contents of this method with the code editor.
        private void InitializeComponent() {
            this.tb_Conversation = new System.Windows.Forms.TextBox();
            this.tb_InputPath = new System.Windows.Forms.TextBox();
            this.bt_Submit = new System.Windows.Forms.Button();
            this.tb_Port = new System.Windows.Forms.TextBox();
            this.lb_Port = new System.Windows.Forms.Label();
            this.bt_Connect = new System.Windows.Forms.Button();
            this.lb_NumSplits = new System.Windows.Forms.Label();
            this.tb_numSplits = new System.Windows.Forms.TextBox();
            this.of_Browse = new System.Windows.Forms.OpenFileDialog();
            this.bt_browse = new System.Windows.Forms.Button();
            this.lb_separate = new System.Windows.Forms.Label();
            this.lb_inputPath = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tb_Conversation
            // 
            this.tb_Conversation.AcceptsReturn = true;
            this.tb_Conversation.AcceptsTab = true;
            this.tb_Conversation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_Conversation.Location = new System.Drawing.Point(222, 90);
            this.tb_Conversation.Multiline = true;
            this.tb_Conversation.Name = "tb_Conversation";
            this.tb_Conversation.ReadOnly = true;
            this.tb_Conversation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Conversation.Size = new System.Drawing.Size(215, 168);
            this.tb_Conversation.TabIndex = 0;
            // 
            // tb_InputPath
            // 
            this.tb_InputPath.Location = new System.Drawing.Point(64, 264);
            this.tb_InputPath.Name = "tb_InputPath";
            this.tb_InputPath.Size = new System.Drawing.Size(314, 20);
            this.tb_InputPath.TabIndex = 1;
            // 
            // bt_Submit
            // 
            this.bt_Submit.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_Submit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Submit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_Submit.Location = new System.Drawing.Point(111, 302);
            this.bt_Submit.Name = "bt_Submit";
            this.bt_Submit.Size = new System.Drawing.Size(227, 32);
            this.bt_Submit.TabIndex = 2;
            this.bt_Submit.Text = "Submit Job";
            this.bt_Submit.UseVisualStyleBackColor = false;
            this.bt_Submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // tb_Port
            // 
            this.tb_Port.Location = new System.Drawing.Point(13, 23);
            this.tb_Port.Name = "tb_Port";
            this.tb_Port.Size = new System.Drawing.Size(84, 20);
            this.tb_Port.TabIndex = 3;
            // 
            // lb_Port
            // 
            this.lb_Port.Location = new System.Drawing.Point(10, 4);
            this.lb_Port.Name = "lb_Port";
            this.lb_Port.Size = new System.Drawing.Size(85, 16);
            this.lb_Port.TabIndex = 4;
            this.lb_Port.Text = "JobTracker Port";
            this.lb_Port.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bt_Connect
            // 
            this.bt_Connect.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_Connect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Connect.Location = new System.Drawing.Point(103, 21);
            this.bt_Connect.Name = "bt_Connect";
            this.bt_Connect.Size = new System.Drawing.Size(125, 23);
            this.bt_Connect.TabIndex = 5;
            this.bt_Connect.Text = "Initiate Client";
            this.bt_Connect.UseVisualStyleBackColor = false;
            this.bt_Connect.Click += new System.EventHandler(this.button2_Click);
            // 
            // lb_NumSplits
            // 
            this.lb_NumSplits.Location = new System.Drawing.Point(10, 240);
            this.lb_NumSplits.Name = "lb_NumSplits";
            this.lb_NumSplits.Size = new System.Drawing.Size(53, 18);
            this.lb_NumSplits.TabIndex = 6;
            this.lb_NumSplits.Text = "Nº Splits : ";
            this.lb_NumSplits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_numSplits
            // 
            this.tb_numSplits.Location = new System.Drawing.Point(64, 238);
            this.tb_numSplits.Name = "tb_numSplits";
            this.tb_numSplits.Size = new System.Drawing.Size(21, 20);
            this.tb_numSplits.TabIndex = 7;
            // 
            // of_Browse
            // 
            this.of_Browse.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            this.of_Browse.FilterIndex = 2;
            this.of_Browse.InitialDirectory = "c:\\";
            this.of_Browse.RestoreDirectory = true;
            // 
            // bt_browse
            // 
            this.bt_browse.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_browse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_browse.Location = new System.Drawing.Point(384, 261);
            this.bt_browse.Name = "bt_browse";
            this.bt_browse.Size = new System.Drawing.Size(53, 23);
            this.bt_browse.TabIndex = 8;
            this.bt_browse.Text = "browse";
            this.bt_browse.UseVisualStyleBackColor = false;
            this.bt_browse.Click += new System.EventHandler(this.openFileDialog);
            // 
            // lb_separate
            // 
            this.lb_separate.BackColor = System.Drawing.Color.Gainsboro;
            this.lb_separate.Location = new System.Drawing.Point(-8, 56);
            this.lb_separate.Name = "lb_separate";
            this.lb_separate.Size = new System.Drawing.Size(459, 21);
            this.lb_separate.TabIndex = 9;
            this.lb_separate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_inputPath
            // 
            this.lb_inputPath.Location = new System.Drawing.Point(10, 266);
            this.lb_inputPath.Name = "lb_inputPath";
            this.lb_inputPath.Size = new System.Drawing.Size(62, 18);
            this.lb_inputPath.TabIndex = 10;
            this.lb_inputPath.Text = "File Path : ";
            this.lb_inputPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // FormChatClient
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(449, 346);
            this.Controls.Add(this.tb_InputPath);
            this.Controls.Add(this.lb_inputPath);
            this.Controls.Add(this.lb_separate);
            this.Controls.Add(this.bt_browse);
            this.Controls.Add(this.tb_numSplits);
            this.Controls.Add(this.lb_NumSplits);
            this.Controls.Add(this.bt_Connect);
            this.Controls.Add(this.lb_Port);
            this.Controls.Add(this.tb_Port);
            this.Controls.Add(this.bt_Submit);
            this.Controls.Add(this.tb_Conversation);
            this.Name = "FormChatClient";
            this.Text = "PADI MAP NO REDUCE APPLICATION";
            this.Load += new System.EventHandler(this.FormChatClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main(string[] args)
        {
            String mapName = "Map";
            String code = @"..\..\..\Mapper\bin\Debug\Mapper.dll";
            new Client(mapName,code);
            //Application.Run(new FormChatClient());
        }

        private void submit_Click(object sender, System.EventArgs e) {
            //Stream myStream = null;
            this.client.setFile( this.tb_InputPath.Text);
        }  

        public void AddMsg(string s) { this.tb_Conversation.AppendText("\r\n" + s); } // Adiciona uma
        
        private void button2_Click(object sender, System.EventArgs e) { 

        }

        private void openFileDialog(object sender, System.EventArgs e) {
            if (of_Browse.ShowDialog() == DialogResult.OK) { 
                try {
                    if ((of_Browse.OpenFile()) != null) {
                        tb_InputPath.Text = of_Browse.FileName; 
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }

        }

        private void FormChatClient_Load(object sender, EventArgs e)
        {

        } // --------------------
    }

  
}

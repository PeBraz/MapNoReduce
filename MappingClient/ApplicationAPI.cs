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
        private System.Windows.Forms.TextBox tb_WorkerId;
        private System.Windows.Forms.Label lb_WorkerId;
        private System.Windows.Forms.Button bt_InitiateClient;
        private System.Windows.Forms.Label lb_NumSplits;
        private System.Windows.Forms.TextBox tb_numSplits;
        private System.Windows.Forms.OpenFileDialog of_Browse;
        private System.Windows.Forms.Button bt_browse;
        private System.Windows.Forms.Label lb_separate;
        private Label lb_inputPath;
        private Label lb_output;
        private TextBox tb_outptDir;
        private Button bt_browse2;
        private Label lb_mapperInterface;
        private TextBox tb_MapperInterface;
        private Label lb_dll;
        private TextBox tb_dll;
        private Button bt_browse4;
        
        private IClient client;
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
            this.tb_WorkerId = new System.Windows.Forms.TextBox();
            this.lb_WorkerId = new System.Windows.Forms.Label();
            this.bt_InitiateClient = new System.Windows.Forms.Button();
            this.lb_NumSplits = new System.Windows.Forms.Label();
            this.tb_numSplits = new System.Windows.Forms.TextBox();
            this.of_Browse = new System.Windows.Forms.OpenFileDialog();
            this.bt_browse = new System.Windows.Forms.Button();
            this.lb_separate = new System.Windows.Forms.Label();
            this.lb_inputPath = new System.Windows.Forms.Label();
            this.lb_output = new System.Windows.Forms.Label();
            this.tb_outptDir = new System.Windows.Forms.TextBox();
            this.bt_browse2 = new System.Windows.Forms.Button();
            this.lb_mapperInterface = new System.Windows.Forms.Label();
            this.tb_MapperInterface = new System.Windows.Forms.TextBox();
            this.lb_dll = new System.Windows.Forms.Label();
            this.tb_dll = new System.Windows.Forms.TextBox();
            this.bt_browse4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tb_Conversation
            // 
            this.tb_Conversation.AcceptsReturn = true;
            this.tb_Conversation.AcceptsTab = true;
            this.tb_Conversation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_Conversation.Location = new System.Drawing.Point(280, 90);
            this.tb_Conversation.Multiline = true;
            this.tb_Conversation.Name = "tb_Conversation";
            this.tb_Conversation.ReadOnly = true;
            this.tb_Conversation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Conversation.Size = new System.Drawing.Size(182, 34);
            this.tb_Conversation.TabIndex = 0;
            // 
            // tb_InputPath
            // 
            this.tb_InputPath.Location = new System.Drawing.Point(89, 191);
            this.tb_InputPath.Name = "tb_InputPath";
            this.tb_InputPath.Size = new System.Drawing.Size(314, 20);
            this.tb_InputPath.TabIndex = 1;
            // 
            // bt_Submit
            // 
            this.bt_Submit.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_Submit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Submit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_Submit.Location = new System.Drawing.Point(128, 319);
            this.bt_Submit.Name = "bt_Submit";
            this.bt_Submit.Size = new System.Drawing.Size(227, 32);
            this.bt_Submit.TabIndex = 2;
            this.bt_Submit.Text = "Submit Job";
            this.bt_Submit.UseVisualStyleBackColor = false;
            this.bt_Submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // tb_WorkerId
            // 
            this.tb_WorkerId.Location = new System.Drawing.Point(89, 25);
            this.tb_WorkerId.Name = "tb_WorkerId";
            this.tb_WorkerId.Size = new System.Drawing.Size(106, 20);
            this.tb_WorkerId.TabIndex = 3;
            // 
            // lb_WorkerId
            // 
            this.lb_WorkerId.Location = new System.Drawing.Point(20, 26);
            this.lb_WorkerId.Name = "lb_WorkerId";
            this.lb_WorkerId.Size = new System.Drawing.Size(85, 16);
            this.lb_WorkerId.TabIndex = 4;
            this.lb_WorkerId.Text = "Worker Id : ";
            this.lb_WorkerId.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bt_InitiateClient
            // 
            this.bt_InitiateClient.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_InitiateClient.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_InitiateClient.Location = new System.Drawing.Point(209, 24);
            this.bt_InitiateClient.Name = "bt_InitiateClient";
            this.bt_InitiateClient.Size = new System.Drawing.Size(125, 23);
            this.bt_InitiateClient.TabIndex = 5;
            this.bt_InitiateClient.Text = "Initiate Client";
            this.bt_InitiateClient.UseVisualStyleBackColor = false;
            this.bt_InitiateClient.Click += new System.EventHandler(this.initClient_Click);
            // 
            // lb_NumSplits
            // 
            this.lb_NumSplits.Location = new System.Drawing.Point(24, 166);
            this.lb_NumSplits.Name = "lb_NumSplits";
            this.lb_NumSplits.Size = new System.Drawing.Size(64, 18);
            this.lb_NumSplits.TabIndex = 6;
            this.lb_NumSplits.Text = "Nº Splits    : ";
            this.lb_NumSplits.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_numSplits
            // 
            this.tb_numSplits.Location = new System.Drawing.Point(89, 165);
            this.tb_numSplits.Name = "tb_numSplits";
            this.tb_numSplits.Size = new System.Drawing.Size(33, 20);
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
            this.bt_browse.Location = new System.Drawing.Point(409, 188);
            this.bt_browse.Name = "bt_browse";
            this.bt_browse.Size = new System.Drawing.Size(53, 23);
            this.bt_browse.TabIndex = 8;
            this.bt_browse.Text = "browse";
            this.bt_browse.UseVisualStyleBackColor = false;
            this.bt_browse.Click += new System.EventHandler(this.openFileDialog);
            // 
            // lb_separate
            // 
            this.lb_separate.BackColor = System.Drawing.Color.Silver;
            this.lb_separate.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lb_separate.Location = new System.Drawing.Point(-6, 56);
            this.lb_separate.Name = "lb_separate";
            this.lb_separate.Size = new System.Drawing.Size(495, 21);
            this.lb_separate.TabIndex = 9;
            this.lb_separate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_inputPath
            // 
            this.lb_inputPath.Location = new System.Drawing.Point(23, 192);
            this.lb_inputPath.Name = "lb_inputPath";
            this.lb_inputPath.Size = new System.Drawing.Size(69, 18);
            this.lb_inputPath.TabIndex = 10;
            this.lb_inputPath.Text = "File Path    : ";
            this.lb_inputPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lb_output
            // 
            this.lb_output.Location = new System.Drawing.Point(23, 217);
            this.lb_output.Name = "lb_output";
            this.lb_output.Size = new System.Drawing.Size(67, 18);
            this.lb_output.TabIndex = 11;
            this.lb_output.Text = "Output Dir  :";
            this.lb_output.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_outptDir
            // 
            this.tb_outptDir.Location = new System.Drawing.Point(89, 216);
            this.tb_outptDir.Name = "tb_outptDir";
            this.tb_outptDir.Size = new System.Drawing.Size(314, 20);
            this.tb_outptDir.TabIndex = 12;
            // 
            // bt_browse2
            // 
            this.bt_browse2.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_browse2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_browse2.Location = new System.Drawing.Point(409, 214);
            this.bt_browse2.Name = "bt_browse2";
            this.bt_browse2.Size = new System.Drawing.Size(53, 23);
            this.bt_browse2.TabIndex = 13;
            this.bt_browse2.Text = "browse";
            this.bt_browse2.UseVisualStyleBackColor = false;
            this.bt_browse2.Click += new System.EventHandler(this.openFileDialog2);
            // 
            // lb_mapperInterface
            // 
            this.lb_mapperInterface.Location = new System.Drawing.Point(20, 244);
            this.lb_mapperInterface.Name = "lb_mapperInterface";
            this.lb_mapperInterface.Size = new System.Drawing.Size(71, 18);
            this.lb_mapperInterface.TabIndex = 14;
            this.lb_mapperInterface.Text = "IMapper Int :";
            this.lb_mapperInterface.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_MapperInterface
            // 
            this.tb_MapperInterface.Location = new System.Drawing.Point(89, 242);
            this.tb_MapperInterface.Name = "tb_MapperInterface";
            this.tb_MapperInterface.Size = new System.Drawing.Size(314, 20);
            this.tb_MapperInterface.TabIndex = 15;
            // 
            // lb_dll
            // 
            this.lb_dll.Location = new System.Drawing.Point(24, 266);
            this.lb_dll.Name = "lb_dll";
            this.lb_dll.Size = new System.Drawing.Size(68, 18);
            this.lb_dll.TabIndex = 17;
            this.lb_dll.Text = "DLL           :";
            this.lb_dll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_dll
            // 
            this.tb_dll.Location = new System.Drawing.Point(90, 267);
            this.tb_dll.Name = "tb_dll";
            this.tb_dll.Size = new System.Drawing.Size(314, 20);
            this.tb_dll.TabIndex = 18;
            // 
            // bt_browse4
            // 
            this.bt_browse4.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_browse4.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_browse4.Location = new System.Drawing.Point(409, 266);
            this.bt_browse4.Name = "bt_browse4";
            this.bt_browse4.Size = new System.Drawing.Size(53, 23);
            this.bt_browse4.TabIndex = 19;
            this.bt_browse4.Text = "browse";
            this.bt_browse4.UseVisualStyleBackColor = false;
            this.bt_browse4.Click += new System.EventHandler(this.openFileDialog4);
            // 
            // FormChatClient
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.ClientSize = new System.Drawing.Size(485, 363);
            this.Controls.Add(this.tb_WorkerId);
            this.Controls.Add(this.bt_browse4);
            this.Controls.Add(this.tb_dll);
            this.Controls.Add(this.lb_dll);
            this.Controls.Add(this.tb_MapperInterface);
            this.Controls.Add(this.lb_mapperInterface);
            this.Controls.Add(this.bt_browse2);
            this.Controls.Add(this.tb_outptDir);
            this.Controls.Add(this.lb_output);
            this.Controls.Add(this.tb_InputPath);
            this.Controls.Add(this.lb_inputPath);
            this.Controls.Add(this.lb_separate);
            this.Controls.Add(this.bt_browse);
            this.Controls.Add(this.tb_numSplits);
            this.Controls.Add(this.lb_NumSplits);
            this.Controls.Add(this.bt_InitiateClient);
            this.Controls.Add(this.lb_WorkerId);
            this.Controls.Add(this.bt_Submit);
            this.Controls.Add(this.tb_Conversation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
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
            //new Client(1);
            Application.Run(new FormChatClient());
        }

        private void submit_Click(object sender, System.EventArgs e) {
            //Stream myStream = null;
            //this.client.setFile( this.tb_InputPath.Text);
            string a = this.tb_InputPath.Text;
            string b = this.tb_outptDir.Text;
            string c = this.tb_MapperInterface.Text;
            string d = this.tb_dll.Text;
            string f = this.tb_numSplits.Text;

            this.client.newJob(Client.trackerUrl, a, b, int.Parse(f), c, d); // temporary stuff in the bonkers
        }  

        public void AddMsg(string s) { this.tb_Conversation.AppendText("\r\n" + s); } // Adiciona uma
        
        private void initClient_Click(object sender, System.EventArgs e)
        {
            Client c = new Client(1);
            c.init(int.Parse(tb_WorkerId.Text));
            this.client = (IClient)Activator.GetObject(typeof(IClient), "tcp://localhost:" + (10000 + 1).ToString() + "/C");
            (sender as Button).Enabled = false;
        }

        private void openFileDialog(object sender, System.EventArgs e)
        {
            browseHandler(tb_InputPath);
        }

        private void openFileDialog2(object sender, System.EventArgs e)
        {
            browseHandler(tb_outptDir);
        }

        private void openFileDialog4(object sender, System.EventArgs e) {
            browseHandler(tb_dll);
        }


        private void browseHandler(TextBox textbox)
        {
            if (of_Browse.ShowDialog() == DialogResult.OK) { 
                    try {
                        if ((of_Browse.OpenFile()) != null) {
                            textbox.Text = of_Browse.FileName; 
                        }
                    }
                    catch (Exception ex) {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                    }
                }
        }
        private void FormChatClient_Load(object sender, EventArgs e)
        {

        }

    }

  
}

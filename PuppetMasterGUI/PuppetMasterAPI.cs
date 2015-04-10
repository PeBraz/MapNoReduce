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
    public class FormPuppetMaster : System.Windows.Forms.Form {
        /// Required designer variable.
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.TextBox tb_Conversation;
        private System.Windows.Forms.TextBox tb_InputPath;
        private System.Windows.Forms.Button bt_Submit;
        private System.Windows.Forms.TextBox tb_Command;
        private System.Windows.Forms.Label lb_Command;
        private System.Windows.Forms.Button bt_SubmitCommand;
        private System.Windows.Forms.OpenFileDialog of_Browse;
        private System.Windows.Forms.Button bt_browse;
        private System.Windows.Forms.Label lb_separate;
        private Label lb_Script;
        private TextBox tb_id;
        private Label lb_id;
        private Label label1;

        private PuppetMaster me;
        private Button bt_id;

        private static PuppetMaster puppetMaster = null;
        public FormPuppetMaster() {
            InitializeComponent();
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
            this.tb_Command = new System.Windows.Forms.TextBox();
            this.lb_Command = new System.Windows.Forms.Label();
            this.bt_SubmitCommand = new System.Windows.Forms.Button();
            this.of_Browse = new System.Windows.Forms.OpenFileDialog();
            this.bt_browse = new System.Windows.Forms.Button();
            this.lb_separate = new System.Windows.Forms.Label();
            this.lb_Script = new System.Windows.Forms.Label();
            this.tb_id = new System.Windows.Forms.TextBox();
            this.lb_id = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.bt_id = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tb_Conversation
            // 
            this.tb_Conversation.AcceptsReturn = true;
            this.tb_Conversation.AcceptsTab = true;
            this.tb_Conversation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tb_Conversation.Location = new System.Drawing.Point(378, 67);
            this.tb_Conversation.Multiline = true;
            this.tb_Conversation.Name = "tb_Conversation";
            this.tb_Conversation.ReadOnly = true;
            this.tb_Conversation.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tb_Conversation.Size = new System.Drawing.Size(191, 153);
            this.tb_Conversation.TabIndex = 0;
            // 
            // tb_InputPath
            // 
            this.tb_InputPath.Location = new System.Drawing.Point(64, 278);
            this.tb_InputPath.Name = "tb_InputPath";
            this.tb_InputPath.Size = new System.Drawing.Size(424, 20);
            this.tb_InputPath.TabIndex = 1;
            // 
            // bt_Submit
            // 
            this.bt_Submit.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_Submit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_Submit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_Submit.Location = new System.Drawing.Point(111, 314);
            this.bt_Submit.Name = "bt_Submit";
            this.bt_Submit.Size = new System.Drawing.Size(314, 32);
            this.bt_Submit.TabIndex = 2;
            this.bt_Submit.Text = "Submit Job";
            this.bt_Submit.UseVisualStyleBackColor = false;
            this.bt_Submit.Click += new System.EventHandler(this.submit_Click);
            // 
            // tb_Command
            // 
            this.tb_Command.Location = new System.Drawing.Point(64, 124);
            this.tb_Command.Name = "tb_Command";
            this.tb_Command.Size = new System.Drawing.Size(300, 20);
            this.tb_Command.TabIndex = 3;
            // 
            // lb_Command
            // 
            this.lb_Command.Location = new System.Drawing.Point(7, 123);
            this.lb_Command.Name = "lb_Command";
            this.lb_Command.Size = new System.Drawing.Size(69, 19);
            this.lb_Command.TabIndex = 4;
            this.lb_Command.Text = "Command : ";
            this.lb_Command.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bt_SubmitCommand
            // 
            this.bt_SubmitCommand.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_SubmitCommand.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_SubmitCommand.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_SubmitCommand.Location = new System.Drawing.Point(78, 173);
            this.bt_SubmitCommand.Name = "bt_SubmitCommand";
            this.bt_SubmitCommand.Size = new System.Drawing.Size(239, 33);
            this.bt_SubmitCommand.TabIndex = 5;
            this.bt_SubmitCommand.Text = "SubmitJob";
            this.bt_SubmitCommand.UseVisualStyleBackColor = false;
            this.bt_SubmitCommand.Click += new System.EventHandler(this.submitCommand_Click);
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
            this.bt_browse.Location = new System.Drawing.Point(494, 275);
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
            this.lb_separate.Location = new System.Drawing.Point(-7, 223);
            this.lb_separate.Name = "lb_separate";
            this.lb_separate.Size = new System.Drawing.Size(591, 21);
            this.lb_separate.TabIndex = 9;
            this.lb_separate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_Script
            // 
            this.lb_Script.Location = new System.Drawing.Point(20, 282);
            this.lb_Script.Name = "lb_Script";
            this.lb_Script.Size = new System.Drawing.Size(43, 18);
            this.lb_Script.TabIndex = 10;
            this.lb_Script.Text = "Script :";
            this.lb_Script.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tb_id
            // 
            this.tb_id.Location = new System.Drawing.Point(64, 12);
            this.tb_id.Name = "tb_id";
            this.tb_id.Size = new System.Drawing.Size(33, 20);
            this.tb_id.TabIndex = 11;
            this.tb_id.TextChanged += new System.EventHandler(this.tb_Port_TextChanged);
            // 
            // lb_id
            // 
            this.lb_id.Location = new System.Drawing.Point(29, 12);
            this.lb_id.Name = "lb_id";
            this.lb_id.Size = new System.Drawing.Size(34, 19);
            this.lb_id.TabIndex = 12;
            this.lb_id.Text = "Port : ";
            this.lb_id.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Gainsboro;
            this.label1.Location = new System.Drawing.Point(-7, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(591, 21);
            this.label1.TabIndex = 13;
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bt_id
            // 
            this.bt_id.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.bt_id.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bt_id.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_id.Location = new System.Drawing.Point(111, 7);
            this.bt_id.Name = "bt_id";
            this.bt_id.Size = new System.Drawing.Size(137, 33);
            this.bt_id.TabIndex = 14;
            this.bt_id.Text = "Start PuppetMaster";
            this.bt_id.UseVisualStyleBackColor = false;
            this.bt_id.Click += new System.EventHandler(this.bt_id_click);
            // 
            // FormPuppetMaster
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.GrayText;
            this.ClientSize = new System.Drawing.Size(581, 368);
            this.Controls.Add(this.bt_id);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tb_id);
            this.Controls.Add(this.lb_id);
            this.Controls.Add(this.tb_Command);
            this.Controls.Add(this.tb_InputPath);
            this.Controls.Add(this.lb_Script);
            this.Controls.Add(this.lb_separate);
            this.Controls.Add(this.bt_browse);
            this.Controls.Add(this.bt_SubmitCommand);
            this.Controls.Add(this.lb_Command);
            this.Controls.Add(this.bt_Submit);
            this.Controls.Add(this.tb_Conversation);
            this.Name = "FormPuppetMaster";
            this.Text = "PADI MAP NO REDUCE APPLICATION";
            this.Load += new System.EventHandler(this.FormChatClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        [STAThread]
        static void Main(string[] args)
        {

            Application.Run(new FormPuppetMaster());
        }

        private void submit_Click(object sender, System.EventArgs e) {
            if (this.me == null) 
            { 
               MessageBox.Show("Create a puppet Master first");
               return;
            }
            this.me.readFile(this.tb_InputPath.Text);            
        }  

        public void AddMsg(string s) { this.tb_Conversation.AppendText("\r\n" + s); } // Adiciona uma
        
        private void submitCommand_Click(object sender, System.EventArgs e) {
            puppetMaster.parse(this.tb_Command.Text);
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

        }

        private void tb_Port_TextChanged(object sender, EventArgs e)
        {

        }

        private void bt_id_click(object sender, EventArgs e)
        {
            string pmId = this.tb_id.Text;

            if (pmId == null) 
            {
                MessageBox.Show("No id given");
                return;
            }

            this.me = new PuppetMaster(int.Parse(pmId));  //local instance
        }

    }

  
}

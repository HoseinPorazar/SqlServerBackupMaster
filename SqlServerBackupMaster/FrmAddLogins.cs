using SqlServerBackupMaster.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlServerBackupMaster
{
    public partial class FrmAddLogins : Form
    {
        List<Login> logins = new List<Login>();
        string FilePath = Application.StartupPath + "\\logins.xml";
        public FrmAddLogins()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtServer.Text.Length > 0 && txtusername.Text.Length > 0 && txtpass.Text.Length > 0)
            {
                logins.Add(new Login { ServerName = txtServer.Text, Username = txtusername.Text, Password = txtpass.Text });
                radListControl1.Items.Add(txtServer.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void radListControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radListControl1.SelectedIndex>=0)
            {

                txtServer.Text = logins[radListControl1.SelectedIndex].ServerName;
                txtusername.Text = logins[radListControl1.SelectedIndex].Username;
                txtpass.Text = logins[radListControl1.SelectedIndex].Password;
            }
        }

        private void radListControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {

                if (radListControl1.SelectedIndex >= 0)
                {
                    Login lo = logins.Find(l => l.ServerName == radListControl1.SelectedItem.ToString());
                    radListControl1.Items.RemoveAt(radListControl1.SelectedIndex);
                    txtpass.Text = "";
                    txtServer.Text = "";
                    txtusername.Text = "";
                    logins.Remove(lo);
                }
                MessageBox.Show("Deleted");
            }
        }

        private void FrmAddLogins_Load(object sender, EventArgs e)
        {

            List<Login> mm = Serializ.DeSerialize(FilePath);

            if (mm != null)
                logins = mm;
            foreach (Login l in logins)
            {
                radListControl1.Items.Add(l.ServerName);

            }
        }

        private void FrmAddLogins_FormClosing(object sender, FormClosingEventArgs e)
        {
            Serializ.Serialize(logins, FilePath);
        }
    }
}

using SqlServerBackupMaster.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SqlServerBackupMaster.Classes;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Threading;

namespace SqlServerBackupMaster
{
    ///
    public partial class Form1 : Form
    {
        string FilePath = Application.StartupPath + "\\logins.xml";
        string selectedDbPath = Application.StartupPath + "\\dbs.xml";
        List<Login> log;
        List<database> SelectedDatabases = new List<database>();
        List<ServerCommandPair> ServersCommands = new List<ServerCommandPair>();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkBackupOnStart.Checked = Settings.Default.BackupOnStart;
            checkCloseOnFinis.Checked = Settings.Default.ExistOnFinish;
            loadServers();
            LoadSelectedDatabases();
            RefreshSelected();

            if (this.checkBackupOnStart.Checked)
            {
                StartBAckupProcess();
            }
        }
        private void RefreshSelected()
        {
            treeView2.Nodes.Clear();
            foreach (database kv in SelectedDatabases)
            {
                TreeNode nn = treeView2.Nodes.Add(kv.Server);
                foreach (string s in kv.Databases)
                {
                    nn.Nodes.Add(s);
                }


            }
           treeView2.ExpandAll();
        }
        private void StartBAckupProcess()
        {


            Backu();
        }
        private void Backu()
        {
            // string RootFolder = @"\\192.168.100.96\bk\";
            // string RootFolder = @"\\192.168.100.191\bk\";
            string RootFolder = textBox1.Text;

            if (SelectedDatabases.Count > 0)
            {
                string stra;

                if (DateTime.Now.Hour < 12)
                {
                    stra = "-AM";
                }
                else
                {
                    stra = "-PM";
                }

                PersianCalendar calendar = new PersianCalendar();
                string year = calendar.GetYear(DateTime.Now).ToString();
                string month = calendar.GetMonth(DateTime.Now).ToString();
                string dayOfMonth = calendar.GetDayOfMonth(DateTime.Now).ToString();
                if (int.Parse(month) < 10)
                {
                    month = "0" + month;
                }

                if (int.Parse(dayOfMonth) < 10)
                {
                    dayOfMonth = "0" + dayOfMonth;
                }

                string BackupFolder = RootFolder + year.ToString() + month + dayOfMonth + stra;
                string MegaBackupFolder = RootFolder + "MEGA_" + Convert.ToDateTime(DateTime.Today).ToString("yyyyMMdd") + stra;

                if (!System.IO.Directory.Exists(BackupFolder))
                {
                    System.IO.Directory.CreateDirectory(BackupFolder);
                }

                if (!System.IO.Directory.Exists(MegaBackupFolder))
                {
                    System.IO.Directory.CreateDirectory(MegaBackupFolder);
                }

                foreach (TreeNode node in treeView2.Nodes)
                {

                    string srver = node.Text;
                    string username = log.Find(a => a.ServerName == srver).Username;
                    string password = log.Find(a => a.ServerName == srver).Password;
                    string conString = string.Format("Data Source={0};Persist Security Info=True; User Id={1};Password={2}", srver, username, password);










                    string backupFilename = BackupFolder;
                    if (srver.EndsWith(".18"))
                    {
                        backupFilename = MegaBackupFolder;
                    }
                    backupFilename += @"\";
                    string ext = ".bak";
                    foreach (TreeNode child in node.Nodes)
                    {



                        string databaseName = child.Text;
                        string timestamp = "_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
                        string _CommandText = "BACKUP DATABASE [" + databaseName + "] TO DISK = N'" + backupFilename + databaseName + timestamp + ext + "' WITH NOFORMAT, NOINIT, NAME = N'" + databaseName + "-Full Database Backup', SKIP, NOREWIND, NOUNLOAD,  STATS = 10 ";
                        ServersCommands.Add(new ServerCommandPair(conString, _CommandText));
                    }


                    //Thread t = new Thread(()=> BackupDatabase(conString, _CommandText));
                    //t.IsBackground = true;
                    //t.Priority = ThreadPriority.Normal;

                    //t.Start();
                    // BackupDatabase(conString, _CommandText);


                }
            }

            BackupDatabase();

        }
        public void BackupDatabase()
        {
            Thread t = new Thread(() =>
            {
                using (SqlConnection con = new SqlConnection(ServersCommands[counter].ConnectionString))
                {
                    con.FireInfoMessageEventOnUserErrors = true;
                    con.InfoMessage += OnInfoMessage;
                    con.Open();



                    using (var cmd = new SqlCommand(ServersCommands[counter].CmdString, con))
                    {
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                    //con.Close();
                    //con.InfoMessage -= OnInfoMessage;
                    //con.FireInfoMessageEventOnUserErrors = false;

                }
            });
            t.Start();
        }
        string _lastDbB;
        int counter = 0;
        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            foreach (SqlError info in e.Errors)
            {
                if (info.Class > 10)
                {
                    // TODO: treat this as a genuine error
                    MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnAdd.Enabled = true;
                   
                }
                else
                {
                    this.richTextBox1.Invoke((Action)(() => richTextBox1.AppendText(e.Message + "\r\n")));
                    if (e.Message.Contains("database"))
                    {
                        string dbname = e.Message.Substring(e.Message.IndexOf('\'') + 1);
                        dbname = dbname.Substring(0, dbname.IndexOf('\''));
                        _lastDbB = dbname;
                    }
                    if (e.Message.Contains("percent"))
                    {
                        string percent = e.Message.Substring(0, e.Message.IndexOf("percent")).Trim();
                        this.progressBar1.Invoke((Action)(() => {


                           progressBar1.Value = int.Parse(percent);
                            //  int xx = (int)(_backupCounter * 100 / totaldatabaseCount);
                            //   radProgressBar1.Value2 = xx;
                        }));
                    }
                    //there is not enough space
                    if (e.Message.Contains("BACKUP DATABASE successfully processed"))
                    {
                        treeView2.Invoke((Action)(() => {

                            foreach (TreeNode node in treeView2.Nodes)
                            {
                                foreach (TreeNode child in node.Nodes)
                                {
                                    if (child.Text.Equals(_lastDbB))
                                    {
                                        child.BackColor = Color.Green;
                                        counter++;
                                        if (counter < ServersCommands.Count)
                                        {
                                            BackupDatabase();
                                        }
                                        else
                                        {
                                            counter = 0;
                                            if (checkCloseOnFinis.Checked)
                                            {
                                                timer1.Interval = 1000;
                                                timer1.Enabled = true;
                                               btnAdd.Enabled = true;
                                           
                                            }
                                        }
                                    }
                                }
                            }

                        }));

                        //this.richTextBox1.Invoke((Action)(() => MessageBox.Show(_lastDbB)));
                    }

                }
            }
        }
        private void LoadSelectedDatabases()
        {
            if (!File.Exists(selectedDbPath))
                return;

            XmlSerializer s = new XmlSerializer(typeof(List<database>));
            using (StringReader reader = new StringReader(System.IO.File.ReadAllText(selectedDbPath)))
            {
                object obj = s.Deserialize(reader);
                this.SelectedDatabases = (List<database>)obj;
            }
        }
        private void loadServers()
        {
            log = Serializ.DeSerialize(FilePath);

            if (log != null && log.Count > 0)
            {
                treeView1.Nodes.Clear();
                foreach (Login l in log)
                {


                    TreeNode node = treeView1.Nodes.Add(l.ServerName);


                    List<string> dbs = GetDatabaseList(l.ServerName, l.Username, l.Password);
                    foreach (string s in dbs)
                    {
                        TreeNode child = node.Nodes.Add(s);

                        //if (s.ToLower().Contains("perla"))
                        //{
                        //    child.ForeColor = Color.Red;
                        //}

                    }
                }

            }
        }
        public List<string> GetDatabaseList(string server, string usernme, string psw)
        {
            List<string> list = new List<string>();


            string conString = string.Format("Data Source={0};Persist Security Info=True; User Id={1};Password={2}", server, usernme, psw);

            using (SqlConnection con = new SqlConnection(conString))
            {
                con.Open();


                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases where name not in('master','msdb','tempdb','model')", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string name = dr[0].ToString();

                            list.Add(name);
                        }
                    }
                }
            }

            return list;

        }
        int tim = 60;
        private void timer1_Tick(object sender, EventArgs e)
        {
            tim--;
            this.Text = "Exiting app in " + tim.ToString() + " second(s)";
            if (!checkCloseOnFinis.Checked)
            {
                timer1.Enabled = false;
                this.Text = "HOSEIN Backup";
            }
            if (tim <= 0)
            {

                Application.Exit();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode!=null)
            {
     
                    SelectDatabase(treeView1.SelectedNode);
            
            }
        }
        private void SelectDatabase(TreeNode node)
        {

            if (node.Parent != null)
            {

                string server = node.Parent.Text;
                string db = node.Text;

                database b = SelectedDatabases.Find(d => d.Server == server);
                if (b != null)
                {

                    if (!b.Databases.Contains(db))
                        b.Databases.Add(db);

                }
                else
                {
                    b = new database();
                    b.Server = server;

                    List<string> dd = new List<string>();
                    dd.Add(db);
                    b.Databases = dd;

                    SelectedDatabases.Add(b);




                }
                RefreshSelected();
                SaveSelectedDatabasesToXml();
            }
        }
        private void SaveSelectedDatabasesToXml()
        {


            if (File.Exists(selectedDbPath))
            {
                File.Delete(selectedDbPath);
            }
            XmlSerializer serialiser = new XmlSerializer(typeof(List<database>));

            TextWriter Filestream = new StreamWriter(selectedDbPath);

            serialiser.Serialize(Filestream, SelectedDatabases);

            Filestream.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
          
           
            StartBAckupProcess();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (treeView2.SelectedNode != null)
            {

                TreeNode node = treeView2.SelectedNode;

                string server = node.Text;

                if (node.Parent == null)
                {
                    SelectedDatabases.Remove(SelectedDatabases.Find(a => a.Server == server));
                    node.Remove();
                }
                else
                {
                    TreeNode parent = node.Parent;
                    SelectedDatabases.Find(a => a.Server == parent.Text).Databases.Remove(node.Text);
                    node.Remove();
                    if (parent.Nodes.Count == 0)
                    {
                        SelectedDatabases.Remove(SelectedDatabases.Find(a => a.Server == parent.Text));
                        parent.Remove();
                    }



                }


            }
        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmAddLogins fl = new FrmAddLogins();
            DialogResult d = fl.ShowDialog();
            if (d == DialogResult.OK)
            {

                loadServers();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }

        private void checkCloseOnFinis_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.ExistOnFinish = checkCloseOnFinis.Checked;
            Settings.Default.Save();
        }

        private void checkBackupOnStart_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.BackupOnStart = checkBackupOnStart.Checked;
            Settings.Default.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSelectedDatabasesToXml();
        }
    }
}

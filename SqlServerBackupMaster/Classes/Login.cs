using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerBackupMaster.Classes
{
    public class Login
    {
        public string ServerName { get; set; }
        public string Username { get; set; }
        private string _username;
        public string Password
        {
            get;
            set;
        }
    }
}

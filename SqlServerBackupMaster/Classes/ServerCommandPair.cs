using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerBackupMaster.Classes
{
    public class ServerCommandPair
    {


        public string ConnectionString { get; set; }
        public string CmdString { get; set; }

        public ServerCommandPair(string ConnectionString, string CmdString)
        {
            this.ConnectionString = ConnectionString;
            this.CmdString = CmdString;
        }
        public ServerCommandPair()
        {

        }
    }
}

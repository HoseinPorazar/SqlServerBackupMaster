using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlServerBackupMaster.Classes
{
    public class database
    {
        public string Server { get; set; }
        public List<string> Databases { get; set; }
    }
}

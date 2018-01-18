using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SqlServerBackupMaster.Classes
{
    public static class Serializ
    {

        public static List<Login> DeSerialize(string path)
        {
            if (!File.Exists(path))
                return null;

            XmlSerializer s = new XmlSerializer(typeof(List<Login>));
            using (StringReader reader = new StringReader(System.IO.File.ReadAllText(path)))
            {
                object obj = s.Deserialize(reader);
                List<Login> tmp = (List<Login>)obj;
                foreach (Login l in tmp)
                {
                    l.Password = Encoding.UTF8.GetString(Convert.FromBase64String(l.Password));
                    l.Username = Encoding.UTF8.GetString(Convert.FromBase64String(l.Username));
                }
                return tmp;
            }
        }
        public static void Serialize(List<Login> logins, string path)
        {

            foreach (Login l in logins)
            {
                l.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(l.Password));
                l.Username = Convert.ToBase64String(Encoding.UTF8.GetBytes(l.Username));
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            XmlSerializer serialiser = new XmlSerializer(typeof(List<Login>));

            TextWriter Filestream = new StreamWriter(path);

            serialiser.Serialize(Filestream, logins);

            Filestream.Close();
        }
    }
}

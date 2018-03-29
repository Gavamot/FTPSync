using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    public class FtpSettings
    {
        public FtpSettings(string ip, string user, string password)
        {
            Ip = ip;
            User = user;
            Password = password;
        }

        public string Ip { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}

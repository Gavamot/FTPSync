using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Value;

namespace FtpSync.Entety
{
    class VideoReg
    {
        public int Id { get; set; }
        public int BrigadeCode { get; set; }

        // Настройки FTP
        public string Ip { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        //public FtpSettings FtpSettings => new FtpSettings(Ip, User, Password);

        // Каталоги для перекачки если null то незакачивать
        //public string ChannelFolder { get; set; }
        //public bool AutoLoadChannel { get; set; }

        //public string VideoFolder { get; set; }
        //public bool AutoLoadVideo { get; set; }
        //public IEnumerable<Camera> Cameras { get; set; }
    }
}

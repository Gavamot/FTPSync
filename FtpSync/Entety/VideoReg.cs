using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Каталоги для перекачки если null то незакачивать
        public string ChannelFolder { get; set; }
        public string VideoFolder { get; set; }
    }
}

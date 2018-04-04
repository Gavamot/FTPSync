using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync
{
    class Config
    {
        public string VideoFolder { get; set; } = "D:\\video";
        public string ChannelFolder { get; set; } = "D:\\channels";
        public string Host { get; set; } = "http://localhost:9000/";
        public int ChannelAutoDelayMs { get; set; } = 60000;
        public int VideoAutoDelayMs { get; set; } = 60000;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    public class BrigadeChannelValue
    {
        public DateTime? Actual { get; set; } = DateTime.MinValue;
        public List<DeviceChannelData> Channels { get; set; }
        public int BrigadeCode { get; set; }
        public DateTime? LastActual { get; set; } = DateTime.MinValue;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    public class BrigadeChannelValue
    {
        public DateTime Actual { get; set; }
        public List<ChannelValue> Channels { get; set; }
        public int BrigadeCode { get; set; }
      
    }
}

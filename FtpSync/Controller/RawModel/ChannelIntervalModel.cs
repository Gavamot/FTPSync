using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Value;
using NUnit.Framework.Constraints;

namespace FtpSync.Controller
{
    public class ChannelIntervalModel
    {
        public int BrigadeCode { get; set; }
        public DateInterval Interval { get; set; }
    }
}

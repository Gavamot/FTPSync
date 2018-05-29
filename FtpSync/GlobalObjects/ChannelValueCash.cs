using FtpSync.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtpSync
{
    public class ChannelValueCash
    {
        private static readonly ChannelValueCash instance = new ChannelValueCash();
        private ChannelValueCash() { }
        public static ChannelValueCash Instance => instance;
        volatile List<BrigadeChannelValue> Cash = new List<BrigadeChannelValue>();
        private object lockObj = new object();

        public BrigadeChannelValue Get(int brigadeCode)
        {
            var res = Cash.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
            return res;
        }

        public List<BrigadeChannelValue> GetAll()
        {
            var res = Cash.ToList();
            return res;
        }

        public void Set(BrigadeChannelValue val)
        {
            var cash = Cash;
            lock (lockObj)
            {
                var item = cash.FirstOrDefault(x=> x.BrigadeCode == val.BrigadeCode);
                if (item == null)
                {
                    cash.Add(val);
                }
                else
                {
                    item.LastActual = item.Actual;
                    item.Actual = val.Actual;
                    item.Channels = val.Channels;
                }
            }
        }
    }
}

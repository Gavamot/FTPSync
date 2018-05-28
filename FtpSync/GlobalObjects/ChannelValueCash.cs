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
        private object obj = new object();

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

        public void Set(int brigadeCode, DateTime actual, List<ChannelValue> channelValues)
        {
            var cash = Cash;
            lock (obj)
            {
                var item = cash.FirstOrDefault();
                if (item == null)
                {
                    item = new BrigadeChannelValue
                    {
                        BrigadeCode = brigadeCode,
                        Actual = actual,
                        Channels = channelValues
                    };
                    cash.Add(item);
                }
                else
                {
                    item.Actual = actual;
                    item.Channels = channelValues;
                }
            }
        }
    }
}

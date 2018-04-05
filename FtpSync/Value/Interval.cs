using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    public struct DateTimeInterval
    {
        public DateTimeInterval(DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start can not be more then end");
            Start = start;
            End = end;
        }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        public bool BitwinDate(DateTime dt)
        {
            return Start <= dt && End >= dt;
        }

        public static DateTimeInterval GetFullInterval()
        {
            return new DateTimeInterval(DateTime.MinValue, DateTime.MaxValue);
        }

        public static bool operator == (DateTimeInterval interval1, DateTimeInterval interval2)
        {
            return 
                interval1.Start == interval2.Start &&
                interval1.End == interval2.End;
        }

        public static bool operator !=(DateTimeInterval interval1, DateTimeInterval interval2)
        {
            return !(interval1 == interval2);
        }

        public override string ToString()
        {
            return $"{Start.ToNormalString()} - {End.ToNormalString()}";
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    public struct DateInterval
    {
        public DateInterval(DateTime start, DateTime end)
        {
            if(start > end)
                throw new ArgumentException("Start can not be more then end");
            Start = start;
            End = end;
        }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public bool BitwinDate(DateTime dt)
        {
            return Start <= dt && End >= dt;
        }

        public bool BitwinDateFolder(Folder f)
        {
            return Start.RoundToHour() <= f.YyyyMMddHH 
                && End.RoundToEndHour() >= f.YyyyMMddHH;
        }

        public static DateInterval GetFullInterval()
        {
            return new DateInterval(DateTime.MinValue, DateTime.MaxValue);
        }

        public static bool operator == (DateInterval interval1, DateInterval interval2)
        {
            return 
                interval1.Start == interval2.Start &&
                interval1.End == interval2.End;
        }

        public static bool operator !=(DateInterval interval1, DateInterval interval2)
        {
            return !(interval1 == interval2);
        }

        public override string ToString()
        {
            return $"{Start.ToNormalString()} - {End.ToNormalString()}";
        }
    }
}

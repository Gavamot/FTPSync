using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync
{
    static class DateExt
    {
        public const string DefDateFormat = "yyyy/M/dTHH:mm:ss";
 
        public static DateTime ToDate(this string self, string format = DefDateFormat)
        {
            return DateTime.ParseExact(self, format, CultureInfo.CurrentCulture);
        }

        public static string ToNormalString(this DateTime self)
        {
            return self.ToString("yyyy/M/d HH:mm:ss");
        }

        public static DateTime RoundToHour(this DateTime self)
        {
            return new DateTime(self.Year, self.Month, self.Day,
                                 self.Hour, 0, 0, self.Kind);
        }
    }
}

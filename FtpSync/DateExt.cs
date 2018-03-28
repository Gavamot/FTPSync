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

        public const string DefDateFormet = "yyyy-MM-ddTHH:mm:ss";

        public static DateTime ToDate(this string self, string format = DefDateFormet)
        {
            return DateTime.ParseExact(self, format, CultureInfo.InvariantCulture);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;

namespace FtpSync.Value
{
    class Folder
    {
        public DateTime YyyyMMddHH { get; set; }
        public FtpListItem File { get; set; }

        public bool BitwinDate(DateTime start, DateTime end)
        {
            return start <= YyyyMMddHH && end >= YyyyMMddHH;
        }

        public static Folder Create(FtpListItem f)
        {
            var res = new Folder();
            res.File = f;
            string[] str = f.FullName.Split('/');
            int yyyy = int.Parse(str[str.Length - 4]);
            int MM = int.Parse(str[str.Length - 3]);
            int dd = int.Parse(str[str.Length - 2]);
            int HH = int.Parse(str[str.Length - 1]);
            res.YyyyMMddHH = new DateTime(yyyy, MM, dd, HH, 0, 0);
            return res;
        }
    }
}

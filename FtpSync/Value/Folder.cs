using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentFTP;
using MoreLinq;

namespace FtpSync.Value
{
    static class FolderExt
    {
        public static DirectoryInfo GetFolderWithMaxValue(this DirectoryInfo self)
        {
            return self
                .GetDirectories()
                .MaxBy(x => int.Parse(x.Name));
        }

        public static FtpListItem GetFolderWithMaxValue(this FtpListItem self, FtpClient client)
        {
            return client
                .GetListing(self.FullName)
                .MaxBy(x => int.Parse(x.Name));
        }
    }

    class Folder
    {
        public DateTime YyyyMMddHH { get; protected set; }
        public string Path { get; protected set; }
    
        public virtual void SetPath(string path)
        {
            Path = path;
            YyyyMMddHH = Parce(Path);
        }

        protected static DateTime Parce(string path)
        {
            string[] str = path.Replace('/', '\\').Split('\\');
            int yyyy = int.Parse(str[str.Length - 4]);
            int MM = int.Parse(str[str.Length - 3]);
            int dd = int.Parse(str[str.Length - 2]);
            int HH = int.Parse(str[str.Length - 1]);
            return new DateTime(yyyy, MM, dd, HH, 0, 0);
        }

        public override string ToString()
        {
            return Path;
        }
    }
}

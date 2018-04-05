using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace FtpSync.Value
{
    class LocalFolder : Folder
    {
        public DirectoryInfo Dir { get; protected set; }

        public override void SetPath(string path)
        {
            path = path.Replace("/", "\\");
            base.SetPath(path);
            Dir = new DirectoryInfo(Path);
        }

        /// <summary>
        /// Поиск даты максимального файла 
        /// </summary>
        /// <param name="root">Корневой католог с годами</param>
        /// <returns>дата максимального файла </returns>
        public static DateTime? GetMaxDate(string root)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(root)
                    .GetFolderWithMaxValue()  // yyyy
                    .GetFolderWithMaxValue()  // MM
                    .GetFolderWithMaxValue()  // dd
                    .GetFolderWithMaxValue(); // HH
                DateTime res = Parce(dir.FullName);
                return res;
            }
            catch(InvalidOperationException e)
            {
                return null;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}

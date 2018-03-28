using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using FluentFTP;
using NUnit.Framework;

namespace FtpSync.Value
{
    class RemoteFolder
    {
        public DateTime YyyyMMddHH { get; set; }
        public FtpListItem File { get; set; }

        public bool BitwinDate(DateTime start, DateTime end)
        {
            return start <= YyyyMMddHH && end >= YyyyMMddHH;
        }

        public string GetLocalPath(string root, int brigadeCode)
        {
            return Path.Combine(
                root, 
                brigadeCode.ToString(), 
                YyyyMMddHH.Year.ToString(),
                YyyyMMddHH.Month.ToString(), 
                YyyyMMddHH.Day.ToString(),
                YyyyMMddHH.Hour.ToString()
            );
        }

        public static RemoteFolder Create(FtpListItem f)
        {
            var res = new RemoteFolder();
            res.File = f;
            string[] str = f.FullName.Split('/');
            int yyyy = int.Parse(str[str.Length - 4]);
            int MM = int.Parse(str[str.Length - 3]);
            int dd = int.Parse(str[str.Length - 2]);
            int HH = int.Parse(str[str.Length - 1]);
            res.YyyyMMddHH = new DateTime(yyyy, MM, dd, HH, 0, 0);
            return res;
        }

        public override string ToString()
        {
            return File.FullName;
        }

        public static List<RemoteFolder> GetAllHoursFolders(FtpClient client, string folder, Func<RemoteFolder, bool> func = null)
        {
            var res = new List<RemoteFolder>();
            void AddFolders(FtpListItem f, int recurs)
            {
                if (f.Type != FtpFileSystemObjectType.Directory)
                    return;
                if (recurs == 0)
                {
                    RemoteFolder item = RemoteFolder.Create(f);
                    if (func == null)
                    {
                        res.Add(item);
                    }
                    else
                    {
                        if (func(item))
                            res.Add(item);
                    }
                }
                else
                {
                    foreach (FtpListItem y in client.GetListing(f.FullName))
                    {
                        AddFolders(y, recurs - 1);
                    }
                }
            }

            // 1    2  3  4
            // yyyy MM dd HH
            AddFolders(new FtpListItem { FullName = folder, Type = FtpFileSystemObjectType.Directory}, 4);
            return res;
        }


        [TestFixture]
        class Test : TestBase
        {
            const string Root = "C:\\channel";

            [Test]
            [TestCase(44, 2018, 4, 20, 22)]
            [TestCase(50, 2018, 5, 22, 23)]
            [TestCase(50, 2018, 3, 15, 0)]
            public void GetLocalPath(int brigadeCode, int yyyy, int MM, int dd, int HH)
            {
                var f = new RemoteFolder();
                f.YyyyMMddHH = new DateTime(yyyy, MM, dd, HH, 0, 0);
                string res = f.GetLocalPath(Root, brigadeCode);
                string actual = $@"{Root}\{brigadeCode}\{yyyy}\{MM}\{dd}\{HH}";
                AreEqual(res, actual);
            }

            [Test]
            public void Create()
            {
                var dt = new DateTime(2018, 1, 2, 3, 0, 0);
                var f = new FtpListItem();
                f.FullName = $"{Root}/{dt.Year}/{dt.Month}/{dt.Day}/{dt.Hour}";
                var actual = new RemoteFolder
                {
                    File = f,
                    YyyyMMddHH = dt
                };
                var res = RemoteFolder.Create(f);
                AreEqual(actual, res);
            }
        }
    }
}

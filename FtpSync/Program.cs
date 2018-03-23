using FluentFTP;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using Newtonsoft.Json;
using NLog;

namespace FtpSync
{
    class Program
    {
        private const string PathConfig = "config.json";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly Config config = ReadConfig();

        static Config ReadConfig()
        {
            Config res;
            try
            {
                string json = File.ReadAllText(PathConfig);
                res = JsonConvert.DeserializeObject<Config>(json);
            }
            catch
            {
                res = new Config();
            }
            return res;
        }

        // Глобальный обработчик ошибок
        static void ProcessException(object sender, UnhandledExceptionEventArgs args)
        {
            var exception = (args.ExceptionObject as Exception)?.StackTrace;
            logger.Error(exception);
        }

        static void Main(string[] args)
        {
            Inject.SetDependenciesTest();
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            using (var db = new DataContext())
            {
                var a = db.VideoReg.Add(new VideoReg()
                {
                    BrigadeCode = 60,
                    Ip = "123",
                    User = "4",
                    Password = "12",
                    VideoFolder = "video",
                    ChannelFolder = "channel",
                    AutoLoadChannel = 1,
                    AutoLoadVideo = 2,
                    DT = DateTime.Now
                });
                db.SaveChanges();
            }

            using (var db = new DataContext())
            {
                db.VideoReg.Load();
                var a = db.VideoReg.Local.ToList();
                var aa = 1;
            }
            //Камера/Год/Месяц/День/Час

            //foreach (FtpListItem item in client.GetListing("/video"))
            //{
            //    Console.WriteLine(item.FullName);
            //    // if this is a file
            //    if (item.Type == FtpFileSystemObjectType.File)
            //    {
            //        // get the file size
            //        long size = client.GetFileSize(item.FullName);
            //    }

            //    // get modified date/time of the file or folder
            //    DateTime time = client.GetModifiedTime(item.FullName);

            //   //calculate a hash for the file on the server side (default algorithm)
            //   // FtpHash hash = client.GetHash(item.FullName);

            //}

            Console.ReadKey();
        }


    }
}

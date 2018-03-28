using FluentFTP;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Real;
using FtpSync.Value;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NLog;
using NUnit.Framework;
using File = System.IO.File;

namespace FtpSync
{
    class Program
    {
        private const string PathConfig = "config.json";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public static readonly Config config = ReadConfig();

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

        static void DownloadFilesByInterval(int brigadeCode, DateTime start, DateTime end)
        {
            List<RemoteFolder> remoteFolders = RemoteFolder.GetAllHoursFolders(client, "/channels", folder => folder.BitwinDate(start, end));
            foreach (RemoteFolder remoteFolder in remoteFolders)
            {
                //Console.WriteLine($"{remoteFolder}   |    {remoteFolder.YyyyMMddHH:yyyy-MM-dd HH:mm:ss}");
                string localFolder = remoteFolder.GetLocalPath(config.ChannelFolder, brigadeCode);
                //Console.WriteLine($"localFolder = {localFolder}");
               // client.DownloadFiles(localFolder, client.GetListing(remoteFolder.ToString()).Select(x => x.FullName));
                foreach (FtpListItem remoteFile in client.GetListing(remoteFolder.ToString()))
                {
                    if (remoteFile.Type == FtpFileSystemObjectType.File)
                    {
                        client.DownloadFile(Path.Combine(localFolder, remoteFile.Name), remoteFile.FullName);
                        Console.WriteLine(remoteFile.FullName);
                    }
                }
            }
        }

        private static FtpClient client;
        // Получить все катологи  
        static void Main(string[] args)
        {
            //var a = new FtpListItem("/channel");
            //Inject.SetDependenciesTest();
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);

            client = new FtpClient("192.168.1.158");
            // if you don't specify login credentials, we use the "anonymous" user account
            client.Credentials = new NetworkCredential("oem", "123");
            // begin connecting to the server
            client.Connect();

            DateTime start = new DateTime(2018, 5, 2, 17, 0, 0);
            DateTime end = new DateTime(2018, 5, 2, 18, 0, 0);
            int brigadeCode = 123;

            var video = new VideoReg()
            {
                Ip = "192.168.1.158",
                Password = "123",
                User = "oem",
                BrigadeCode = 123,
                ChannelFolder = "/channels"
            };
            //var ftp = new FtpChannelsLoader(video.FtpSettings, video.BrigadeCode, video.ChannelFolder);
            
            //DownloadByInterval(brigadeCode, start, end);

           // client.DownloadFile(@"C:\MyVideo_2.mp4", "/htdocs/big2.txt");


            // Год
            //foreach (FtpListItem y in client.GetListing("/channels"))
            //{
            //    if (y.Type == FtpFileSystemObjectType.Directory)
            //    {
            //        // Месяц
            //        foreach (FtpListItem m in client.GetListing(y.FullName))
            //        {
            //            if (m.Type == FtpFileSystemObjectType.Directory)
            //            {
            //                // Месяц
            //                foreach (FtpListItem d in client.GetListing(m.FullName))
            //                {
            //                    if (d.Type == FtpFileSystemObjectType.Directory)
            //                    {
            //                        // Месяц
            //                        foreach (FtpListItem h in client.GetListing(d.FullName))
            //                        {
            //                            if (h.Type == FtpFileSystemObjectType.Directory)
            //                            {
            //                                Console.WriteLine(h.FullName);
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            Console.ReadKey();
        }

        // Каскадное удалене
        // Извлечь нужного покупателя из таблицы вместе с заказами
        //Customer customer = context.Customers
        //        .Include(c => c.Orders)
        //        .FirstOrDefault(c => c.FirstName == "Василий");

        //    // Удалить этого покупателя
        //    if (customer != null)
        //{
        //    context.Customers.Remove(customer);
        //    context.SaveChanges();
        //}

    static void TestWebApi()
        {
            logger.Info($"HOST {config.Host} was starting...");
            var client = new HttpClient();
            var response = client.GetAsync(config.Host + "api/channel").Result;

            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
        }

        static void TestOrm()
        {
            using (var db = new DataContext())
            {
                db.Camera.Load();
                db.VideoReg.Load();
                foreach (var vid in db.VideoReg.Local)
                {
                    Console.WriteLine(vid.ToString());
                }
                var v = new VideoReg()
                {
                    BrigadeCode = 60,
                    Ip = "123",
                    User = "4",
                    Password = "12",
                    VideoFolder = "video",
                    ChannelFolder = "channel",
                    ChannelAutoLoad = 1,
                    AutoLoadVideo = 2,
                    ChannelTimeStamp = DateTime.Now
                };

                var c1 = new Camera
                {
                    VideoReg = v,
                    Num = 1,
                    VideoRegId = 1,
                    TimeStamp = DateTime.Now,
                };

                var c2 = new Camera
                {
                    VideoReg = v,
                    Num = 2,
                    VideoRegId = 1,
                    TimeStamp = DateTime.Now
                };
                v.Camers = new List<Camera> { c1, c2 };
                db.VideoReg.Add(v);
                db.SaveChanges();
            }

            Console.WriteLine("----------------------------------------------------");

            using (var db = new DataContext())
            {
                db.Camera.Load();
                db.VideoReg.Load();
                foreach (var v in db.VideoReg.Local)
                {
                    Console.WriteLine(v.ToString());
                }

            }
        }


    }
}

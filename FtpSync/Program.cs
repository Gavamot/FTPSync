using FluentFTP;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
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


        // Получить все катологи  
        static List<Folder> GetAllFoldersInFolders(FtpClient client, string folder, Func<Folder, bool> func = null)
        {
            var res = new List<Folder>();
            void AddFolders(FtpListItem f, int recurs)
            {
                if (recurs == 0)
                {
                    Folder item = Folder.Create(f);

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
                        if (y.Type == FtpFileSystemObjectType.Directory)
                        {
                            AddFolders(y, recurs - 1);
                        }
                    }
                }
                
            }
            AddFolders(new FtpListItem{ FullName = folder }, 4);
            return res;
        }

        static void Main(string[] args)
        {
            //var a = new FtpListItem("/channel");
            //Inject.SetDependenciesTest();
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);

            FtpClient client = new FtpClient("192.168.88.11");
            // if you don't specify login credentials, we use the "anonymous" user account
            client.Credentials = new NetworkCredential("ftpuser", "123");
            // begin connecting to the server
            client.Connect();

            var f = new FtpListItem {FullName = "/channels/2018/2/14/10/2018.02.14T10.43.17_14_0_0_0.json" };
            Console.WriteLine(f.);
            var f2 = new FtpListItem { FullName = "/channels/2018/2/14/15/2018.02.14T15.47.37_14_0_0_0.json" };
            Console.WriteLine(f2.LinkCount);


            //DateTime start = new DateTime(2018, 2, 15, 10, 0, 0);
            //DateTime end = new DateTime(2018, 2, 16, 10, 0, 0);

            //foreach (var v in GetAllFoldersInFolders(client, "/channels", folder =>folder.BitwinDate(start, end) ))
            //{
            //    Console.WriteLine($"{v.File.FullName}   |    {v.YyyyMMddHH:yyyy-MM-dd HH:mm:ss}");
            //}




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

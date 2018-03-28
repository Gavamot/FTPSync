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

       
        // Получить все катологи  
        static void Main(string[] args)
        {
            //var a = new FtpListItem("/channel");
            //Inject.SetDependenciesTest();
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            // Первая инициализация для чтобы потом работало быстрее
            var t = ChannelTaskManager.Instance;
            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);

            //DateTime start = new DateTime(2018, 5, 2, 17, 0, 0);
            //DateTime end = new DateTime(2018, 5, 2, 18, 0, 0);

            //var video = new VideoReg
            //{
            //    Ip = "192.168.1.158",
            //    Password = "123",
            //    User = "oem",
            //    BrigadeCode = 123,
            //    ChannelFolder = "/channels"
            //};

            //var ftp = FtpLoader.Start(video.FtpSettings, video.BrigadeCode, video.ChannelFolder, config.ChannelFolder);
            //ftp.DownloadFilesByInterval(start, end);
            
            //DownloadByInterval(brigadeCode, start, end);
            // client.DownloadFile(@"C:\MyVideo_2.mp4", "/htdocs/big2.txt");
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
                var v = new VideoReg
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

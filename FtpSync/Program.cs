using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using FtpSync.Entety;
using FtpSync.TaskManager;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NLog;
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
                logger.Warn($"Config file [{PathConfig}] not found or haves the bad format.");
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

        // Установить культуру 
        static void SetDefaultCulture(CultureInfo culture)
        {
            Type type = typeof(CultureInfo);

            try
            {
                type.InvokeMember("s_userDefaultCulture",
                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    culture,
                    new object[] { culture });

                type.InvokeMember("s_userDefaultUICulture",
                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    culture,
                    new object[] { culture });
            }
            catch { }

            try
            {
                type.InvokeMember("m_userDefaultCulture",
                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    culture,
                    new object[] { culture });

                type.InvokeMember("m_userDefaultUICulture",
                    BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    culture,
                    new object[] { culture });
            }
            catch { }
        }

        // Получить все катологи  
        static void Main(string[] args)
        {
            //  Глобальный обработчик ошибок
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            // Установить культуру 
            var culture = new CultureInfo("ru-RU");
            culture.DateTimeFormat.FullDateTimePattern = DateExt.DefDateFormat;
            culture.NumberFormat.NumberDecimalSeparator = ".";
            SetDefaultCulture(culture);

            StartAuto();

            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);
            logger.Info($"Wep Api 2 started on host {config.Host}");

            while (true)
            {
                Console.ReadKey();
            }
        }

       

        static void StartAuto()
        {
            using (var db = new DataContext())
            {
                foreach (var reg in db.VideoReg)
                {
                    if (reg.ChannelAutoLoad == 1)
                    {
                        AutoLoadChannelTaskManager.Instance.OnAutoload(reg.BrigadeCode);
                    }
                }

                foreach (var cam in db.Camera)
                {
                    if (cam.AutoLoadVideo == 1)
                    {
                        AutoLoadVideoTaskManager.Instance.OnAutoload(cam.VideoReg.BrigadeCode, cam.Num);
                    }
                }
              
            }
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
                var v = new VideoReg
                {
                    BrigadeCode = 60,
                    Ip = "123",
                    User = "4",
                    Password = "12",
                    VideoFolder = "video",
                    ChannelFolder = "channel",
                    ChannelAutoLoad = 1,
                    ChannelTimeStamp = DateTime.Now
                };

                var c1 = new Camera
                {
                    VideoReg = v,
                    Num = 1,
                    VideoRegId = 1,
                    AutoLoadVideo = 0,
                    TimeStamp = DateTime.Now,
                };

                var c2 = new Camera
                {
                    VideoReg = v,
                    Num = 2,
                    AutoLoadVideo = 0,
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

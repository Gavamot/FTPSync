using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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
        static void SetDefaultCulture()
        {
            // Установить культуру 
            var culture = new CultureInfo("ru-RU");
            culture.DateTimeFormat.FullDateTimePattern = DateExt.DefDateFormat;
            culture.NumberFormat.NumberDecimalSeparator = ".";

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

            //try
            //{
            //    type.InvokeMember("m_userDefaultCulture",
            //        BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
            //        null,
            //        culture,
            //        new object[] { culture });

            //    type.InvokeMember("m_userDefaultUICulture",
            //        BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
            //        null,
            //        culture,
            //        new object[] { culture });
            //}
            //catch { }
        }

        // Запуск служб автоподкачки
        static void StartAuto()
        {
            StartAutoChannel();
            StartAutoVideo();
            Thread.Sleep(500);
        }

        // Получить все катологи  
        static void Main(string[] args)
        {
            SetDefaultCulture();

            //  Глобальный обработчик ошибок
            AppDomain.CurrentDomain.UnhandledException += ProcessException;

            // Запуск служб автоподкачки
            StartAuto();

            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);
            logger.Info($"Wep Api 2 started on host {config.Host}");

            while (true)
            {
                Console.ReadKey();
            }
        }

        static void StartAutoChannel()
        {
            logger.Info("Start auto loading channel");
            Task.Factory.StartNew(() =>
            {
                using (var db = new DataContext())
                {
                    Parallel.ForEach(db.VideoReg, reg =>
                    {
                        // Автодокачка каналов
                        if (reg.ChannelAutoLoad == AutoLoadStatus.on)
                        {
                            AutoLoadChannelTaskManager.Instance.OnAutoload(reg.BrigadeCode);
                        }
                        // Автодокачка текущих значений
                        if (reg.UpdateChannelValues == AutoLoadStatus.on)
                        {
                            DeviceDataTaskManager.Instance.SetOn(reg);
                        }
                    });
                }
            });
        }

        static void StartAutoVideo()
        {
            logger.Info("Start auto loading video");
            Task.Factory.StartNew(() =>
            {
                using (var db = new DataContext())
                {
                    Parallel.ForEach(db.Camera.Include(x=>x.VideoReg).ToList(), cam =>
                    {
                        if (cam.AutoLoadVideo == AutoLoadStatus.on)
                        {
                            int brigadeCode = cam.VideoReg.BrigadeCode;
                            AutoLoadVideoTaskManager.Instance.OnAutoload(brigadeCode, cam.Num);
                        }
                    });
                }
            });
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
                    ChannelAutoLoad = AutoLoadStatus.on,
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

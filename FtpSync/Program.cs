using FluentFTP;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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
            string a = culture.DateTimeFormat.FullDateTimePattern;
            culture.DateTimeFormat.FullDateTimePattern = DateExt.DefDateFormat;

            culture.NumberFormat.NumberDecimalSeparator = ".";
            SetDefaultCulture(culture);
           
            // Запускаем web Api 2
            var host = WebApp.Start<Startup>(config.Host);
            logger.Info($"Wep Api 2 started on host {config.Host}");

            while (true)
            {
                Console.ReadKey();
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

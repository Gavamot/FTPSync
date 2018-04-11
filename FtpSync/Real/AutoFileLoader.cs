using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FtpSync.Real.File;
using FtpSync.Value;
using MoreLinq;
using NLog;

namespace FtpSync.Real
{
    interface IIntervalManager
    {
        string LocalDir { get; }
        DateInterval GetTimeStamp();
        void SetTimeStamp(DateTime timestamp);
    }

    class ChannelIntervalManager : IIntervalManager
    {
        public ChannelIntervalManager(int brigadeCode)
        {
            BrigadeCode = brigadeCode;
        }

        public int BrigadeCode { get; set; }

        public string LocalDir => Path.Combine(Program.config.ChannelFolder, BrigadeCode.ToString());

        public DateInterval GetTimeStamp()
        {
            DateTime? timeStamp = null;

            // Берем из базы
            using (var db = new DataContext())
            {
                timeStamp = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == BrigadeCode)?.ChannelTimeStamp;
            }

            // Если нету в базе ищем в папке
            if (timeStamp == null)
            {
                timeStamp = LocalFolder.GetMaxDate(LocalDir);
            }

            var res = new DateInterval(timeStamp ?? DateTime.MinValue, DateTime.MaxValue);
            return res;
        }

        public void SetTimeStamp(DateTime timestamp)
        {
            using (var db = new DataContext())
            {
                var item = db.VideoReg.First(x => x.BrigadeCode == BrigadeCode);
                item.ChannelTimeStamp = timestamp;
                db.SaveChanges();
            }
        }
    }

    class VideoCamIntervalManager : IIntervalManager
    {
        public VideoCamIntervalManager(int brigadeCode, int cameraNum)
        {
            BrigadeCode = brigadeCode;
            CameraNum = cameraNum;
        }


        public int BrigadeCode { get; set; }
        public int CameraNum { get; set; }

        public string LocalDir => Path.Combine(Program.config.VideoFolder, BrigadeCode.ToString(), CameraNum.ToString());

        public DateInterval GetTimeStamp()
        {
            DateTime? timeStamp = null;
            using (var db = new DataContext())
            {
                timeStamp = db.Camera
                    .FirstOrDefault(x => x.VideoReg.BrigadeCode == BrigadeCode && x.Num == CameraNum)
                    ?.TimeStamp;
            }

            // Если нету в базе ищем в папке
            if (timeStamp == null)
            {
                timeStamp = LocalFolder.GetMaxDate(LocalDir);
            }

            var res = new DateInterval(timeStamp ?? DateTime.MinValue, DateTime.MaxValue);
            return res;
        }

        public void SetTimeStamp(DateTime timestamp)
        {
            using (var db = new DataContext())
            {
                var item = db.Camera.First(x => x.VideoReg.BrigadeCode == BrigadeCode && x.Num == CameraNum);
                item.TimeStamp = timestamp;
                db.SaveChanges();
            }
        }
    }

    class AutoFileLoader : IDisposable
    {
        private DateInterval intrerval = DateInterval.GetFullInterval();
        
        public IIntervalManager TimeStampManager { get; set; }
        private string localRoot { get; set; }
        private string remoteRoot { get; set; }
        private FtpLoader Loader { get; set; }
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void Load(CancellationTokenSource cts = null)
        {
            // Получаем метку из базы если ее нет в кэше
            if (intrerval == DateInterval.GetFullInterval()) 
                intrerval = TimeStampManager.GetTimeStamp();

            List<IFile> files = new List<IFile>();
            
            // Копируем файлы
            try
            {
                files = Loader.DownloadFilesByInterval(intrerval, remoteRoot, localRoot, cts);
            }
            catch (Exception e)
            {
                logger.Error(e, $"[{remoteRoot}] {e.Message}");
            }

            if (files.Any()) // Нет файлов
            {
                // Находим дату максимального файла
                DateTime pdt = files.Max(x => x.Pdt);
                pdt = pdt.RoundToHour();
                // Кэш временной метки
                intrerval = new DateInterval(pdt, DateTime.MaxValue);

                // Выставлем временную метку в базу данных
                TimeStampManager.SetTimeStamp(pdt);
            }
        }

        public static AutoFileLoader CreateChannelAutoLoader(int brigadeCode)
        {
            var res = new AutoFileLoader();
            res.localRoot = Path.Combine(Program.config.ChannelFolder, brigadeCode.ToString());
            res.TimeStampManager = new ChannelIntervalManager(brigadeCode);

            using (var db = new DataContext())
            {
                var reg = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
                res.remoteRoot = reg.ChannelFolder;
                res.Loader = new FtpLoader(reg.FtpSettings);
            }
            return res;
        }

        public static AutoFileLoader CreateVideoAutoLoader(int brigadeCode, int cameraNum)
        {
            var res = new AutoFileLoader();
            res.localRoot = Path.Combine(Program.config.VideoFolder, brigadeCode.ToString(), cameraNum.ToString());
            res.TimeStampManager =  new VideoCamIntervalManager(brigadeCode, cameraNum);

            using (var db = new DataContext())
            {
                var reg = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
                res.remoteRoot = Path.Combine(reg.VideoFolder, cameraNum.ToString());
                res.Loader = new FtpLoader(reg.FtpSettings);
            }
            return res;
        }

        public void Dispose()
        {
            Loader.Dispose();
        }
    }
}

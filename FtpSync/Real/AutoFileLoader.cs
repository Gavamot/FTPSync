using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FtpSync.Value;
using MoreLinq;

namespace FtpSync.Real
{
    interface IIntervalManager
    {
        DateTimeInterval GetTimeStamp();
        void SetTimeStamp(DateTime timestamp);
    }

    class ChannelIntervalManager : IIntervalManager
    {
        public int BrigadeCode { get; set; }

        public DateTimeInterval GetTimeStamp()
        {
            DateTime? timeStamp = null;
            using (var db = new DataContext())
            {
                timeStamp = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == BrigadeCode)?.ChannelTimeStamp;
            }
            var res = new DateTimeInterval(timeStamp ?? DateTime.MinValue, DateTime.MaxValue);
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
        public int BrigadeCode { get; set; }
        public int CameraNum { get; set; }

        public DateTimeInterval GetTimeStamp()
        {
            DateTime? timeStamp = null;
            using (var db = new DataContext())
            {
                timeStamp = db.Camera
                    .FirstOrDefault(x => x.VideoReg.BrigadeCode == BrigadeCode && x.Num == CameraNum)
                    ?.TimeStamp;
            }
            var res = new DateTimeInterval(timeStamp ?? DateTime.MinValue, DateTime.MaxValue);
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

    class AutoFileLoader
    {
        private DateTimeInterval intrerval = DateTimeInterval.GetFullInterval();
        
        public IIntervalManager TimeStampManager { get; set; }
        private string localRoot { get; set; }
        private string remoteRoot { get; set; }
        private FtpLoader Loader { get; set; }


        public void Load(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                // Получаем метку из базы если ее нет в кеше
                if (intrerval == DateTimeInterval.GetFullInterval())
                    intrerval = TimeStampManager.GetTimeStamp();

                // Копируем файлы
                var files = Loader.DownloadFilesByInterval(intrerval, remoteRoot, localRoot, token);
                if (!files.Any()) // Нет файлов
                    return;

                // Находим дату максимального файла
                DateTime pdt = files.Max(x => x.Pdt);

                // Кэш временной метки
                intrerval = new DateTimeInterval(pdt, DateTime.MaxValue);

                // Выставлем временную метку в базу данных
                TimeStampManager.SetTimeStamp(pdt);

                Task.Delay(Program.config.ChannelAutoDelayMs, token);
            }
        }

        public static AutoFileLoader CreateChannelAutoLoader(int brigadeCode)
        {
            string localRoot = Path.Combine(Program.config.ChannelFolder, brigadeCode.ToString());
            var res = new AutoFileLoader();
            res.localRoot = localRoot;
            res.TimeStampManager = new ChannelIntervalManager();

            using (var db = new DataContext())
            {
                var reg = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
                res.remoteRoot = reg.ChannelFolder;
            }

            return res;
        }

        public static AutoFileLoader CreateVideoAutoLoader(int brigadeCode, int cumeraNum)
        {
            string localRoot = Path.Combine(Program.config.ChannelFolder, brigadeCode.ToString(), cumeraNum.ToString());
            var res = new AutoFileLoader();
            res.localRoot = localRoot;
            res.TimeStampManager = new ChannelIntervalManager();

            using (var db = new DataContext())
            {
                var reg = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
                res.remoteRoot = Path.Combine(reg.VideoFolder, cumeraNum.ToString());
            }

            return res;
        }

    }
}

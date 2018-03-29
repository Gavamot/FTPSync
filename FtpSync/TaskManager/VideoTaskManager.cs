using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Real;
using FtpSync.Value;
using NLog;

namespace FtpSync
{
    class VideoTaskManager
    {
        class ChannelTask
        {
            public int BrigadeCode { get; set; }
            public DateTimeInterval Interval { get; set; }
            public Task Task { get; set; }

        }

        private static readonly VideoTaskManager instance = new VideoTaskManager();
        private VideoTaskManager() { }
        public static VideoTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<ChannelTask> tasks = new List<ChannelTask>();
        private readonly string videoFolder = Program.config.VideoFolder;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public bool SyncChannelsByPeriod(VideoReg video, DateTimeInterval interval)
        {
            //ChannelTask newTask = new ChannelTask
            //{
            //    BrigadeCode = video.BrigadeCode,
            //    Interval = interval,
            //    Task = new Task(() =>
            //    {
            //        // Загружаем данные за необходимый интревал
            //        var ftp = FtpLoader.Start(video.FtpSettings, video.BrigadeCode, video.ChannelFolder, videoFolder);
            //        ftp.DownloadFilesByInterval(interval);

            //        // Сннимаем задачу из списка задач
            //        lock (tasksLock)
            //        {
            //            tasks.RemoveAll(t =>
            //                t.BrigadeCode == video.BrigadeCode &&
            //                t.Interval == interval);
            //        }
            //    })
            //};
            //lock (tasksLock)
            //{
            //    // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
            //    ChannelTask oldTask = tasks.FirstOrDefault(x =>
            //        x.BrigadeCode == video.BrigadeCode &&
            //        x.Interval == interval);
            //    if (oldTask != null)
            //    {
            //        logger.Info($"SyncChannelsByPeriod({video.BrigadeCode}, {interval}) [EXECUTION-MISS]");
            //        return false;
            //    }
            //    // Ставим задачу на выполнение
            //    tasks.Add(newTask);
            //}

            //newTask.Task.Start();
            //logger.Info($"SyncChannelsByPeriod({video.BrigadeCode}, {interval}) [EXECUTION]");
            return true;
        }
    }
}

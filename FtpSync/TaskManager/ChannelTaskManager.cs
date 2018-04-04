using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Real;
using FtpSync.Value;
using NLog;

namespace FtpSync
{
    class ChannelTaskManager
    {
        class ChannelTask
        {
            public int BrigadeCode { get; set; }
            public DateTimeInterval Interval { get; set; }
            public Task Task { get; set; }
            public CancellationTokenSource Cts { get; set; }
        }

        private static readonly ChannelTaskManager instance = new ChannelTaskManager();
        private ChannelTaskManager() { }
        public static ChannelTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<ChannelTask> tasks = new List<ChannelTask>();
        private readonly string channelFolder = Program.config.ChannelFolder;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool SyncChannelsByPeriod(VideoReg video, DateTimeInterval interval)
        {
            var cts = new CancellationTokenSource();
            var task = new Task(() =>
            {
                try
                {
                    // Загружаем данные за необходимый интревал
                    // video.BrigadeCode, video.ChannelFolder, channelFolder
                    var ftp = FtpLoader.Start(video.FtpSettings);
                    string localRoot = Path.Combine(channelFolder, video.BrigadeCode.ToString());
                    ftp.DownloadFilesByInterval(interval, video.ChannelFolder, localRoot, cts.Token);
                }
                catch (OperationCanceledException e)
                {
                    logger.Warn(e, $"{video.BrigadeCode}  [{interval}] operation canseled");
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }

                // Сннимаем задачу из списка задач
                lock (tasksLock)
                {
                    tasks.RemoveAll(t =>
                        t.BrigadeCode == video.BrigadeCode &&
                        t.Interval == interval);
                }
            }, cts.Token);

            ChannelTask newTask = new ChannelTask
            {
                BrigadeCode = video.BrigadeCode,
                Interval = interval,
                Task = task,
                Cts = cts 
            };

            lock (tasksLock)
            {
                // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
                ChannelTask oldTask = tasks.FirstOrDefault(x =>
                    x.BrigadeCode == video.BrigadeCode &&
                    x.Interval == interval);
                if (oldTask != null)
                {
                    logger.Info($"SyncChannelsByPeriod({video.BrigadeCode}, {interval}) [EXECUTION-MISS]");
                    return false;
                }
                // Ставим задачу на выполнение
                tasks.Add(newTask);
            }

            newTask.Task.Start();
            logger.Info($"SyncChannelsByPeriod({video.BrigadeCode}, {interval}) [EXECUTION]");
            return true;
        }
    }
}

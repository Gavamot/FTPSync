using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Real;
using NLog;

namespace FtpSync
{
    class ChannelTaskManager
    {
        class ChannelTask
        {
            public int BrigadeCode { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public Task Task { get; set; }
        }

        private static readonly ChannelTaskManager instance = new ChannelTaskManager();
        private ChannelTaskManager() { }
        public static ChannelTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<ChannelTask> tasks = new List<ChannelTask>();
        private readonly string channelFolder = Program.config.ChannelFolder;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public void SyncChannelsByPeriod(VideoReg video, DateTime start, DateTime end)
        {
            lock (tasksLock)
            {
                // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
                ChannelTask oldTask = tasks.FirstOrDefault(x =>
                    x.BrigadeCode == video.BrigadeCode &&
                    x.Start == start &&
                    x.End == end);
                if (oldTask == null)
                {
                    logger.Info($"SyncChannelsByPeriod({video.BrigadeCode},{start:yyyy-MM-dd HH:mm:ss},{end:yyyy-MM-dd HH:mm:ss}) [EXECUTION-MISS]");
                    return;
                }

                ChannelTask newTask = new ChannelTask
                {
                    BrigadeCode = video.BrigadeCode,
                    Start = start,
                    End = end,
                    Task = new Task(() =>
                    {
                        // Загружаем данные за необходимый интревал
                        var ftp = FtpLoader.Start(video.FtpSettings, video.BrigadeCode, video.ChannelFolder,
                            channelFolder);
                        ftp.DownloadFilesByInterval(start, end);

                    }).ContinueWith(x =>
                    {
                        // Сннимаем задачу из списка задач
                        lock (tasksLock)
                        {
                            tasks.RemoveAll(t =>
                                t.BrigadeCode == video.BrigadeCode &&
                                t.Start == start &&
                                t.End == end);
                        }
                    })
                };
                
                // Ставим задачу на выполнение
                tasks.Add(newTask);
                newTask.Task.Start();
                logger.Info($"SyncChannelsByPeriod({video.BrigadeCode},{start:yyyy-MM-dd HH:mm:ss},{end:yyyy-MM-dd HH:mm:ss}) [EXECUTION]");
            }
        }
    }
}

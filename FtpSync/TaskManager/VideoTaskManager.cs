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
using Newtonsoft.Json;
using NLog;

namespace FtpSync
{
    public class VideolTask
    {
        public int BrigadeCode { get; set; }

        public DateInterval Interval { get; set; }

        public int CameraNum { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }

        [JsonIgnore]
        public CancellationTokenSource Cts { get; set; }
    }

    class VideoTaskManager
    {
        private static readonly VideoTaskManager instance = new VideoTaskManager();
        private VideoTaskManager() { }
        public static VideoTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<VideolTask> tasks = new List<VideolTask>();
        private readonly string videoFolder = Program.config.VideoFolder;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public bool SyncChannelsByPeriod(VideoReg video, int cameraNum, DateInterval interval)
        {
            var cts = new CancellationTokenSource();

            var task = new Task(() =>
            {
                using(var ftp = FtpLoader.Start(video.FtpSettings))
                {
                    try
                    {
                        // Загружаем данные за необходимый интревал
                        string removeRoot = Path.Combine(video.VideoFolder, cameraNum.ToString());
                        string localRoot = Path.Combine(videoFolder, video.BrigadeCode.ToString(), cameraNum.ToString());
                        ftp.DownloadFilesByInterval(interval, removeRoot, localRoot, cts);
                    }
                    catch (OperationCanceledException e)
                    {
                        logger.Info(e, $"{video.BrigadeCode} ({cameraNum}) [{interval}] operation canseled");
                    }
                    catch (Exception e)
                    {
                        logger.Error(e);
                    }
                }

                // Сннимаем задачу из списка задач
                lock (tasksLock)
                {
                    tasks.RemoveAll((t =>
                        t.BrigadeCode == video.BrigadeCode &&
                        t.Interval == interval &&
                        t.CameraNum == cameraNum));
                }

            }, cts.Token);

            var newTask = new VideolTask
            {
                BrigadeCode = video.BrigadeCode,
                Interval = interval,
                CameraNum = cameraNum,
                Task = task,
                Cts = cts
            };

            lock (tasksLock)
            {
                // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
                VideolTask oldTask = tasks.FirstOrDefault(
                ( x =>
                    x.BrigadeCode == video.BrigadeCode &&
                    x.CameraNum == cameraNum &&
                    x.Interval == interval)
                );

                if (oldTask != null)
                {
                    logger.Info($"SyncCameraByPeriod({video.BrigadeCode}, {cameraNum}, {interval}) [EXECUTION-MISS]");
                    return false;
                }

                // Ставим задачу на выполнение
                tasks.Add(newTask);
                newTask.Task.Start();
            }
      
            logger.Info($"SyncCameraByPeriod({video.BrigadeCode}, {cameraNum}, {interval}) [EXECUTION]");
            return true;
        }

        public void CancelTask(Camera cam, DateInterval interval)
        {
            lock (tasksLock)
            {
                // Отменяем задачу
                var t = tasks.First(x => x.BrigadeCode == cam.VideoReg.BrigadeCode && x.Interval == interval);
                t.Cts.Cancel();

                // Удаляем задачу из списка
                tasks.Remove(t);
            }
        }

        public List<VideolTask> GetAll => tasks;   
    }
}

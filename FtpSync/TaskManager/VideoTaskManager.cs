﻿using System;
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
using NUnit.Framework.Constraints;

namespace FtpSync
{
    public class VideolTask
    {
        [JsonProperty("brigadeCode")]
        public int BrigadeCode { get; set; }

        [JsonProperty("interval")]
        public DateTimeInterval Interval { get; set; }

        [JsonProperty("cameraNum")]
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
        public bool SyncChannelsByPeriod(VideoReg video, int cameraNum, DateTimeInterval interval)
        {
            var cts = new CancellationTokenSource();
            var task = new Task((token) =>
            {
                try
                {
                    // Загружаем данные за необходимый интревал
                    var ftp = FtpLoader.Start(video.FtpSettings);
                    string removeRoot = Path.Combine(video.VideoFolder, cameraNum.ToString());
                    string localRoot = Path.Combine(videoFolder, video.BrigadeCode.ToString(), cameraNum.ToString());
                    ftp.DownloadFilesByInterval(interval, removeRoot, localRoot);
                }
                catch (OperationCanceledException e)
                {
                    logger.Warn(e, $"{video.BrigadeCode} ({cameraNum}) [{interval}] operation canseled");
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
                        t.Interval == interval &&
                        t.CameraNum == cameraNum);
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
                VideolTask oldTask = tasks.FirstOrDefault(x =>
                    x.BrigadeCode == video.BrigadeCode &&
                    x.CameraNum == cameraNum &&
                    x.Interval == interval);
                if (oldTask != null)
                {
                    logger.Info($"SyncCameraByPeriod({video.BrigadeCode}, {cameraNum}, {interval}) [EXECUTION-MISS]");
                    return false;
                }
                // Ставим задачу на выполнение
                tasks.Add(newTask);
            }

            newTask.Task.Start();
            logger.Info($"SyncCameraByPeriod({video.BrigadeCode}, {cameraNum}, {interval}) [EXECUTION]");
            return true;
        }

        public List<VideolTask> GetAll => tasks;
        
    }
}

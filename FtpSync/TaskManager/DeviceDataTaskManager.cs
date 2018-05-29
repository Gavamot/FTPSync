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
    public class DeviceDataTaskManager
    {
        public class DeviceDataTask
        {
            public int BrigadeCode { get; set; }
            [JsonIgnore]
            public Task Task { get; set; }
            [JsonIgnore]
            public CancellationTokenSource Cts { get; set; }
        }

        private static readonly DeviceDataTaskManager instance = new DeviceDataTaskManager();
        private DeviceDataTaskManager() { }
        public static DeviceDataTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<DeviceDataTask> tasks = new List<DeviceDataTask>();
        private readonly string filePath = Program.config.ValuesFilePath;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<DeviceDataTask> GetAll => tasks;

        public bool SetOn(VideoReg video)
        {
            var cts = new CancellationTokenSource();
            var task = new Task(() =>
            {
                logger.Info($"Task [{video.BrigadeCode}] autoupdate channel values was started");
                using (var ftp = FtpLoader.Start(video.FtpSettings))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        string strJson = ftp.DownloadFile(filePath);
                        var obj = JsonConvert.DeserializeObject<BrigadeChannelValue>(strJson);
                        ChannelValueCash.Instance.Set(obj);
                    }
                }           
                logger.Info($"Task [{video.BrigadeCode}] autoupdate channel values was canceled");
            }, cts.Token);

            DeviceDataTask newTask = new DeviceDataTask
            {
                BrigadeCode = video.BrigadeCode,
                Task = task,
                Cts = cts 
            };

            lock (tasksLock)
            {
                // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
                DeviceDataTask oldTask = tasks.FirstOrDefault(x => x.BrigadeCode == video.BrigadeCode);
                if (oldTask != null)
                {
                    return false;
                }
                // Ставим задачу на выполнение
                tasks.Add(newTask);
                newTask.Task.Start();
            }

            logger.Info($"OnAutoUpdateChannelValue({video.BrigadeCode}) [EXECUTION]");
            return true;
        }

        public void SetOff(int brigadeCode)
        {
            lock (tasksLock)
            {
                // Отменяем задачу
                var t = tasks.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                if (t == null)
                    return;
                t.Cts.Cancel();
                // Удаляем задачу из списка
                tasks.Remove(t);
            }
        }
    }
}

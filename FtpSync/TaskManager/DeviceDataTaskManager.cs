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
      
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public List<DeviceDataTask> GetAll => tasks;

        private void DeleteTask(int brigadeCode)
        {
            lock (tasksLock)
            {
                var task = tasks.First(x => x.BrigadeCode == brigadeCode);
                tasks.Remove(task);
            }
        }

        private void AddTask(DeviceDataTask task)
        {
            lock (tasksLock)
            {
                // Проверем выполняется ли в данный момент аналогичная задача если да то не надо ее дублировать
                DeviceDataTask oldTask = tasks.FirstOrDefault(x => x.BrigadeCode == task.BrigadeCode);
                if (oldTask != null)
                {
                    return;
                }
                // Ставим задачу на выполнение
                tasks.Add(task);
            }
        }
        public bool SetOn(VideoReg reg)
        {
            var cts = new CancellationTokenSource();
            // Задача постоянного обновления кеша данных с приборов 
            var task = new Task(() =>
            {
                logger.Info($"Task [{reg.BrigadeCode}] autoupdate device values was started");
                if (string.IsNullOrEmpty(reg.ValuesFile))
                {
                    logger.Info($"Task [{reg.BrigadeCode}] autoupdate device values. File name is uncorrect.");
                    DeleteTask(reg.BrigadeCode);
                    return;
                }
                using (var ftp = FtpLoader.Start(reg.FtpSettings))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        string strJson = "";
                        try
                        {
                            strJson = ftp.DownloadFile(reg.ValuesFile);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, $"Task [{reg.BrigadeCode}] autoupdate device values. Faild load file {reg.ValuesFile} [ERROR]");
                        }
                        BrigadeChannelValue obj = null;
                        try
                        {
                            obj = JsonConvert.DeserializeObject<BrigadeChannelValue>(strJson);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, $"Task [{reg.BrigadeCode}] autoupdate device values. Faild load file {reg.ValuesFile} [ERROR]");
                            return;
                        }
                       
                        if (obj.BrigadeCode != reg.BrigadeCode)
                        { 

                            logger.Error($"[IMPORTANT] Task [{reg.BrigadeCode}]  Database data not equals device brigadeCode [ERROR]");
                        }

                        DeviceDataCash.Instance.Set(obj);
                    }
                }
                // Если задача была отменена убераем ее с выполнения  
                DeleteTask(reg.BrigadeCode);
                logger.Info($"Task [{reg.BrigadeCode}] autoupdate device values was canceled");
            }, cts.Token);

            DeviceDataTask newTask = new DeviceDataTask
            {
                BrigadeCode = reg.BrigadeCode,
                Task = task,
                Cts = cts 
            };

            newTask.Task.Start();
            AddTask(newTask);
            logger.Info($"OnAutoUpdateChannelValue({reg.BrigadeCode}) [EXECUTION]");
            return true;
        }
        public void SetOff(int brigadeCode)
        {
            lock (tasksLock)
            {
                // Отменяем задачу
                var task = tasks.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                if (task == null)
                    return;
                task.Cts.Cancel();
                // Удаляем задачу из списка
                tasks.Remove(task);
            }
        }
    }
}

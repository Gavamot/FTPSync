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

namespace FtpSync.TaskManager
{
    public class AutoLoadChannelTaskManager
    {
        public class AutoLoadChannelTask
        {
            public int BrigadeCode { get; set; }

            [JsonIgnore]
            public Task Task { get; set; }

            [JsonIgnore]
            public CancellationTokenSource Cts { get; set; }
        }

        private static readonly AutoLoadChannelTaskManager instance = new AutoLoadChannelTaskManager();
        private AutoLoadChannelTaskManager() { }
        public static AutoLoadChannelTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<AutoLoadChannelTask> tasks = new List<AutoLoadChannelTask>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        // Установить значение автообновления в бд
        private void SetToDbAuto(int brigadeCode, AutoLoadStatus val)
        {
            using (var db = new DataContext())
            {
                var item = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
                item.ChannelAutoLoad = val;
                db.SaveChanges();
            }
        }

        public void OnAutoload(int brigadeCode)
        {
            lock (tasksLock)
            {
                if (tasks.Any(x => x.BrigadeCode == brigadeCode))
                    return;

                var t = new AutoLoadChannelTask();
                t.BrigadeCode = brigadeCode;
                var cts = new CancellationTokenSource();
                t.Cts = cts;

                t.Task = new Task(async ()=> {
                    logger.Info($"Task [{brigadeCode}] autoupdate channel was started");
                    using (var loader = AutoFileLoader.CreateChannelAutoLoader(brigadeCode))
                    {
                        while (!cts.IsCancellationRequested)
                        {
                            loader.Load(cts);
                            await Task.Delay(Program.config.ChannelAutoDelayMs, cts.Token);
                        }
                    }
                    lock (tasksLock)
                    {
                        // Удаляем задачу из списка
                        tasks.RemoveAll(x => x.BrigadeCode == brigadeCode);
                    }
                    logger.Info($"Task [{brigadeCode}] autoupdate channel was canseled");
                }, cts.Token);

                tasks.Add(t);
                t.Task.Start();
            }
            logger.Info($"{brigadeCode} [AUTO_CHANNEL_ON]");
        }

        public void SetOnAutoload(int brigadeCode)
        {
            // Установить значение в БД
            //SetToDbAuto(brigadeCode, AutoLoadStatus.on);

            OnAutoload(brigadeCode);
        }

        public void SetOffAutoload(int brigadeCode)
        {
            lock (tasksLock)
            {
                // Выключаем в базе
                //SetToDbAuto(brigadeCode, 0);

                // Отменяем задачу
                var t = tasks.First(x => x.BrigadeCode == brigadeCode);
                t.Cts.Cancel();

                // Удаляем задачу из списка
                tasks.Remove(t);
            }

            logger.Info($"{brigadeCode} [AUTO_CHANNEL_OFF)]");
        }

        public List<AutoLoadChannelTask> GetAll => tasks;
    }
}

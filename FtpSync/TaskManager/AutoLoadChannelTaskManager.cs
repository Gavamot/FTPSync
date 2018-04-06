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
    public class AutoLoadChannelTask
    {
        [JsonProperty("brigadeCode")]
        public int BrigadeCode { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }

        [JsonIgnore]
        public CancellationTokenSource Cts { get; set; }
    }

    class AutoLoadChannelTaskManager
    {
        private static readonly AutoLoadChannelTaskManager instance = new AutoLoadChannelTaskManager();
        private AutoLoadChannelTaskManager() { }
        public static AutoLoadChannelTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<AutoLoadChannelTask> tasks = new List<AutoLoadChannelTask>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void SetChannelAuto(int brigadeCode, AutoLoadStatus val)
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

                SetChannelAuto(brigadeCode, AutoLoadStatus.on);

                if (tasks.Any(x => x.BrigadeCode == brigadeCode) == false)
                {
                    var t = new AutoLoadChannelTask();
                    t.BrigadeCode = brigadeCode;
                    var cts = new CancellationTokenSource();
                    t.Cts = cts;
                    t.Task = new Task(async (token) =>
                    {
                        using (var loader = AutoFileLoader.CreateChannelAutoLoader(brigadeCode))
                        {
                            try
                            {
                                while (true)
                                {
                                    cts.Token.ThrowIfCancellationRequested();
                                    loader.Load((CancellationTokenSource)token);
                                    await Task.Delay(Program.config.ChannelAutoDelayMs, cts.Token);
                                }
                            }
                            catch (OperationCanceledException e)
                            {
                                logger.Info($"Task [{brigadeCode}] autoupdate channel was canseled");
                            }
                        }

                        lock (tasksLock)
                        {
                            // Удаляем задачу из списка
                            tasks.RemoveAll(x => x.BrigadeCode == brigadeCode);
                        }

                    }, cts);

                    tasks.Add(t);
                    t.Task.Start();
                }
            }
        }


        public void SetOnAutoload(int brigadeCode)
        {
            // Установить значение в БД
            SetChannelAuto(brigadeCode, AutoLoadStatus.on);

            lock (tasksLock)
            {
                // Задача уже выполняется
                if (tasks.Any(x => x.BrigadeCode == brigadeCode))
                    return;

                var t = new AutoLoadChannelTask();
                t.BrigadeCode = brigadeCode;
                var cts = new CancellationTokenSource();
                t.Cts = cts;
                t.Task = new Task(async (token) =>
                {
                    using (var loader = AutoFileLoader.CreateChannelAutoLoader(brigadeCode))
                    {
                        try
                        {
                            while (true)
                            {
                                cts.Token.ThrowIfCancellationRequested();
                                loader.Load((CancellationTokenSource)token);
                                await Task.Delay(Program.config.ChannelAutoDelayMs, cts.Token);
                            }
                        }
                        catch (OperationCanceledException e)
                        {
                            logger.Info($"Task [{brigadeCode}] autoupdate channel was canseled");
                        }
                    }

                    lock (tasksLock)
                    {
                        // Удаляем задачу из списка
                        tasks.RemoveAll(x => x.BrigadeCode == brigadeCode);
                    }

                }, cts);

                tasks.Add(t);
                t.Task.Start();
               
            }
        }

        public void SetOffAutoload(int brigadeCode)
        {
            lock (tasksLock)
            {
                // Выклычаем в базе
                SetChannelAuto(brigadeCode, 0);
                
                // Отменяем задачу
                tasks.ForEach( x => x.Cts.Cancel() );
                
                // Удаляем задачу из списка
                tasks.RemoveAll( x => x.BrigadeCode == brigadeCode );
            }
        }

        public List<AutoLoadChannelTask> GetAll => tasks;
    }
}

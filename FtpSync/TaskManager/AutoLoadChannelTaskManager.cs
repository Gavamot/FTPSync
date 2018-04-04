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

namespace FtpSync.TaskManager
{
    class AutoLoadChannelTaskManager
    {
        class AutoLoadChannelTask
        {
            public int BrigadeCode { get; set; }
            public Task Task { get; set; }
            public CancellationTokenSource Cts { get; set; }
        }

        private static readonly AutoLoadChannelTaskManager instance = new AutoLoadChannelTaskManager();
        private AutoLoadChannelTaskManager() { }
        public static AutoLoadChannelTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<AutoLoadChannelTask> tasks = new List<AutoLoadChannelTask>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void SetChannelAuto(int brigadeCode, int val)
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
                SetChannelAuto(brigadeCode, 1);

                if (tasks.Any(x => x.BrigadeCode == brigadeCode) == false)
                {
                    var t = new AutoLoadChannelTask();
                    t.BrigadeCode = brigadeCode;
                    var cts = new CancellationTokenSource();
                    t.Cts = cts;
                    t.Task = new Task(() =>
                    {
                        try
                        {
                            var loader = AutoFileLoader.CreateChannelAutoLoader(brigadeCode);
                            loader.Load(cts.Token);
                        }
                        catch (OperationCanceledException e)
                        {
                            logger.Info($"Task [{brigadeCode}] autoupdate channel was canseled");
                        }
                    }, cts.Token);
                    t.Task.Start();
                    tasks.Add(t);
                }
            }
        }

        public void OffAutoload(int brigadeCode)
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
    }
}

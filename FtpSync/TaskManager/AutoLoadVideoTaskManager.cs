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
    class AutoLoadVideoTaskManager
    {
        class AutoLoadVideoTask
        {
            public int BrigadeCode { get; set; }
            public int CameraNum { get; set; }
            public Task Task { get; set; }
            public CancellationTokenSource Cts { get; set; }
        }

        private static readonly AutoLoadVideoTaskManager instance = new AutoLoadVideoTaskManager();
        private AutoLoadVideoTaskManager() { }
        public static AutoLoadVideoTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<AutoLoadVideoTask> tasks = new List<AutoLoadVideoTask>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void SetChannelAuto(int brigadeCode, int cameraNum, int val)
        {
            using (var db = new DataContext())
            {
                var item = db.Camera.First(x => x.VideoReg.BrigadeCode == brigadeCode  && x.Num == cameraNum);
                item.AutoLoadVideo = val;
                db.SaveChanges();
            }
        }

        public void OnAutoload(int brigadeCode, int cameraNum)
        {
            lock (tasksLock)
            {
                SetChannelAuto(brigadeCode, 1, cameraNum);

                if (tasks.Any(x => x.BrigadeCode == brigadeCode && x.CameraNum == cameraNum) == false)
                {
                    var t = new AutoLoadVideoTask();
                    t.BrigadeCode = brigadeCode;
                    t.CameraNum = cameraNum;
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
                            logger.Info($"Task [{brigadeCode}] autoupdate video was canseled");
                        }
                    }, cts.Token);
                    t.Task.Start();
                    tasks.Add(t);
                }
            }
        }

        public void OffAutoload(int brigadeCode, int cameraNum)
        {
            lock (tasksLock)
            {
                // Выклычаем в базе
                SetChannelAuto(brigadeCode, 0, cameraNum);
                
                // Отменяем задачу
                tasks.ForEach( x => x.Cts.Cancel() );
                
                // Удаляем задачу из списка
                tasks.RemoveAll( x => x.BrigadeCode == brigadeCode );
            }
        }
    }
}

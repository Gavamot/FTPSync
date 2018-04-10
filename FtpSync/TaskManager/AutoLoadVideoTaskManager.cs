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
    public class AutoLoadVideoTask
    {
        [JsonProperty("brigadeCode")]
        public int BrigadeCode { get; set; }

        [JsonProperty("cameraNum")]
        public int CameraNum { get; set; }

        [JsonIgnore]
        public Task Task { get; set; }

        [JsonIgnore]
        public CancellationTokenSource Cts { get; set; }
    }

    class AutoLoadVideoTaskManager
    {
        private static readonly AutoLoadVideoTaskManager instance = new AutoLoadVideoTaskManager();
        private AutoLoadVideoTaskManager() { }
        public static AutoLoadVideoTaskManager Instance => instance;

        volatile object tasksLock = new object();
        volatile List<AutoLoadVideoTask> tasks = new List<AutoLoadVideoTask>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private void SetToDblAuto(int brigadeCode, int cameraNum, AutoLoadStatus val)
        {
            using (var db = new DataContext())
            {
                var item = db.Camera.First(x => x.VideoReg.BrigadeCode == brigadeCode && x.Num == cameraNum);
                item.AutoLoadVideo = val;
                db.SaveChanges();
            }
        }

        public void OnAutoload(int brigadeCode, int cameraNum)
        {
            lock (tasksLock)
            {
                if (tasks.Any(x => x.BrigadeCode == brigadeCode && x.CameraNum == cameraNum) == false)
                {
                    var t = new AutoLoadVideoTask();
                    t.BrigadeCode = brigadeCode;
                    t.CameraNum = cameraNum;
                    var cts = new CancellationTokenSource();
                    t.Cts = cts;
                     
                    t.Task = new Task(async (token) =>
                    {
                        using (var loader = AutoFileLoader.CreateChannelAutoLoader(brigadeCode))
                        {
                            try
                            {
                                logger.Info($"Task [{brigadeCode}({cameraNum})] autoupdate video was started");
                                while (true)
                                {
                                    cts.Token.ThrowIfCancellationRequested();
                                    loader.Load();
                                    await Task.Delay(Program.config.VideoAutoDelayMs, cts.Token);
                                }
                            }
                            catch (OperationCanceledException e)
                            {
                                logger.Info($"Task [{brigadeCode}] autoupdate video was canseled");
                            }
                        }
                    }, cts.Token);

                    t.Task.Start();
                    tasks.Add(t);
                }
            }
        }

        public void SetOnAutoload(int brigadeCode, int cameraNum)
        {
            // Установить значение в БД
            //SetToDblAuto(brigadeCode, cameraNum, AutoLoadStatus.on);

            OnAutoload(brigadeCode, cameraNum);
        }

        public void SetOffAutoload(int brigadeCode, int cameraNum)
        {
            lock (tasksLock)
            {
                // Выклычаем в базе
                //SetToDblAuto(brigadeCode, cameraNum, AutoLoadStatus.off);
                
                // Отменяем задачу
                var t = tasks.First(x=> x.BrigadeCode==brigadeCode && x.CameraNum==cameraNum);
                t.Cts.Cancel();

                // Удаляем задачу из списка
                tasks.Remove(t);
            }
        }

        public List<AutoLoadVideoTask> GetAll => tasks;
    }
}

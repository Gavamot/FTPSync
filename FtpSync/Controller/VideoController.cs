using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.TaskManager;

namespace FtpSync.Controller
{
    public class VideoController : MyController
    {
        [HttpGet]
        public List<VideolTask> GetTasks()
        {
            List<VideolTask> res = VideoTaskManager.Instance.GetAll;
            return res;
        }

        [HttpGet]
        public List<AutoLoadVideoTask> GetAutoLoadTasks()
        {
            List<AutoLoadVideoTask> res = AutoLoadVideoTaskManager.Instance.GetAll;
            return res;
        }

        private IHttpActionResult SetAuto(int brigadeCode, int cameraNum, AutoLoadStatus status)
        {
            switch (Camera.UpdateAuto(db, brigadeCode, cameraNum, status)) // Устанавливаем значение в БД
            {
                case UpdateEntetyStatus.notExist: return BadRequest("The camera not exsist.");
                case UpdateEntetyStatus.notUpdate: return BadRequest("Camera auto the value is the same.");
                case UpdateEntetyStatus.updated:
                    {
                        // Ставил либо снимаем задачу
                        if (status == AutoLoadStatus.on)
                            AutoLoadVideoTaskManager.Instance.SetOnAutoload(brigadeCode, cameraNum);
                        else
                            AutoLoadVideoTaskManager.Instance.SetOffAutoload(brigadeCode, cameraNum);
                        return Ok();
                    };
                default: return BadRequest("Unknown error");
            }
        }

        [HttpPost]
        public IHttpActionResult OnAuto([FromBody] CameraModel cam)
        {
            return SetAuto(cam.BrigadeCode, cam.CameraNum, AutoLoadStatus.on);
        }

        [HttpPost]
        public IHttpActionResult OffAuto([FromBody] CameraModel cam)
        {
            return SetAuto(cam.BrigadeCode, cam.CameraNum, AutoLoadStatus.off);
        }

        [HttpPost]
        public IHttpActionResult CancelTask([FromBody] VideoIntervalModel model)
        {
            using (db)
            {
                // Поиск видеорегистратора в базе
                var reg = db.Camera.Include(x=>x.VideoReg)
                    .FirstOrDefault( x => 
                        x.VideoReg.BrigadeCode == model.BrigadeCode && 
                        x.Num == model.CameraNum );

                if (reg == null)
                {
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                }

                // Выполнение операции
                VideoTaskManager.Instance.CancelTask(reg, model.Interval);
            }
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult SyncByPeriod([FromBody] VideoIntervalModel model)//(int brigadeCode, string start, string end)
        {
            using (db)
            {
                // Поиск видеорегистратора в базе
                var reg = db.VideoReg
                    .Include(x => x.Camers)
                    .FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
                if (reg == null)
                {
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                }

                // Выполнение операции
                if (VideoTaskManager.Instance.SyncChannelsByPeriod(reg, model.CameraNum, model.Interval))
                    return Ok();
                return BadRequest($"{model.BrigadeCode} cam={model.CameraNum}({model.Interval}) - [ALREADY EXECUTE]");
            }
        }

        [HttpPost]
        public IHttpActionResult SetTimeStamp([FromBody] TimeStampVideoModel model)
        {
            Camera cam;
            // Меняем временную метку
            using (db)
            {
                cam = db.Camera.FirstOrDefault(x =>
                    x.VideoReg.BrigadeCode == model.BrigadeCode &&
                    x.Num == model.Num);

                if (cam == null)
                    return BadRequest($"The video camera with brigadeCode={model.BrigadeCode} and num={ model.Num } was not found");

                cam.TimeStamp = model.TimeStamp;
                db.SaveChanges();
            }

            logger.Info($"BRIGADE={model.BrigadeCode} ({model.Num}) auto video timeStamp was setting {model.TimeStamp}");

            var camera = new CameraModel
            {
                CameraNum = model.Num,
                BrigadeCode = model.BrigadeCode
            };

            if (cam.AutoLoadVideo == AutoLoadStatus.on)
            {
                // Отключаем автообновление
                OffAuto(camera);
                // Включаем автообновление
                OnAuto(camera);
            }

            return Ok();
        }

    }
}

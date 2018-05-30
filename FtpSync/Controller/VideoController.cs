using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac.Integration.WebApi;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.Repositories;
using FtpSync.Repositories.Interfaces;
using FtpSync.TaskManager;

namespace FtpSync.Controller
{
    [AutofacControllerConfiguration]
    public class VideoController : MyController
    {
        readonly ICameraRep cameraRep;
        readonly IVideoRegRep regRep;

        public VideoController(ICameraRep cameraRep, IVideoRegRep videoRegRep)
        {
            this.cameraRep = cameraRep;
            this.regRep = videoRegRep;
        }

        [HttpGet]
        public List<VideoTaskManager.VideolTask> GetTasks()
        {
            List<VideoTaskManager.VideolTask> res = VideoTaskManager.Instance.GetAll;
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
            var val = cameraRep.UpdateAuto(brigadeCode, cameraNum, status);
            switch (val) // Устанавливаем значение в БД
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
            Camera cam = cameraRep.Get(model.BrigadeCode, model.CameraNum);
            if (cam == null)
                return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
            // Выполнение операции
            VideoTaskManager.Instance.CancelTask(cam, model.Interval);
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult SyncByPeriod([FromBody] VideoIntervalModel model)//(int brigadeCode, string start, string end)
        {
            VideoReg reg = regRep.Get(model.BrigadeCode);
            if (reg == null)
                return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
            // Выполнение операции
            if (VideoTaskManager.Instance.SyncChannelsByPeriod(reg, model.CameraNum, model.Interval))
                return Ok();
            return BadRequest($"{model.BrigadeCode} cam={model.CameraNum}({model.Interval}) - [ALREADY EXECUTE]");
        }

        [HttpPost]
        public IHttpActionResult SetTimeStamp([FromBody] TimeStampVideoModel model)
        {
            AutoLoadVideoTaskManager.Instance.SetOffAutoload(model.BrigadeCode, model.Num);
            // Устанавливаем метку в базе 
            var res = cameraRep.SetTimeStamp(model.BrigadeCode, model.Num, model.TimeStamp);
            if(res == UpdateEntetyStatus.notExist)
                return BadRequest($"BRIGADE={model.BrigadeCode} ({model.Num}) auto video timeStamp was setting {model.TimeStamp}");

            var cam = cameraRep.Get(model.BrigadeCode, model.Num);
            if (cam.AutoLoadVideo == AutoLoadStatus.on)
                AutoLoadVideoTaskManager.Instance.OnAutoload(model.BrigadeCode, model.Num);
            
            return Ok();
        }

    }
}

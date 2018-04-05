using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.TaskManager;

namespace FtpSync.Controller
{
    public class VideoController : MyController
    {
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
            using (db)
            {
                var cam = db.Camera.FirstOrDefault(x => 
                    x.VideoReg.BrigadeCode == model.BrigadeCode && 
                    x.Num == model.Num);

                if (cam == null)
                    return BadRequest($"The video camera with brigadeCode={model.BrigadeCode} and num={model.Num} was not found");

                cam.TimeStamp = model.TimeStamp;
                db.SaveChanges();
            }
            return Ok();
        }

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

    }

}

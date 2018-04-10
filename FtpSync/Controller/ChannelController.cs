using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.TaskManager;

namespace FtpSync.Controller
{
    public class ChannelController : MyController
    {
        [HttpGet]
        public List<ChannelTask> GetTasks()
        {
            List<ChannelTask> res = ChannelTaskManager.Instance.GetAll;
            return res;
        }

        [HttpGet]
        public List<AutoLoadChannelTask> GetAutoLoadTasks()
        {
            List<AutoLoadChannelTask> res = AutoLoadChannelTaskManager.Instance.GetAll;
            return res;
        }

        [HttpPost]
        public IHttpActionResult SyncByPeriod([FromBody] VideoIntervalModel model)//(int brigadeCode, string start, string end)
        {
            using (db)
            {
                // Поиск видеорегистратора в базе
                var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
                if (reg == null)
                {
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                }
                // Выполнение операции
                if (ChannelTaskManager.Instance.SyncChannelsByPeriod(reg, model.Interval))
                    return Ok();
                return BadRequest($"{model.BrigadeCode}({model.Interval}) - [ALREADY EXECUTE]");
            }
        }

        [HttpPost]
        public IHttpActionResult SetTimeStamp([FromBody] TimeStampChannelModel model)
        {
            VideoReg reg;
            using (db)
            {
                reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
                if (reg == null)
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                reg.ChannelTimeStamp = model.TimeStamp;
                db.SaveChanges();
            }

            logger.Info($"BRIGADE={model.BrigadeCode} auto channel timeStamp was setting {model.TimeStamp}");

            if (reg.ChannelAutoLoad == AutoLoadStatus.on)
            {
                OffAuto(model.BrigadeCode);
                OnAuto(model.BrigadeCode);
            }

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult CancelTask([FromBody] ChannelIntervalModel model)
        {
            using (db)
            {
                // Поиск видеорегистратора в базе
                var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
                if (reg == null)
                {
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                }
                // Выполнение операции
                ChannelTaskManager.Instance.CancelTask(reg, model.Interval);
            }

            return Ok();
        }

        private IHttpActionResult SetAuto(int brigadeCode, AutoLoadStatus status)
        {
            switch (VideoReg.UpdateChannelAuto(db, brigadeCode, status)) // Устанавливаем значение в БД
            {
                case UpdateEntetyStatus.NotExist: return BadRequest("The video registrator not exsist.");
                case UpdateEntetyStatus.NotUpdate: return BadRequest("Channel auto the value is the same.");
                case UpdateEntetyStatus.Updated:
                    {
                        // Ставил либо снимаем задачу
                        if(status == AutoLoadStatus.on)
                            AutoLoadChannelTaskManager.Instance.SetOnAutoload(brigadeCode); 
                        else
                            AutoLoadChannelTaskManager.Instance.SetOffAutoload(brigadeCode);
                        return Ok();
                    };
                default: return BadRequest("Unknown error");
            }
        }

        [HttpPost]
        public IHttpActionResult OnAuto([FromBody] int brigadeCode) 
        {
            return SetAuto(brigadeCode, AutoLoadStatus.on);
        }

        [HttpPost]
        public IHttpActionResult OffAuto([FromBody] int brigadeCode)
        {
            return SetAuto(brigadeCode, AutoLoadStatus.off);
        }

    }

}

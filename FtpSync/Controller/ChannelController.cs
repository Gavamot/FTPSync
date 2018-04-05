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
    public class ChannelController : MyController
    {
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
            using (db)
            {
                var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == model.BrigadeCode);
                if(reg == null)
                    return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
                reg.ChannelTimeStamp = model.TimeStamp;
                db.SaveChanges();
            }
            return Ok();
        }

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
    }

}

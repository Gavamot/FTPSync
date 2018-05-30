using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.Interface;
using FtpSync.Repositories.Interfaces;
using FtpSync.TaskManager;


namespace FtpSync.Controller
{
    public class ChannelController : MyController
    {
        readonly IVideoRegRep regRep;
        public ChannelController(IVideoRegRep videoRegRep)
        {
            this.regRep = videoRegRep;
        }

        [HttpGet]
        public List<ChannelTaskManager.ChannelTask> GetTasks()
        {
            List<ChannelTaskManager.ChannelTask> res = ChannelTaskManager.Instance.GetAll;
            return res;
        }

        [HttpGet]
        public List<AutoLoadChannelTaskManager.AutoLoadChannelTask> GetAutoLoadTasks()
        {
            List<AutoLoadChannelTaskManager.AutoLoadChannelTask> res = AutoLoadChannelTaskManager.Instance.GetAll;
            return res;
        }

        [HttpPost]
        public IHttpActionResult SyncByPeriod([FromBody] VideoIntervalModel model)
        {
            VideoReg reg = regRep.Get(model.BrigadeCode);
            if (reg == null)
            {
                return BadRequest($"The video registrator with brigadeCode={ model.BrigadeCode } was not found");
            }

            // Выполнение операции
            if (ChannelTaskManager.Instance.SyncChannelsByPeriod(reg, model.Interval))
                return Ok();

            return BadRequest($"{model.BrigadeCode}({model.Interval}) - [ALREADY EXECUTE]");
        }

        [HttpPost]
        public IHttpActionResult SetTimeStamp([FromBody] TimeStampChannelModel model)
        {
            UpdateEntetyStatus status = regRep.SetChannelTimeStamp(model.BrigadeCode, model.TimeStamp);

            if (status == UpdateEntetyStatus.notExist)
                BadRequest("Brigade was not found");

            var reg = regRep.Get(model.BrigadeCode);

            if (reg.ChannelAutoLoad == AutoLoadStatus.on)
            {
                AutoLoadChannelTaskManager.Instance.SetOffAutoload( model.BrigadeCode );
                AutoLoadChannelTaskManager.Instance.SetOnAutoload( model.BrigadeCode );
            }

            logger.Info($"BRIGADE={ model.BrigadeCode } auto channel timeStamp was setting { model.TimeStamp }");
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult CancelTask([FromBody] ChannelIntervalModel model)
        {
            VideoReg reg = regRep.Get(model.BrigadeCode);
            if (reg == null)
            {
                return BadRequest($"The video registrator with brigadeCode={model.BrigadeCode} was not found");
            }

            // Выполнение операции
            ChannelTaskManager.Instance.CancelTask(reg, model.Interval);
            return Ok();
        }

        private IHttpActionResult SetAuto(int brigadeCode, AutoLoadStatus status)
        {
            var val = VideoReg.UpdateChannelAuto(brigadeCode, status);
            switch (val) // Устанавливаем значение в БД
            {
                case UpdateEntetyStatus.notExist: return BadRequest("The video registrator not exsist.");
                case UpdateEntetyStatus.notUpdate: return BadRequest("Channel auto the value is the same.");
                case UpdateEntetyStatus.updated:
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

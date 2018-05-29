using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.TaskManager;


namespace FtpSync.Controller
{
    public class ChannelValuesController : MyController
    {
        [HttpGet]
        public List<DeviceDataTaskManager.DeviceDataTask> GetTasks()
        {
            List<DeviceDataTaskManager.DeviceDataTask> res = DeviceDataTaskManager.Instance.GetAll;
            return res;
        }

        [HttpPost]
        public IHttpActionResult OnAutoUpdate([FromBody] int brigadeCode)//(int brigadeCode, string start, string end)
        {
            VideoReg reg = null;
            using (var db = new DataContext())
            {
                // Поиск видеорегистратора в базе
                reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                if (reg == null)
                {
                    return BadRequest($"Brigade {brigadeCode} is not exsist");
                }
                reg.UpdateChannelValues = AutoLoadStatus.on;
                db.SaveChanges();
            }
            DeviceDataTaskManager.Instance.SetOn(reg);
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult OffAutoUpdat([FromBody] int brigadeCode)
        {
            DeviceDataTaskManager.Instance.SetOff(brigadeCode);
            return Ok();
        }

    }

}

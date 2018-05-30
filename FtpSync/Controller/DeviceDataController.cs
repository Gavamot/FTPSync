using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using FtpSync.Controller.RawModel;
using FtpSync.Entety;
using FtpSync.Repositories.Interfaces;
using FtpSync.TaskManager;
using FtpSync.Value;


namespace FtpSync.Controller
{
    public class DeviceDataController : MyController
    {
        readonly IVideoRegRep regRep;

        public DeviceDataController(IVideoRegRep videoRegRep)
        {
            this.regRep = videoRegRep;
        }

        [HttpGet]
        public List<DeviceDataTaskManager.DeviceDataTask> GetTasks()
        {
            List<DeviceDataTaskManager.DeviceDataTask> res = DeviceDataTaskManager.Instance.GetAll;
            return res;
        }

        [HttpGet]
        public List<BrigadeChannelValue> GetDeviceData()
        {
            var res = DeviceDataCash.Instance.GetAll();
            return res;
        }

        private IHttpActionResult SetAutoUpdate(int brigadeCode, AutoLoadStatus status)
        {
            var res = regRep.SetUpdateChannelValues(brigadeCode, status);
            if (res == UpdateEntetyStatus.updated)
            {
                var reg = regRep.Get(brigadeCode);
                if (status == AutoLoadStatus.on)
                {
                    DeviceDataTaskManager.Instance.SetOn(reg);
                }
                else
                {
                    DeviceDataTaskManager.Instance.SetOff(brigadeCode);
                }
                return Ok();
            }
            if (res == UpdateEntetyStatus.notUpdate)
            {
                return Ok();
            }
            return BadRequest("brigadeCode not found");
        }

        [HttpPost]
        public IHttpActionResult OnAutoUpdate([FromBody] int brigadeCode)
        {
            return SetAutoUpdate(brigadeCode, AutoLoadStatus.on);
        }

        [HttpPost]
        public IHttpActionResult OffAutoUpdat([FromBody] int brigadeCode)
        {
            var status = regRep.SetUpdateChannelValues(brigadeCode, AutoLoadStatus.off);
            DeviceDataTaskManager.Instance.SetOff(brigadeCode);
            return Ok();
        }

    }

}

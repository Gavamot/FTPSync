using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FtpSync.Controller
{
    public class ChannelController : ApiController
    {
        readonly DataContext db = new DataContext();

        [HttpPost]
        public void Sync(int brigadeCode, string start, string end)
        {
            DateTime s = start.ToDate();
            DateTime e = end.ToDate();
            var video = db.VideoReg.First(x => x.BrigadeCode == brigadeCode);
            ChannelTaskManager.Instance.SyncChannelsByPeriod(video, s, e);
        }

        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }

}

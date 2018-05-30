using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using FtpSync.Entety;
using FtpSync.Repositories.Interfaces;

namespace FtpSync.Controller
{
    public class VideoRegController : MyController
    {
        readonly IVideoRegRep regRep;
        public VideoRegController(IVideoRegRep videoRegRep)
        {
            this.regRep = videoRegRep;
        }

        [HttpPost]
        public IHttpActionResult Add(VideoReg videoReg)
        {
            regRep.Add(videoReg);
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Update(VideoReg videoReg)
        {
            var res = regRep.Update(videoReg);
            if (res == UpdateEntetyStatus.updated)
                return Ok();
            return BadRequest("Video registrator not found.");

        }

        [HttpPost]
        public IHttpActionResult Delete([FromBody]int brigadeCode)
        {
            var res = regRep.Delete(brigadeCode);
            if (res == UpdateEntetyStatus.updated)
                return Ok();
            return BadRequest("Video registrator not found.");
        }

        [HttpGet]
        public VideoReg Get(int brigadeCode)
        {
            var res = regRep.Get(brigadeCode);
            return res;
        }

        [HttpGet]
        public IEnumerable<VideoReg> GetAll()
        {
            var res = regRep.GetAll();
            return res;
        }
    }
}

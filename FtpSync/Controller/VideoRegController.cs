using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using FtpSync.Entety;

namespace FtpSync.Controller
{
    public class VideoRegController : MyController
    {
        [HttpPost]
        public IHttpActionResult Add(VideoReg videoReg)
        {
            using (db)
            {
                videoReg.Camers = new List<Camera>();
                for (int i = 0; i < 10; i++)
                {
                    var cam = new Camera {Num = i};
                    videoReg.Camers.Add(cam);
                }
                db.VideoReg.Add(videoReg);
                db.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Update(VideoReg videoReg)
        {
            using (db)
            {
                var v = db.VideoReg.FirstOrDefault(x => x.Id == videoReg.Id);
                if (v == null)
                    return BadRequest("The video registrator not exsist.");

                v.VideoFolder = videoReg.VideoFolder;
                v.AutoLoadVideo = videoReg.AutoLoadVideo;
                v.BrigadeCode = videoReg.BrigadeCode;
                v.Ip = videoReg.Ip;
                v.VideoFolder = videoReg.VideoFolder;
                v.ChannelAutoLoad = videoReg.ChannelAutoLoad;
                v.ChannelTimeStamp = videoReg.ChannelTimeStamp;
                v.Password = videoReg.Password;
                v.User = videoReg.User;
                db.SaveChanges();
            }
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Delete([FromBody]int id)
        {
            using (db)
            {
                // Каскадное удалене
                // Извлечь нужного покупателя из таблицы вместе с заказами
                VideoReg customer = db.VideoReg
                    .Include(x => x.Camers)
                    .FirstOrDefault(x => x.Id == id);

                // Удалить этого покупателя
                if (customer != null)
                {
                    db.VideoReg.Remove(customer);
                    db.SaveChanges();
                    return Ok();
                }
            }
            return BadRequest("Video registrator not found.");
        }

        [HttpGet]
        public VideoReg Get(int id)
        {
            using (db)
            {
                VideoReg res = db.VideoReg
                    .Include(x => x.Camers)
                    .FirstOrDefault(x => x.Id == id);
                return res;
            }         
        }

        [HttpGet]
        public IEnumerable<VideoReg> GetAll()
        {
            using (db)
            {
                var res = db.VideoReg
                    .Include(x=>x.Camers)
                    .ToList();
                return res;
            }
        }
    }
}

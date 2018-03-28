using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Value;

namespace FtpSync.Entety
{
    class VideoRegRep
    {
        readonly DataContext db;

        public VideoRegRep()
        {
            this.db = new DataContext();
        }

        public List<VideoReg> GetAll()
        {
            return db.VideoReg.ToList();
        }

        public VideoReg Get(int id)
        {
            return db.VideoReg.FirstOrDefault(x => x.Id == id);
        }

        public void Update(VideoReg v)
        {
             var item = db.VideoReg.First(x=>x.Id == v.Id);
            item.BrigadeCode = v.BrigadeCode;
            item.Ip = v.Ip;
            item.User = v.User;
            item.Password = v.Password;
            item.ChannelFolder = v.ChannelFolder;
            item.ChannelAutoLoad = v.ChannelAutoLoad;

        }

    }
}


using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Repositories.Interfaces;

namespace FtpSync.Repositories
{
    public class VideoRegRep : IVideoRegRep
    {
        public UpdateEntetyStatus SetUpdateChannelValues(int brigadeCode, AutoLoadStatus status)
        {
            using (var db = new DataContext())
            {
                // Поиск видеорегистратора в базе
                var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                if (reg == null)
                {
                    return UpdateEntetyStatus.notExist;
                }

                if (reg.UpdateChannelValues == AutoLoadStatus.on)
                {
                    return UpdateEntetyStatus.notUpdate;
                }
                reg.UpdateChannelValues = AutoLoadStatus.on;
                db.SaveChanges();
            }

            return UpdateEntetyStatus.updated;
        }
        public UpdateEntetyStatus SetChannelTimeStamp(int brigadeCode, DateTime? timeStamp)
        {
            using (var db = new DataContext())
            {
                var reg = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                if (reg == null)
                    return UpdateEntetyStatus.notExist;
                if (reg.ChannelTimeStamp == timeStamp)
                    return UpdateEntetyStatus.notUpdate;
                reg.ChannelTimeStamp = timeStamp;
                db.SaveChanges();
            }
            return UpdateEntetyStatus.updated;

        }
        public VideoReg Get(int brigadeCode)
        {
            using (var db = new DataContext())
            {
                // Поиск видеорегистратора в базе
                var reg = db.VideoReg
                    .Include(x => x.Camers)
                    .FirstOrDefault(x => x.BrigadeCode == brigadeCode);
                return reg;
            }
        }
        public List<VideoReg> GetAll()
        {
            using (var db = new DataContext())
            {
                var res = db.VideoReg
                    .Include(x => x.Camers)
                    .ToList();
                return res;
            }
        }
        public void Add(VideoReg videoReg)
        {
            using (var db = new DataContext())
            {
                videoReg.Camers = new List<Camera>();
                // Добавляем сразу 10 камер 
                for (int i = 0; i < 10; i++)
                {
                    var cam = new Camera { Num = i };
                    videoReg.Camers.Add(cam);
                }
                db.VideoReg.Add(videoReg);
                db.SaveChanges();
            }
        }
        public UpdateEntetyStatus Update(VideoReg videoReg)
        {
            using (var db = new DataContext())
            {
                var v = db.VideoReg.FirstOrDefault(x => x.BrigadeCode == videoReg.BrigadeCode);
                if (v == null)
                    return UpdateEntetyStatus.notExist;
                v.VideoFolder = videoReg.VideoFolder;
                v.BrigadeCode = videoReg.BrigadeCode;
                v.Ip = videoReg.Ip;
                v.VideoFolder = videoReg.VideoFolder;
                v.ChannelAutoLoad = videoReg.ChannelAutoLoad;
                v.ChannelTimeStamp = videoReg.ChannelTimeStamp;
                v.UpdateChannelValues = videoReg.UpdateChannelValues;
                v.ValuesFile = videoReg.ValuesFile;
                v.Password = videoReg.Password;
                v.User = videoReg.User;
                db.SaveChanges();
            }
            return UpdateEntetyStatus.updated;
        }
        public UpdateEntetyStatus Delete(int brigadeCode)
        {
            using (var db = new DataContext())
            {
                VideoReg reg = db.VideoReg
                    .Include(x => x.Camers) // Для каскадного удаления
                    .FirstOrDefault(x => x.BrigadeCode == brigadeCode);

                if (reg == null)
                    return UpdateEntetyStatus.notExist;

                db.VideoReg.Remove(reg);
                db.SaveChanges();
                return UpdateEntetyStatus.updated;
            }
        }
    }
}

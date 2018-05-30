using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;

namespace FtpSync.Repositories
{
    class CameraRep : ICameraRep
    {
        public UpdateEntetyStatus UpdateAuto(int brigadeCode, int cameraNum, AutoLoadStatus status)
        {
            using (var db = new DataContext())
            {
                var v = db.Camera.FirstOrDefault(x => x.VideoReg.BrigadeCode == brigadeCode && x.Num == cameraNum);
                if (v == null)
                    return UpdateEntetyStatus.notExist;
                if (v.AutoLoadVideo == status)
                    return UpdateEntetyStatus.notUpdate;
                v.AutoLoadVideo = status;
                db.SaveChanges();
            }
            return UpdateEntetyStatus.updated;
        }
        public Camera Get(int brigadeCode, int cameraNum)
        {
            using (var db = new DataContext())
            {
                // Поиск видеорегистратора в базе
                var cam = db.Camera
                    .Include(x => x.VideoReg)
                    .FirstOrDefault(x =>
                        x.VideoReg.BrigadeCode == brigadeCode &&
                        x.Num == cameraNum);
                return cam;
            }
        }
        public UpdateEntetyStatus SetTimeStamp(int brigadeCode, int cameraNum, DateTime? timeStamp)
        {
            using (var db = new DataContext())
            {
                var cam = db.Camera.FirstOrDefault(x =>
                    x.VideoReg.BrigadeCode == brigadeCode &&
                    x.Num == cameraNum);

                if (cam == null)
                    return UpdateEntetyStatus.notExist;
                if (cam.TimeStamp == timeStamp)
                    return UpdateEntetyStatus.notUpdate;
                cam.TimeStamp = timeStamp;
                db.SaveChanges();
            }
            return UpdateEntetyStatus.updated;
        }
    }
}

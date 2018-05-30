using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;

namespace FtpSync.Repositories
{
    public interface ICameraRep
    {
        UpdateEntetyStatus UpdateAuto(int brigadeCode, int cameraNum, AutoLoadStatus status);
        Camera Get(int brigadeCode, int cameraNum);
        UpdateEntetyStatus SetTimeStamp(int brigadeCode, int cameraNum, DateTime? timeStamp);
    }
}

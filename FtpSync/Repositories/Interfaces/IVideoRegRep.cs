using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;

namespace FtpSync.Repositories.Interfaces
{
    public interface IVideoRegRep
    {
        UpdateEntetyStatus SetChannelTimeStamp(int brigadeCode, DateTime? timeStamp);
        UpdateEntetyStatus SetUpdateChannelValues(int brigadeCode, AutoLoadStatus status);
        VideoReg Get(int brigadeCode);
        List<VideoReg> GetAll();
        void Add(VideoReg videoReg);
        UpdateEntetyStatus Update(VideoReg videoReg);
        UpdateEntetyStatus Delete(int brigadeCode);
    }
}

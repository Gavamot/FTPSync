using System.Collections.Generic;
using FtpSync.Entety;

namespace FtpSync.Interface
{
    interface IVideoRegRepository
    {
        IEnumerable<VideoReg> GetAll();
    }
}

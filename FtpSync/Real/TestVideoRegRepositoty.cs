using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;
using FtpSync.Interface;

namespace FtpSync.Real
{
    class TestVideoRegRepositoty : IVideoRegRepository
    {
        public IEnumerable<VideoReg> GetAll()
        {
            yield return new VideoReg
            {
                Id = 1,
                BrigadeCode = 1,
                Src = "ftp://",
                User = "ftpuser",
                Password = "123"
            };
            yield return new VideoReg
            {
                Id = 2,
                BrigadeCode = 2,
                Src = "ftp://",
                User = "ftpuser",
                Password = "123"
            };
        }
    }
}

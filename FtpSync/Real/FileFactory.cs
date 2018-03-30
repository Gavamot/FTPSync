using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Value;
using NUnit.Framework;

namespace FtpSync.Real
{
    class FileFactory
    {
        public IFile Create(string fileName)
        {
            var f = new FileInfo(fileName);
            string fname = Path.GetFileNameWithoutExtension(f.Name);
            string[] str = fname.Split('_');

            DateTime dt = str[0].ToDate("yyyy.MM.ddTHH.mm.ss");
            int model = int.Parse(str[1]);
            int number = int.Parse(str[2]);
            int release = int.Parse(str[3]);
            int serial = int.Parse(str[4]);

            switch (f.Extension)
            {
                case ".json": return new FileChannelJson
                {
                    DT = dt,
                    Model = model,
                    Number = number,
                    Release = release,
                    Serial = serial
                };
                case ".mp4":
                {
                    int? durationSec = str.Length > 5 ? (int?)int.Parse(str[5]) : null;  
                    return new FileVdeoMp4
                    {
                        DT = dt,
                        Model = model,
                        Number = number,
                        Release = release,
                        Serial = serial,
                        DurationSec = durationSec
                    };
                }
                default:
                    throw new FormatException($"Неизветное расширение файла ({f.Extension})");
            }

        }
    }
}

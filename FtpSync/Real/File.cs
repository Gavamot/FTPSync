using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    interface IFile
    {
        DateTime DT { get; }
        int Model { get; }
        int Number { get; }
        int Release { get;}
        int Serial { get; }
        bool IsComplete { get; }
        string Exst { get;  }
    }

    // Формат информации по каналам
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial.json
    // 2018.03.20T09.56.01_14_14422_0_14422.json
    class FileChannelJson : IFile
    {
        public DateTime DT { get; protected set; }
        public int Model { get; protected set; }
        public int Number { get; protected set; }
        public int Release { get; protected set; }
        public int Serial { get; protected set; }
        public bool IsComplete => true;
        public string Exst => ".json";
    }

    // Формат видеофайла
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial_duration.mp4
    // 2018.03.20T09.56.01_14_14422_0_14422_62.mp4
    class FileVdeoMp4 : IFile
    {
        public DateTime DT { get; protected set; }
        public int Model { get; protected set; }
        public int Number { get; protected set; }
        public int Release { get; protected set; }
        public int Serial { get; protected set; }
        public int? DurationSec { get; protected set; }
        public bool IsComplete => DurationSec != null; 
        public string Exst => ".mp4";
    }

}

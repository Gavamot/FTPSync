using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Real.File
{
    // Формат информации по каналам
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial.json
    // 2018.03.20T09.56.01_14_14422_0_14422.json
    class FileChannelJson : File
    {
        public DateTime DT { get; set; }
        public int Model { get; set; }
        public int Number { get; set; }
        public int Release { get; set; }
        public int Serial { get; set; }
        /// <summary>
        /// Json записанны всегда полностью
        /// </summary>
        public override bool IsComplete => true;
        public override string Exst => ".json";
        public override string ToString()
        {
            return $"{DT:yyyy-M-dTHH-mm-ss}_{Model}_{Number}_{Release}_{Serial}{Exst}";
        }
    }
}

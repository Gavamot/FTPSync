using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Real.File;

namespace FtpSync.Real.File
{
    // Формат видеофайла
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial_duration.mp4
    // 2018.03.20T09.56.01_14_14422_0_14422_62.mp4
    class FileVdeoMp4 : File
    {
        public int? DurationSec { get; set; }
        /// <summary>
        /// Видеофайл записан полностью если у него имеется продолжительность 
        /// иначе он либо битые либо записывается в текущий момент
        /// </summary>
        public override bool IsComplete => DurationSec != null;

        public override string Exst => ".mp4";

        public override string ToString()
        {
            return $"{Pdt:yyyy-M-dTHH-mm-ss}_{Model}_{Number}_{Release}_{Serial}{Exst}";
        }
    }

}

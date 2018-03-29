using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Value
{
    interface IFile
    {
        DateTime DT { get; set; }
        int Model   { get; set; }
        int Number  { get; set; }
        int Release { get; set; }
        int Serial  { get; set; }
        string Exst { get; }
        /// <summary>
        /// Показывает завершен полностью ли полностью файл записан на диск
        /// </summary>
        bool IsComplete { get; }
    }

    abstract class File : IFile
    {
        public DateTime DT { get; set; }
        public int Model   { get; set; }
        public int Number  { get; set; }
        public int Release { get; set; }
        public int Serial  { get; set; }
        /// <summary>
        /// Показывает завершен полностью ли файл полностью записан на диск
        /// </summary>
        public abstract bool IsComplete { get; }
        public abstract string Exst { get; }
    }

    // Формат информации по каналам
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial.json
    // 2018.03.20T09.56.01_14_14422_0_14422.json
    class FileChannelJson : File
    {
        public DateTime DT { get; set; }
        public int Model   { get; set; }
        public int Number  { get; set; }
        public int Release { get; set; }
        public int Serial  { get; set; }
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

    // Формат видеофайла
    // yyyy-MM-ddTHH:mm:ss_model_number_release_serial_duration.mp4
    // 2018.03.20T09.56.01_14_14422_0_14422_62.mp4
    class FileVdeoMp4 : IFile
    {
        public DateTime DT { get; set; }
        public int Model   { get; set; }
        public int Number  { get; set; }
        public int Release { get; set; }
        public int Serial  { get; set; }
        public int? DurationSec { get; set; }
        /// <summary>
        /// Видеофайл записан полностью если у него имеется продолжительность 
        /// иначе он либо битые либо записывается в текущий момент
        /// </summary>
        public bool IsComplete => DurationSec != null; 
        public string Exst => ".mp4";
        public override string ToString()
        {
            return $"{DT:yyyy-M-dTHH-mm-ss}_{Model}_{Number}_{Release}_{Serial}{Exst}";
        }
    }

}

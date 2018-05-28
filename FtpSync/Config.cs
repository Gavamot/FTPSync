using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync
{
    class Config
    {
        /// <summary>
        /// Путь к каталогу с видеофайлами на видеорегистраторах
        /// </summary>
        public string VideoFolder { get; set; } = "D:\\video";
        /// <summary>
        /// Путь к каталогу с каналами на видеорегистраторах
        /// </summary>
        public string ChannelFolder { get; set; } = "D:\\channels";
        /// <summary>
        /// Путь к каталогу с каналами на видеорегистраторах
        /// </summary>
        public string Host { get; set; } = "http://localhost:9000/";
        /// <summary>
        /// Задержка между итерациями автозагрузки файлов значений с приборов
        /// </summary>
        public int ChannelAutoDelayMs { get; set; } = 60000;
        /// <summary>
        /// Задержка между итерациями автозагрузки видео файлов
        /// </summary>
        public int VideoAutoDelayMs { get; set; } = 60000;

        /// <summary>
        /// Путь к файлу с текущими данными с прибора на видеорегистраторах 
        /// </summary>
        public string ValuesFilePath { get; set; } = "D:\\values.txt";
    }
}

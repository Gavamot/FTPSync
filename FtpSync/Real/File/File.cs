using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Real.File
{
    abstract class File : IFile
    {
        public DateTime Pdt { get; set; }
        public int Model { get; set; }
        public int Number { get; set; }
        public int Release { get; set; }
        public int Serial { get; set; }
        /// <summary>
        /// Показывает завершен полностью ли файл полностью записан на диск
        /// </summary>
        public abstract bool IsComplete { get; }
        public abstract string Exst { get; }
    }

}

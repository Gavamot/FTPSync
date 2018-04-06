using FtpSync.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Real.File
{
    interface IFile
    {
        DateTime Pdt { get; set; }
        int Model { get; set; }
        int Number { get; set; }
        int Release { get; set; }
        int Serial { get; set; }
        string Exst { get; }
        /// <summary>
        /// Показывает завершен полностью ли полностью файл записан на диск
        /// </summary>
        bool IsComplete { get; }
        bool IsInInterval(DateInterval interval);
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Entety
{
    class Camera
    {
        public int Id { get; set; }

        [Key]
        [ForeignKey("VideoReg")]
        public int VideoRegId { get; set; }

        public VideoReg VideoReg { get; set; }
        public int Num { get; set; }
        public string TimeStamp { get; set; }
        // public DateTime TimeStamp { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Entety
{
    [Table("Camera")]
    class Camera
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("VideoReg")]
        public int VideoRegId { get; set; }

        //[ForeignKey("VideoRegId")]
        public VideoReg VideoReg { get; set; }

        public int Num { get; set; }
        // 2018-03-23 21:59:25.9691178
        public DateTime TimeStamp { get; set; }

        public override string ToString()
        {
            return $"{Id}) cam {Num} TimeStamp = {TimeStamp:yyyy-MM-dd HH:mm:ss}";
        }
    }
}

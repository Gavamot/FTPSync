using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FtpSync.Entety
{
    [Table("Camera")]
    public class Camera
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int VideoRegId { get; set; }

        [Required]
        [ForeignKey("VideoRegId")]
        [IgnoreDataMember]
        public VideoReg VideoReg { get; set; }

        [Required]
        [Range(0, 9)]
        public int Num { get; set; }

        public AutoLoadStatus AutoLoadVideo { get; set; }

        // 2018-03-23 21:59:25.9691178
        public DateTime? TimeStamp { get; set; }

        public override string ToString()
        {
            return $"{Id}) cam {Num} TimeStamp = {TimeStamp:yyyy-MM-dd HH:mm:ss} auto = {AutoLoadVideo}";
        }

    }
}

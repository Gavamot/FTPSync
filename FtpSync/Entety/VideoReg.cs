using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Value;
using Newtonsoft.Json;

namespace FtpSync.Entety
{
    [Table("VideoReg")]
    public class VideoReg
    {
        public int Id { get; set; }

        [Required]
        [Range(0, 10000000)]
        public int BrigadeCode { get; set; }

        // Настройки FTP
        [Required]
        [MaxLength(24)]
        public string Ip { get; set; }

        [Required]
        [MaxLength(24)]
        public string User { get; set; }

        [MaxLength(50)]
        public string Password { get; set; }

        [JsonIgnore]
        public FtpSettings FtpSettings => new FtpSettings(Ip, User, Password);

        // Каталоги для перекачки если null то незакачивать
        [MaxLength(500)]
        public string ChannelFolder { get; set; }

        [Range(0, 1)]
        public int ChannelAutoLoad { get; set; }

        public DateTime? ChannelTimeStamp { get; set; }

        [MaxLength(500)]
        public string VideoFolder { get; set; }

        [Range(0, 1)]
        public int AutoLoadVideo { get; set; }

        [InverseProperty("VideoReg")]
        public List<Camera> Camers { get; set; } = new List<Camera>();

        public override string ToString()
        {
            string res = $@"{Id}) BrigadeCode={BrigadeCode}
                ip={Ip}  User={User}  Password={Password}
                ChannelFolder={ChannelFolder}  ChannelAutoLoad={ChannelAutoLoad}  ChannelTimeStamp={ChannelTimeStamp}
                VideoFolder={VideoFolder}  AutoLoadVideo={AutoLoadVideo}";
            foreach (var c in Camers)
            {
                res += $"\n\r{c}";
            }
            return res;
        }
    }
}

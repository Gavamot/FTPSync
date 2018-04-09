using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpSync.Entety;

namespace FtpSync
{
    public class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection")
        {
            Database.SetInitializer<DataContext>(null);
        }

        public DbSet<Camera> Camera { get; set; }
        public DbSet<VideoReg> VideoReg { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Привязка видеорегистраторов с камерами 
            modelBuilder.Entity<Camera>()
                .HasRequired(cam => cam.VideoReg)
                .WithMany(vreg => vreg.Camers)
                .HasForeignKey(x => x.VideoRegId)
                .WillCascadeOnDelete(true); // Каскадное удаление
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}

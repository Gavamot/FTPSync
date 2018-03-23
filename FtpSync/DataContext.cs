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
    class DataContext : DbContext
    {
        public DataContext() : base("DefaultConnection")
        {
        }

        public DbSet<Camera> Camera { get; set; }
        public DbSet<VideoReg> VideoReg { get; set; }
        

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Camera>()
                .HasRequired(cam => cam.VideoReg)
                .WithMany(vreg => vreg.Camers)
                .HasForeignKey(x => x.VideoRegId);
            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}

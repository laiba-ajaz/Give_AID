using Microsoft.EntityFrameworkCore;
using Give_AID.Models;

namespace Give_AID.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<DonationCauses> DonationCauses { get; set; }
        public DbSet<NGO> NGOs { get; set; }
        public DbSet<Programme> Programmes { get; set; }
        // Stores all financial contributions submitted by donors
        public DbSet<Donation> Donations { get; set; }
        // Stores volunteer and sponsor participation interest for welfare programmes
        public DbSet<ProgrammeInterest> ProgrammeInterests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // NGO ya Cause delete karne par uske programmes khud delete na hon
            modelBuilder.Entity<Programme>()
                .HasOne(p => p.NGO).WithMany()
                .HasForeignKey(p => p.NGOID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Programme>()
                .HasOne(p => p.Cause).WithMany()
                .HasForeignKey(p => p.DonationCausesID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

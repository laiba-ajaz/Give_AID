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


    }
}

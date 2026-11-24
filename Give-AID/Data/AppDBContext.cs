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
        public DbSet<Partner> Partners { get; set; }

        // Donors ki taraf se di gayi donations (User Module Feature 1)
        public DbSet<Donation> Donations { get; set; }

        // Programmes ke liye volunteers aur participants ka interest (User Module Feature 2)
        public DbSet<ProgrammeInterest> ProgrammeInterests { get; set; }

        // Registered members ki taraf se bheje gaye dost ke invitations (User Module Feature 4)
        public DbSet<Invitation> Invitations { get; set; }

        // About Us page ke content aur team blocks
        public DbSet<AboutPage> AboutPages { get; set; }
        public DbSet<AboutItem> AboutItems { get; set; }

        // Contact Us details aur public messages
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        // Photo Gallery ki pictures
        public DbSet<GalleryImage> GalleryImages { get; set; }

        // Live Chat sessions aur messages
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        // Help Centre se aayi queries aur un par admin ke jawab (User Query SRS)
        public DbSet<UserQuery> Queries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ek email se sirf ek account ban sakta hai
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // NGO ya Cause delete karne par uske programmes khud delete na hon
            modelBuilder.Entity<Programme>()
                .HasOne(p => p.NGO).WithMany()
                .HasForeignKey(p => p.NGOID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Programme>()
                .HasOne(p => p.Cause).WithMany()
                .HasForeignKey(p => p.DonationCausesID)
                .OnDelete(DeleteBehavior.Restrict);

            // Har About page ek hi baar ho (slug se dhoondte hain)
            modelBuilder.Entity<AboutPage>()
                .HasIndex(a => a.Slug)
                .IsUnique();

            modelBuilder.Entity<AboutItem>()
                .HasOne(i => i.Page).WithMany(p => p.Items)
                .HasForeignKey(i => i.AboutPageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Programme delete ho to us ki gallery photos rehti hain, bas link hat jata hai
            modelBuilder.Entity<GalleryImage>()
                .HasOne(g => g.Programme).WithMany()
                .HasForeignKey(g => g.ProgrammeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ChatSession>()
                .HasIndex(s => s.Token)
                .IsUnique();

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Session).WithMany(s => s.Messages)
                .HasForeignKey(m => m.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

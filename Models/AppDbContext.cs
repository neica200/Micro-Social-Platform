using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Micro_social_app.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>
        options)
        : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // PROFILE CONFIG
            builder.Entity<Profile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            builder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

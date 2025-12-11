using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Micro_social_app.Models
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<FollowRequest> FollowRequests { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<GroupMessage> GroupMessages { get; set; }
        public DbSet<AIModLog> AIModLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // PROFILE
            builder.Entity<Profile>()
                .HasIndex(p => p.UserId)
                .IsUnique();

            builder.Entity<Profile>()
                .HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            //FOLLOW
            builder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FollowedId })
                .IsUnique();

            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany()
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Cascade);

            // FOLLOW REQUEST
            builder.Entity<FollowRequest>()
                .HasIndex(r => new { r.SenderId, r.ReceiverId })
                .IsUnique();

            builder.Entity<FollowRequest>()
                .HasOne(r => r.Sender)
                .WithMany()
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FollowRequest>()
                .HasOne(r => r.Receiver)
                .WithMany()
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            // POST
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // COMMENT
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // REACTION
            builder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany()
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasIndex(r => new { r.UserId, r.PostId })
                .IsUnique();

            // GROUP
            builder.Entity<Group>()
                .HasOne(g => g.Moderator)
                .WithMany()
                .HasForeignKey(g => g.ModeratorId)
                .OnDelete(DeleteBehavior.Cascade);

            // GROUP MEMBER
            builder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany()
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMember>()
                .HasIndex(gm => new { gm.GroupId, gm.UserId })
                .IsUnique();

            // GROUP MESSAGE
            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.Group)
                .WithMany()
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.User)
                .WithMany()
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI MOD LOG
            builder.Entity<AIModLog>()
                .HasOne(log => log.User)
                .WithMany()
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }

        
    }
}

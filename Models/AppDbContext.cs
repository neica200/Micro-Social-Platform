using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Micro_social_app.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
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

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
          
            
            //FOLLOW
            builder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FollowedId })
                .IsUnique();

            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Follow>()
                .HasOne(f => f.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowedId)
                .OnDelete(DeleteBehavior.Cascade);

            // FOLLOW REQUEST
            builder.Entity<FollowRequest>()
                .HasIndex(r => new { r.SenderId, r.ReceiverId })
                .IsUnique();

            builder.Entity<FollowRequest>()
                .HasOne(r => r.Sender)
                .WithMany(u => u.SentFollowRequests)
                .HasForeignKey(r => r.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FollowRequest>()
                .HasOne(r => r.Receiver)
                .WithMany(u => u.ReceivedFollowRequests)
                .HasForeignKey(r => r.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

            // POST user -> post
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // COMMENT user -> comments post -> comments
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // REACTION
            builder.Entity<Reaction>()
                .HasOne(r => r.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reactions)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Reaction>()
                .HasIndex(r => new { r.UserId, r.PostId })
                .IsUnique();

            // GROUP
            builder.Entity<Group>()
                .HasOne(g => g.Moderator)
                .WithMany(u => u.ModeratedGroups)
                .HasForeignKey(g => g.ModeratorId)
                .OnDelete(DeleteBehavior.Cascade);

            // GROUP MEMBER
            builder.Entity<GroupMember>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMembers)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMember>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupsIn)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMember>()
                .HasIndex(gm => new { gm.GroupId, gm.UserId })
                .IsUnique();

            // GROUP MESSAGE
            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.Group)
                .WithMany(g => g.GroupMessages)
                .HasForeignKey(gm => gm.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GroupMessage>()
                .HasOne(gm => gm.User)
                .WithMany(u => u.GroupMessages)
                .HasForeignKey(gm => gm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AI MOD LOG
            builder.Entity<AIModLog>()
                .HasOne(log => log.User)
                .WithMany(u => u.AIModLogs)
                .HasForeignKey(log => log.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }

        
    }
}

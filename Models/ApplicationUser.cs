using Microsoft.AspNetCore.Identity;

namespace Micro_social_app.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Profile? Profile { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = [];
        public virtual ICollection<Comment> Comments { get; set; } = [];
        public virtual ICollection<Reaction> Reactions { get; set; } = [];
        public virtual ICollection<Follow> Followers { get; set; } = [];
        public virtual ICollection<Follow> Following { get; set; } = [];
        public virtual ICollection<FollowRequest> SentFollowRequests { get; set; } = [];
        public virtual ICollection<FollowRequest> ReceivedFollowRequests { get; set; } = [];
        public virtual ICollection<GroupMember> GroupsIn { get; set; } = [];
        public virtual ICollection<Group> ModeratedGroups { get; set; } = [];
        public virtual ICollection<GroupMessage> GroupMessages { get; set; } = [];
        public virtual ICollection<AIModLog> AIModLogs { get; set; } = [];


    }
}

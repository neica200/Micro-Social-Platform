using Micro_social_app.Models;

namespace Micro_social_app.Models.ViewModels
{
    public class FeedPageVM
    {
        public List<Post> FollowingPosts { get; set; } = new();
        public List<Post> ExplorePosts { get; set; } = new();
        public List<Profile> SuggestedProfiles { get; set; } = new();
        public List<Group> SuggestedGroups { get; set; } = new();
        public string? CurrUserId { get; set; }
    }
}

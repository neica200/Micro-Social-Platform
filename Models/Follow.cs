using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FollowerId { get; set; }
        public ApplicationUser Follower { get; set; }

        [Required]
        public string FollowedId { get; set; }
        public ApplicationUser Followed { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ModeratorId { get; set; }
        public ApplicationUser? Moderator { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        //[Required]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        // prop de navigare
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = [];
        public virtual ICollection<GroupMessage> GroupMessages {  get; set; } = [];
    }
}

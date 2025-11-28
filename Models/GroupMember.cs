using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class GroupMember
    {
        [Key]
        public int Id { get; set; }   

        [Required]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Member"; // "Member" sau "Moderator"

        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // "Pending" sau "Approved"

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}

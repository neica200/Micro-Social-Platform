using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class AIModLog
    {
        [Key]
        public int Id { get; set; } 

        [Required]
        [MaxLength(20)]
        public string ContentType { get; set; }  // "post", "comment", "group_message"

        [Required]
        public int ContentId { get; set; } // id din Posts/Comments/GroupMessages

        [Required]
        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        public string OriginalText { get; set; }

        [Required]
        [MaxLength(20)]
        public string Decision { get; set; }  // "approved", "rejected"

        [MaxLength(500)]
        public string? Reason { get; set; } 

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}


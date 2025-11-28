using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [MaxLength(2000)]
        public string Content { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(500)]
        public string VideoUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AIStatus { get; set; } = "Approved"; // Approved, Rejected

        [MaxLength(500)]
        public string ErrorMessage { get; set; }
    }
}

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; } // practic asta e fk - o postare e postata de un user

        public ApplicationUser? User { get; set; } // asta e prop de navigatie

        [MaxLength(2000)]
        public string? Content { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [MaxLength(500)]
        public string? VideoUrl { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string AIStatus { get; set; } = "Approved"; // Approved, Rejected

        [MaxLength(500)]
        public string? ErrorMessage { get; set; }

        // proprietati de navigare
        public virtual ICollection<Comment>? Comments { get; set; } = [];
        public virtual ICollection<Reaction>? Reactions { get; set; } = [];
    }
}

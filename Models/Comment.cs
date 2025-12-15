using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }  

        [Required]
        public int PostId { get; set; }
        public virtual Post? Post { get; set; }

        [Required]
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public bool AIApproved { get; set; } = true;
    }
}

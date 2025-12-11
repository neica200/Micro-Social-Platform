using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Micro_social_app.Models
{
    public class FollowRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Micro_social_app.Models;

namespace Micro_social_app.Controllers
{
    public class ReactionsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext db;

        public ReactionsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleLikeAjax(int postId)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) 
                return Unauthorized();

            var reaction = db.Reactions.FirstOrDefault(r =>
                r.PostId == postId && r.UserId == userId && r.Type == "Like");

            bool likedNow;

            if (reaction == null)
            {
                db.Reactions.Add(new Reaction
                {
                    PostId = postId,
                    UserId = userId,
                    Type = "Like",
                    CreatedAt = DateTime.UtcNow
                });
                likedNow = true;
            }
            else
            {
                db.Reactions.Remove(reaction); 
                likedNow = false;
            }

            db.SaveChanges();

            int likesCount = db.Reactions.Count(r => r.PostId == postId && r.Type == "Like");

            return Json(new { liked = likedNow, likes = likesCount });
        }
    }
}

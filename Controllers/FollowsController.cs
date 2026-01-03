using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Micro_social_app.Models;

namespace Micro_social_app.Controllers
{
    [Authorize]
    public class FollowsController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(string userId, string? returnUrl = null)
        {
            var me = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId) || me == userId)
                return Redirect(returnUrl ?? "/");

            var alreadyFollowing = await db.Follows.AnyAsync(f => f.FollowerId == me && f.FollowedId == userId);
            if (alreadyFollowing)
                return Redirect(returnUrl ?? "/");

            var targetProfile = await db.Profiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (targetProfile == null)
                return Redirect(returnUrl ?? "/");

            if (targetProfile.IsPrivate)
            {
                var pending = await db.FollowRequests.AnyAsync(r =>
                    r.SenderId == me && r.ReceiverId == userId && r.Status == "Pending");

                if (!pending)
                {
                    db.FollowRequests.Add(new FollowRequest
                    {
                        SenderId = me,
                        ReceiverId = userId,
                        Status = "Pending"
                    });
                    await db.SaveChangesAsync();
                }
            }
            else
            {
                db.Follows.Add(new Follow
                {
                    FollowerId = me,
                    FollowedId = userId
                });

                var req = await db.FollowRequests.FirstOrDefaultAsync(r => r.SenderId == me && r.ReceiverId == userId);
                if (req != null) db.FollowRequests.Remove(req);

                await db.SaveChangesAsync();
            }

            return Redirect(returnUrl ?? "/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(string userId, string? returnUrl = null)
        {
            var me = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || me == userId)
                return Redirect(returnUrl ?? "/");

            var follow = await db.Follows.FirstOrDefaultAsync(f => f.FollowerId == me && f.FollowedId == userId);
            if (follow != null)
            {
                db.Follows.Remove(follow);
                await db.SaveChangesAsync();
            }

            return Redirect(returnUrl ?? "/");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRequest(string userId, string? returnUrl = null)
        {
            var me = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || me == userId)
                return Redirect(returnUrl ?? "/");

            var req = await db.FollowRequests
                .FirstOrDefaultAsync(r => r.SenderId == me && r.ReceiverId == userId && r.Status == "Pending");

            if (req != null)
            {
                db.FollowRequests.Remove(req);
                await db.SaveChangesAsync();
            }

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var me = _userManager.GetUserId(User);

            var requests = await db.FollowRequests
                .Include(r => r.Sender)
                    .ThenInclude(u => u.Profile)
                .Where(r => r.ReceiverId == me && r.Status == "Pending")
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return View(requests);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int requestId, string? returnUrl = null)
        {
            var me = _userManager.GetUserId(User);

            var req = await db.FollowRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (req == null || req.ReceiverId != me || req.Status != "Pending")
                return Redirect(returnUrl ?? "/Follows/Requests");

            var already = await db.Follows.AnyAsync(f => f.FollowerId == req.SenderId && f.FollowedId == me);
            if (!already)
                db.Follows.Add(new Follow { FollowerId = req.SenderId, FollowedId = me });

            req.Status = "Accepted";
            await db.SaveChangesAsync();

            TempData["message"] = "Follow request accepted.";
            TempData["messageType"] = "alert-success";

            return Redirect(returnUrl ?? "/Follows/Requests");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int requestId, string? returnUrl = null)
        {
            var me = _userManager.GetUserId(User);

            var req = await db.FollowRequests.FirstOrDefaultAsync(r => r.Id == requestId);
            if (req == null || req.ReceiverId != me || req.Status != "Pending")
                return Redirect(returnUrl ?? "/Follows/Requests");

            req.Status = "Rejected";
            await db.SaveChangesAsync();

            TempData["message"] = "Follow request rejected.";
            TempData["messageType"] = "alert-info";


            return Redirect(returnUrl ?? "/Follows/Requests");
        }

    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Micro_social_app.Models;

namespace Micro_social_app.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public SearchController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Live(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
                return Json(Array.Empty<object>());

            q = q.Trim();
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            var results = await _context.Profiles
                .Include(p => p.User)
                .Where(p =>
                    p.UserId != currentUserId &&
                    p.User != null &&
                    (
                        (p.FullName != null && EF.Functions.Like(p.FullName, $"%{q}%")) ||
                        (p.User.UserName != null && EF.Functions.Like(p.User.UserName, $"%{q}%"))
                    )
                )
                .OrderBy(p => p.FullName ?? p.User!.UserName)
                .Take(8)
                .Select(p => new
                {
                    userId = p.UserId,
                    fullName = p.FullName,
                    userName = p.User!.UserName,
                    avatar = p.ProfileImageUrl,
                    isPrivate = p.IsPrivate
                })
                .ToListAsync();

            return Json(results);
        }

        [HttpGet]
        public async Task<IActionResult> Users(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return View(new List<Profile>());

            q = q.Trim();
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;

            var profiles = await _context.Profiles
                .Include(p => p.User)
                .Where(p =>
                    p.UserId != currentUserId &&
                    p.User != null &&
                    (
                        (p.FullName != null && EF.Functions.Like(p.FullName, $"%{q}%")) ||
                        (p.User.UserName != null && EF.Functions.Like(p.User.UserName, $"%{q}%"))
                    )
                )
                .OrderBy(p => p.FullName ?? p.User!.UserName)
                .ToListAsync();

            ViewBag.Query = q;
            return View(profiles);
        }

    }
}

using System.Diagnostics;
using Micro_social_app.Models;
using Micro_social_app.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Micro_social_app.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var me = currentUser?.Id;

            var vm = new FeedPageVM { CurrUserId = me };

            List<string> followingIds = new();
            if (me != null)
            {
                followingIds = await _context.Follows
                    .Where(f => f.FollowerId == me)
                    .Select(f => f.FollowedId)
                    .ToListAsync();
            }

            // FOLLOWING FEED
            if (me != null)
            {
                vm.FollowingPosts = await _context.Posts
                    .AsNoTracking()
                    .Include(p => p.User).ThenInclude(u => u.Profile)
                    .Include(p => p.Comments)
                    .Include(p => p.Reactions)
                    .Where(p => p.UserId == me || followingIds.Contains(p.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(30)
                    .ToListAsync();
            }

            // EXPLORE FEED (public + following + mine)
            vm.ExplorePosts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User).ThenInclude(u => u.Profile)
                .Include(p => p.Comments)
                .Include(p => p.Reactions)
                .Where(p =>
                    (p.User.Profile != null && p.User.Profile.IsPrivate == false)
                    || (me != null && (p.UserId == me || followingIds.Contains(p.UserId))))
                .OrderByDescending(p => p.CreatedAt)
                .Take(30)
                .ToListAsync();

            // SUGGESTED PROFILES
            var profilesQuery = _context.Profiles
                .AsNoTracking()
                .Include(p => p.User);

            if (me == null)
            {
                vm.SuggestedProfiles = await profilesQuery
                    .Where(p => p.IsPrivate == false)
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .ToListAsync();
            }
            else
            {
                vm.SuggestedProfiles = await profilesQuery
                    .Where(p => p.UserId != me && !followingIds.Contains(p.UserId))
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .ToListAsync();
            }

            // SUGGESTED GROUPS (simple: recente, neșterse)
            vm.SuggestedGroups = await _context.Groups
                .AsNoTracking()
                .Where(g => g.IsDeleted == false)
                .OrderByDescending(g => g.CreatedAt)
                .Take(6)
                .ToListAsync();

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

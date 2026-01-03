using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Micro_social_app.Models;

namespace Micro_social_app.ViewComponents
{
    public class FollowRequestsBadgeViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowRequestsBadgeViewComponent(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
                return Content("");

            var me = _userManager.GetUserId((System.Security.Claims.ClaimsPrincipal)User);

            var count = await _context.FollowRequests.CountAsync(r =>
                r.ReceiverId == me && r.Status == "Pending");

            return View(count);
        }
    }
}

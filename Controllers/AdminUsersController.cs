using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Micro_social_app.Models;

namespace Micro_social_app.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users
                .OrderBy(u => u.UserName)
                .ToList();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string id)
        {
            var adminId = _userManager.GetUserId(User);

            if (adminId == id)
            {
                TempData["message"] = "You can't delete your own account.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var user = _userManager.FindByIdAsync(id).Result;
            if (user == null)
            {
                TempData["message"] = "User not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var result = _userManager.DeleteAsync(user).Result;

            if (!result.Succeeded)
            {
                TempData["message"] = "Delete failed.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            TempData["message"] = "User deleted.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }
    }
}

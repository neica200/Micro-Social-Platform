using Micro_social_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Micro_social_app.Controllers
{
    [Authorize(Roles ="User,Admin")]
    public class GroupMessagesController : Controller
    {

        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupMessagesController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            db = context;
            _userManager = userManager;
        }

        [HttpPost]
        public IActionResult New (int groupId, string content)
        {
            var userId = _userManager.GetUserId(User);

            if (!IsMemberOrModerator(groupId, userId))
            {
                TempData["message"] = "Only approved members can post messages.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { id = groupId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["message"] = "Message cannot be empty.";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Show", "Groups", new { id = groupId });
            }

            db.GroupMessages.Add(new GroupMessage
            {
                GroupId = groupId,
                UserId = userId,
                Content = content.Trim(),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });

            db.SaveChanges();


            TempData["message"] = "Message sent.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", "Groups", new { id = groupId });
        }

        [HttpPost]
        public IActionResult Edit (int id, string content)
        {
            var msg = db.GroupMessages.FirstOrDefault(m => m.Id == id && m.IsDeleted == false);

            if (msg == null)
            {
                TempData["message"] = "Message not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Groups");
            }

            var userId = _userManager.GetUserId(User);

            if (msg.UserId != userId)
            {
                TempData["message"] = "You can only edit your own messages.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { id = msg.GroupId });
            }

            if (!IsMemberOrModerator(msg.GroupId, userId))
            {
                TempData["message"] = "You are not allowed to edit messages in this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { id = msg.GroupId });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["message"] = "Message cannot be empty.";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Show", "Groups", new { id = msg.GroupId });
            }
            msg.Content = content.Trim();
            msg.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "Message updated.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", "Groups", new { id = msg.GroupId });

        }

        [HttpPost]
        public IActionResult Delete(int id)
        {

            var msg = db.GroupMessages.FirstOrDefault(m => m.Id == id && m.IsDeleted == false);
            if (msg == null)
            {
                TempData["message"] = "Message not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Groups");
            }

            var userId = _userManager.GetUserId(User);

            if (msg.UserId != userId)
            {
                TempData["message"] = "You can only delete your own messages.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Groups", new { id = msg.GroupId });
            }

            msg.IsDeleted = true;
            msg.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "Message deleted.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", "Groups", new { id = msg.GroupId });
        }





        private bool IsMemberOrModerator (int groupId, string userId)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId && g.IsDeleted == false);

            if (group == null) 
                return false;

            if (group.ModeratorId == userId)
                return true;

            return db.GroupMembers.Any (a => a.GroupId == groupId && a.UserId == userId && a.IsDeleted == false && a.Status == "Approved");
        }
        
    }
}

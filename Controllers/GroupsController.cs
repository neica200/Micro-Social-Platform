using Micro_social_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micro_social_app.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public GroupsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

   
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {

            SetAccessRights();

            var userId = _userManager.GetUserId(User);

            // grupurile mele
            ViewBag.MyGroups = db.Groups
                .Include(g => g.Moderator)
                .Where(g => g.IsDeleted == false && g.ModeratorId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .ToList();

            // grupurile in care sunt
            ViewBag.MemberGroups = db.GroupMembers
                .Include(m => m.Group)
                    .ThenInclude(g => g.Moderator)
                .Where(m =>
                    m.UserId == userId &&
                    m.Status == "Approved" &&
                    m.IsDeleted == false &&
                    m.Group.IsDeleted == false &&
                    m.Group.ModeratorId != userId)
                .Select(m => m.Group)
                .ToList();

            // explore grupuri
            ViewBag.ExploreGroups = db.Groups
                .Include(g => g.Moderator)
                .Where(g =>
                    g.IsDeleted == false &&
                    g.ModeratorId != userId &&
                    !db.GroupMembers.Any(m =>
                        m.GroupId == g.Id &&
                        m.UserId == userId &&
                        m.IsDeleted == false))
                .OrderByDescending(g => g.CreatedAt)
                .ToList();

            return View();
        }


        // un user poate vedea grupurile pe care le-a sters
        [Authorize(Roles = "User,Admin")]
        public IActionResult Deleted()
        {
           

            SetAccessRights();

            var userId = _userManager.GetUserId(User);

            var groups = db.Groups
                .Include(g => g.Moderator)
                .Where(g => g.IsDeleted == true)
                .Where(g => g.ModeratorId == userId)
                .OrderByDescending(g => g.UpdatedAt ?? g.CreatedAt)
                .ToList();

            ViewBag.Groups = groups;
            return View();
        }

   
        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int id)
        {
            SetAccessRights();

            var group = db.Groups
                .Include(g => g.Moderator)
                .Include(g => g.GroupMembers.Where(m => m.IsDeleted == false))
                    .ThenInclude(m => m.User)
                .FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            // daca e sters, doar moderatorul il poate vedea
            if (group.IsDeleted == true && group.ModeratorId != userId)
            {
                TempData["message"] = "You don't have access to this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == id && m.UserId == userId && m.IsDeleted == false);

            bool isModerator = (group.ModeratorId == userId) ||
                               (membership != null && membership.Role == "Moderator");

            ViewBag.IsModerator = isModerator;

            ViewBag.IsMemberApproved = membership != null && membership.Status == "Approved";
            ViewBag.HasPendingRequest = membership != null && membership.Status == "Pending";

            if (isModerator)
            {
                ViewBag.PendingRequests = db.GroupMembers
                    .Include(m => m.User)
                    .Where(m => m.GroupId == id && m.IsDeleted == false && m.Status == "Pending")
                    .ToList();
            }
            var messages = db.GroupMessages
    .Include(m => m.User)
    .Where(m => m.GroupId == id && m.IsDeleted == false)
    .OrderBy(m => m.CreatedAt)
    .ToList();

            ViewBag.Messages = messages;

            return View(group);
        }

      
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Group group)
        {
            group.ModeratorId = _userManager.GetUserId(User);

            
            ModelState.Remove("ModeratorId");
            ModelState.Remove("Moderator");

      

            if (ModelState.IsValid)
            {
                group.IsDeleted = false;
                group.CreatedAt = DateTime.UtcNow;

                db.Groups.Add(group);
                db.SaveChanges();

                // Moderatorul e automat membru
                var gm = db.GroupMembers
                    .FirstOrDefault(m => m.GroupId == group.Id && m.UserId == group.ModeratorId);

                if (gm == null)
                {
                    db.GroupMembers.Add(new GroupMember
                    {
                        GroupId = group.Id,
                        UserId = group.ModeratorId,
                        Role = "Moderator",
                        Status = "Approved",
                        IsDeleted = false,
                        JoinedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    gm.IsDeleted = false;
                    gm.Role = "Moderator";
                    gm.Status = "Approved";
                    gm.UpdatedAt = DateTime.UtcNow;
                }

                db.SaveChanges();

                TempData["message"] = "Group created successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            return View(group);
        }

   
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            SetAccessRights();

            var group = db.Groups.FirstOrDefault(g => g.Id == id && g.IsDeleted == false);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (User.IsInRole("Admin"))
            {
                TempData["message"] = "Admin cannot edit groups.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "You are not allowed to edit this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            return View(group);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Group req)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == id && g.IsDeleted == false);


            ModelState.Remove("ModeratorId");
            ModelState.Remove("Moderator");

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (User.IsInRole("Admin"))
            {
                TempData["message"] = "Admin cannot edit groups.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "You are not allowed to edit this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            if (string.IsNullOrWhiteSpace(req.Description))
                ModelState.AddModelError("Description", "Description is required.");

            if (ModelState.IsValid)
            {
                group.Name = req.Name;
                group.Description = req.Description;
                group.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();

                TempData["message"] = "Group updated successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id });
            }

            return View(req);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == id);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var currUserId = _userManager.GetUserId(User);

         
            if (!(User.IsInRole("Admin") || group.ModeratorId == currUserId))
            {
                TempData["message"] = "You are not allowed to delete this group.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            var now = DateTime.UtcNow;

            try
            {
                // Soft delete group
                db.Groups
                  .Where(g => g.Id == id)
                  .ExecuteUpdate(setters => setters
                      .SetProperty(g => g.IsDeleted, true)
                      .SetProperty(g => g.UpdatedAt, now));

                // Soft delete memberships
                db.GroupMembers
                  .Where(m => m.GroupId == id && m.IsDeleted == false)
                  .ExecuteUpdate(setters => setters
                      .SetProperty(m => m.IsDeleted, true)
                      .SetProperty(m => m.UpdatedAt, now));


                db.GroupMessages
                  .Where(m => m.GroupId == id && m.IsDeleted == false)
                  .ExecuteUpdate(setters => setters
                      .SetProperty(m => m.IsDeleted, true)
                      .SetProperty(m => m.UpdatedAt, now));

                TempData["message"] = "Group deleted successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["message"] = "Delete failed: " + ex.GetBaseException().Message;
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Join(int id)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == id && g.IsDeleted == false);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == id && m.UserId == userId);

            if (membership != null && membership.IsDeleted == false)
            {
                if (membership.Status == "Approved")
                    TempData["message"] = "You are already a member of this group.";
                
                else
                    TempData["message"] = "Your join request is already pending.";
                

                TempData["messageType"] = "alert-warning";

                return RedirectToAction("Show", new { id });
            }

            if (membership == null)
            {
                db.GroupMembers.Add(new GroupMember
                {
                    GroupId = id,
                    UserId = userId,
                    Role = "Member",
                    Status = "Pending",
                    IsDeleted = false,
                    JoinedAt = DateTime.UtcNow
                });
            }
            else
            {
                membership.IsDeleted = false;
                membership.Role = "Member";
                membership.Status = "Pending";
                membership.UpdatedAt = DateTime.UtcNow;
            }

            db.SaveChanges();

            TempData["message"] = "Join request sent to the moderator.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id });
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Approve(int groupId, string userId)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId && g.IsDeleted == false);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Only the group moderator can approve requests.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId && m.IsDeleted == false);

            if (membership == null)
            {
                TempData["message"] = "Join request not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            membership.Status = "Approved";
            membership.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "User approved successfully.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = groupId });
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Reject(int groupId, string userId)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId && g.IsDeleted == false);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Only the group moderator can reject requests.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId && m.IsDeleted == false);

            if (membership == null)
            {
                TempData["message"] = "Join request not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            membership.IsDeleted = true;
            membership.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "Join request rejected.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = groupId });
        }


        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Leave(int id)
        {
            var userId = _userManager.GetUserId(User);

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == id && m.UserId == userId && m.IsDeleted == false);

            if (membership == null)
            {
                TempData["message"] = "You are not a member of this group.";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Show", new { id });
            }

            var group = db.Groups.FirstOrDefault(g => g.Id == id && g.IsDeleted == false);

            if (group != null && group.ModeratorId == userId)
            {
                TempData["message"] = "The moderator cannot leave the group. You can delete the group instead.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id });
            }

            membership.IsDeleted = true;
            membership.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "You left the group.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Kick(int groupId, string userId)
        {
            var group = db.Groups.FirstOrDefault(g => g.Id == groupId && g.IsDeleted == false);

            if (group == null)
            {
                TempData["message"] = "Group not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (group.ModeratorId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Only the group moderator can remove members.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            if (group.ModeratorId == userId)
            {
                TempData["message"] = "You cannot remove the group's moderator.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            var membership = db.GroupMembers
                .FirstOrDefault(m => m.GroupId == groupId && m.UserId == userId && m.IsDeleted == false);

            if (membership == null)
            {
                TempData["message"] = "Member not found.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = groupId });
            }

            membership.IsDeleted = true;
            membership.UpdatedAt = DateTime.UtcNow;

            db.SaveChanges();

            TempData["message"] = "User removed from the group.";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = groupId });
        }

        private void SetAccessRights()
        {
            ViewBag.IsLogged = User.Identity != null && User.Identity.IsAuthenticated;
            ViewBag.CurrUser = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");
        }
    }
}

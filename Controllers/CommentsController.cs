using Micro_social_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micro_social_app.Controllers
{
    public class CommentsController(AppDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
    {
        private readonly AppDbContext db = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        public IActionResult Index()
        {
            return View();
        }

        //adaugare comentariu
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult New([FromForm] Comment comm)
        {
            comm.UserId = _userManager.GetUserId(User);
            comm.CreatedAt = DateTime.UtcNow;

            comm.IsDeleted = false;
            comm.AIApproved = true;

            //remove sa nu crape, ca nu le iau din formular
            ModelState.Remove(nameof(Comment.UserId));
            ModelState.Remove(nameof(Comment.CreatedAt));
            ModelState.Remove(nameof(Comment.User));
            ModelState.Remove(nameof(Comment.Post));


            // minimal validation
            if (comm.Content is null || comm.Content.Trim() == "")
            {
                TempData["message"] = "Comment can't be empty.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", "Posts", new { id = comm.PostId });
            }

            if (ModelState.IsValid)
            {
                db.Comments.Add(comm);
                db.SaveChanges();
                TempData["message"] = "Comment added successfully.";
                TempData["messageType"] = "alert-success";
            }
            else
            {
                TempData["message"] = "Comment couldn't be added.";
                TempData["messageType"] = "alert-danger";
            }

            return RedirectToAction("Show", "Posts", new { id = comm.PostId });



        }

        // edit
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id)
        {
            var comment = db.Comments
                          .Include(c => c.User)
                              .ThenInclude(u => u.Profile)
                          .FirstOrDefault(c => c.Id == id);

            if (comment == null)
                return NotFound();
            else
            {
                if (comment.UserId == _userManager.GetUserId(User))
                    return View(comment);
                else
                {
                    TempData["message"] = "You can't edit a comment that is not yours.";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts");
                }
            }
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Edit(int id, Comment reqComm)
        {
            var comment = db.Comments
                            .Include(c => c.User)
                                .ThenInclude(u => u.Profile)
                            .FirstOrDefault(c => c.Id == id);

            if (comment == null) return NotFound();

            if (comment.UserId != _userManager.GetUserId(User))
            {
                TempData["message"] = "You can't edit a comment that is not yours.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index", "Posts");
            }

            // scoatem ce nu vine din form
            ModelState.Remove(nameof(Comment.UserId));
            ModelState.Remove(nameof(Comment.PostId));
            ModelState.Remove(nameof(Comment.CreatedAt));
            ModelState.Remove(nameof(Comment.User));
            ModelState.Remove(nameof(Comment.Post));

            if (reqComm.Content is null || reqComm.Content.Trim() == "")
            {
                ModelState.AddModelError("Content", "Comment can't be empty.");
                return View(reqComm);
            }

            if (ModelState.IsValid)
            {
                comment.Content = reqComm.Content.Trim();
                comment.UpdatedAt = DateTime.UtcNow;
                db.SaveChanges();

                TempData["message"] = "Comment updated successfully.";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", "Posts", new { id = comment.PostId });
            }

            return View(reqComm);
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            Comment? comm = db.Comments.Find(id);
            if (comm == null)
                return NotFound();
            else
            {
                if (comm.UserId == _userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    db.Comments.Remove(comm);
                    db.SaveChanges();
                    return RedirectToAction("Show", "Posts", new { id = comm.PostId });
                }
                else
                {
                    TempData["message"] = "You are not authorize to delete this comment";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index", "Posts");
                }
            }
        }


    }
}

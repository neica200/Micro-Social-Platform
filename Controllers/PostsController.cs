using Microsoft.AspNetCore.Mvc;
using Micro_social_app.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Humanizer;

namespace Micro_social_app.Controllers
{
    public class PostsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext db;
        private readonly IWebHostEnvironment _env;

        public PostsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment env)
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        // Afisare postari (fara search)
  
        public IActionResult Index()
        {
            var posts = db.Posts
                          .Include(p => p.User)
                          .Include(p => p.Comments)
                          .Include(p => p.Reactions)
                          .OrderByDescending(p => p.CreatedAt)
                          .ToList();

            ViewBag.Posts = posts;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
                ViewBag.Alert = TempData["messageType"];
            }

            SetAccessRights();
            return View();
        }

        public IActionResult Show(int id)
        {
            Post? post = db.Posts
                           .Include(a => a.Comments)
                               .ThenInclude(c => c.User)
                           .Include(a => a.Reactions)
                           .Include(a => a.User)
                           .Where(a => a.Id == id)
                           .FirstOrDefault();

            if (post is null)
                return NotFound();

            SetAccessRights();

            return View(post);
        }

        // Formular adaugare postare
        [Authorize(Roles = "User,Admin")]
        public IActionResult New()
        {
            Post post = new Post();
            return View(post);
        }

        // Salvare postare
        [HttpPost]
        [Authorize(Roles = "User,Admin")] 
        [ValidateAntiForgeryToken]
        public IActionResult New(Post post, IFormFile? ImageFile, IFormFile? VideoFile)
        {
            post.CreatedAt = DateTime.UtcNow;
            post.UserId = _userManager.GetUserId(User);
            post.AIStatus = "Approved";

            //  ca să nu mai crape DB (NOT NULL) de scos dupa
            if (post.ImageUrl == null) post.ImageUrl = "";
            if (post.VideoUrl == null) post.VideoUrl = "";
            if (post.ErrorMessage == null) post.ErrorMessage = "";
            if (post.Content == null) post.Content = "";

            //  scoatem din ModelState ce nu vine din form / nu vrem validat
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("AIStatus");
            ModelState.Remove("ErrorMessage");
            ModelState.Remove("Comments");
            ModelState.Remove("Reactions"); 
            ModelState.Remove("ImageFile");
            ModelState.Remove("VideoFile");

            bool hasText = post.Content.Trim() != "";
            bool hasImg = ImageFile != null && ImageFile.Length > 0;
            bool hasVid = VideoFile != null && VideoFile.Length > 0;

            if (!hasText && !hasImg && !hasVid)
                ModelState.AddModelError("", "Postarea nu poate fi goală. Adaugă text sau o poză/video.");

            // salveaza fisierul (optional)
            if (hasImg)
            {
                string imgPath = SaveFile(ImageFile, "images");
                post.ImageUrl = imgPath;
            }

            if (hasVid)
            {
                string vidPath = SaveFile(VideoFile, "videos");
                post.VideoUrl = vidPath;
            }

            if (ModelState.IsValid)
            {
                db.Posts.Add(post);
                db.SaveChanges();

                TempData["message"] = "Postarea a fost adaugata cu succes!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");
            }

            return View(post);
        }

        [Authorize(Roles ="User,Admin")]
        public IActionResult Edit(int id)
        {
            Post? post = db.Posts.Find(id);
                            

            if (post is null)
                return NotFound();

            if ((post.UserId == _userManager.GetUserId(User)))
                return View(post);
            else
            {
                TempData["message"] = "Nu aveti dreptul sa editati o postare care nu va apartine.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
                                
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Post requestPost, IFormFile? ImageFile, IFormFile? VideoFile)
        {
            Post? post = db.Posts.Find(id);

            if (post == null)
                return NotFound();

            if (post.UserId != _userManager.GetUserId(User))
            {
                TempData["message"] = "Nu aveti puteti modifica o postare care nu este a dvs.";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            // daca DB are NOT NULL, nu trimitem null
            if (requestPost.Content == null) requestPost.Content = "";
            if (post.ImageUrl == null) post.ImageUrl = "";
            if (post.VideoUrl == null) post.VideoUrl = "";

            // scoatem campurile care nu vin din form
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("AIStatus");
            ModelState.Remove("ErrorMessage");
            ModelState.Remove("Comments");
            ModelState.Remove("Reactions");
            ModelState.Remove("ImageFile");
            ModelState.Remove("VideoFile");

            // validare minimă: macar text sau media existenta / noua
            bool hasText = requestPost.Content.Trim() != "";
            bool hasExistingMedia = (post.ImageUrl != "" || post.VideoUrl != "");
            bool hasNewImg = (ImageFile != null && ImageFile.Length > 0);
            bool hasNewVid = (VideoFile != null && VideoFile.Length > 0);

            if (!hasText && !hasExistingMedia && !hasNewImg && !hasNewVid)
            {
                ModelState.AddModelError("", "Postarea nu poate fi goală.");
            }

            // update text
            post.Content = requestPost.Content;

            // daca user a incarcat o poza noua, o salvam si suprascriem URL-ul
            if (hasNewImg)
            {
                if (ImageFile!.ContentType.StartsWith("image/") == false)
                    ModelState.AddModelError("", "Fișierul încărcat la imagine nu este valid.");
                else
                    post.ImageUrl = SaveFile(ImageFile, "images");
            }

            // daca user a incarcat un video nou, il salvam si suprascriem URL-ul
            if (hasNewVid)
            {
                if (VideoFile!.ContentType.StartsWith("video/") == false)
                    ModelState.AddModelError("", "Fișierul încărcat la video nu este valid.");
                else
                    post.VideoUrl = SaveFile(VideoFile, "videos");
            }

            if (ModelState.IsValid)
            {
                TempData["message"] = "Postarea a fost modificata.";
                TempData["messageType"] = "alert-success";

                db.SaveChanges();
                return RedirectToAction("Show", new { id = post.Id });
            }

            // ca sa nu se piarda ce a scris, trimitem un model cu ce are userul
            requestPost.ImageUrl = post.ImageUrl;
            requestPost.VideoUrl = post.VideoUrl;
            return View(requestPost);
        }

        [HttpPost]
        [Authorize(Roles ="User,Admin")]
        public IActionResult Delete(int id)
        {
            Post? post = db.Posts.Find(id); // de vazut care merge stergerea si cand sunt comentarii

            if (post is null)
                return NotFound();

            else
            {
                if (post.UserId==_userManager.GetUserId(User) || User.IsInRole("Admin"))
                {
                    //sterg si fisierele din wwwroot
                    if (!string.IsNullOrEmpty(post.ImageUrl))
                    {
                        var imgPath = Path.Combine(_env.WebRootPath, post.ImageUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(imgPath)) 
                            System.IO.File.Delete(imgPath);
                    }

                    if (!string.IsNullOrEmpty(post.VideoUrl))
                    {
                        var vidPath = Path.Combine(_env.WebRootPath, post.VideoUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (System.IO.File.Exists(vidPath)) 
                            System.IO.File.Delete(vidPath);
                    }

                    db.Posts.Remove(post);
                    db.SaveChanges();
                    TempData["message"] = "The post was succesfully deleted!";
                    TempData["messageType"] = "alert-success";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "You can't delete a post that is not yours!";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Index");
                }
            }
        }

        private void SetAccessRights()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                ViewBag.IsLogged = true;
            else
                ViewBag.IsLogged = false;

            ViewBag.CurrUser = _userManager.GetUserId(User);
            ViewBag.IsAdmin = User.IsInRole("Admin");
        }

        [NonAction]
        private string SaveFile(IFormFile file, string folder)
        {
            string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", folder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string ext = Path.GetExtension(file.FileName);
            string fileName = Guid.NewGuid().ToString() + ext;

            string fullPath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return "/uploads/" + folder + "/" + fileName;
        }

    }
}

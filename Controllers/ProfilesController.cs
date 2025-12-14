using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Micro_social_app.Models;
using System.IO;

namespace Micro_social_app.Controllers
{
    [Authorize] 
    public class ProfilesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Supabase.Client _supabaseClient;

        public ProfilesController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            Supabase.Client supabaseClient)
        {
            _context = context;
            _userManager = userManager;
            _supabaseClient = supabaseClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            string targetUserId;

            if (string.IsNullOrEmpty(id))
            {
                if (currentUser == null) return RedirectToAction("Login", "Account");
                targetUserId = currentUser.Id;
            }
            else
            {
                targetUserId = id;
            }

            var profile = await _context.Profiles
                .Include(p => p.User) 
                .FirstOrDefaultAsync(p => p.UserId == targetUserId);

            if (profile == null)
            {
                if (currentUser != null && targetUserId == currentUser.Id)
                {
                    return RedirectToAction("Edit");
                }
                return NotFound("Acest utilizator nu are încă un profil configurat.");
            }

            if (currentUser != null)
            {
                ViewBag.IsMe = (currentUser.Id == targetUserId); 
            }
            else
            {
                ViewBag.IsMe = false; 
            }

            return View(profile);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var profile = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new Profile
                {
                    UserId = user.Id,
                    ProfileImageUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png"
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Profile profileForm)
        {
            // Initializare SupaBase
            await _supabaseClient.InitializeAsync();
            var user = await _userManager.GetUserAsync(User);

            ModelState.Remove("User");
            ModelState.Remove("ProfileImageUrl");

            var profileDb = await _context.Profiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            bool isNew = false;
            if (profileDb == null)
            {
                isNew = true;
                profileDb = profileForm;
                profileDb.UserId = user.Id; 
            }
            else
            {
                profileDb.FullName = profileForm.FullName;
                profileDb.Description = profileForm.Description;
                profileDb.IsPrivate = profileForm.IsPrivate;
            }
            if (profileForm.ImageUpload != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    await profileForm.ImageUpload.CopyToAsync(memoryStream);
                    var fileBytes = memoryStream.ToArray();

                    var fileExt = Path.GetExtension(profileForm.ImageUpload.FileName);
                    var fileName = $"{user.Id}_{DateTime.Now.Ticks}{fileExt}";
                    await _supabaseClient.Storage
                        .From("avatars")
                        .Upload(fileBytes, fileName);
                    var publicUrl = _supabaseClient.Storage
                        .From("avatars")
                        .GetPublicUrl(fileName);
                    profileDb.ProfileImageUrl = publicUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Eroare la upload: " + ex.Message);
                    return View(profileForm);
                }
            }
            else if (isNew && string.IsNullOrEmpty(profileDb.ProfileImageUrl))
            {
                profileDb.ProfileImageUrl = "https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_1280.png";
            }

            if (isNew) _context.Profiles.Add(profileDb);
            else _context.Profiles.Update(profileDb);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
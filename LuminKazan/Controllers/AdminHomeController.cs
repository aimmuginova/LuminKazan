using LuminKazan.Data;
using LuminKazan.Models;
using LuminKazan.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminHomeController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var photographer = await _context.Photographers
                .FirstOrDefaultAsync();

            if (photographer == null)
            {
                photographer = new Photographer
                {
                    FullName = "LuminKazan",
                    Specialization = string.Empty,
                    Biography = string.Empty,
                    ProfileImagePath = string.Empty,
                    ExperienceYears = 0,
                    Phone = string.Empty,
                    Email = string.Empty,
                    TelegramLink = string.Empty,
                    InstagramLink = string.Empty
                };

                _context.Photographers.Add(photographer);
                await _context.SaveChangesAsync();
            }

            var model = new PhotographerProfileViewModel
            {
                Id = photographer.Id,
                FullName = photographer.FullName,
                Specialization = photographer.Specialization,
                Biography = photographer.Biography,
                ExperienceYears = photographer.ExperienceYears,
                Phone = photographer.Phone,
                Email = photographer.Email,
                TelegramLink = photographer.TelegramLink,
                InstagramLink = photographer.InstagramLink,
                ProfileImagePath = photographer.ProfileImagePath,
                CurrentProfileImagePath = photographer.ProfileImagePath
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PhotographerProfileViewModel model)
        {
            var photographer = await _context.Photographers
                .FirstOrDefaultAsync(photographer => photographer.Id == model.Id);

            if (photographer == null)
            {
                photographer = await _context.Photographers.FirstOrDefaultAsync();
            }

            if (photographer == null)
            {
                photographer = new Photographer();
                _context.Photographers.Add(photographer);
            }

            if (!ModelState.IsValid)
            {
                model.CurrentProfileImagePath = photographer.ProfileImagePath;
                return View(model);
            }

            var uploadedImagePath = await SaveProfileImageAsync(model.ProfileImageFile);

            var finalImagePath = photographer.ProfileImagePath;

            if (!string.IsNullOrWhiteSpace(uploadedImagePath))
            {
                finalImagePath = uploadedImagePath;
            }
            else if (!string.IsNullOrWhiteSpace(model.ProfileImagePath))
            {
                finalImagePath = model.ProfileImagePath;
            }
            else if (!string.IsNullOrWhiteSpace(model.CurrentProfileImagePath))
            {
                finalImagePath = model.CurrentProfileImagePath;
            }

            photographer.FullName = model.FullName;
            photographer.Specialization = model.Specialization;
            photographer.Biography = model.Biography;
            photographer.ExperienceYears = model.ExperienceYears;
            photographer.Phone = model.Phone;
            photographer.Email = model.Email;
            photographer.TelegramLink = model.TelegramLink;
            photographer.InstagramLink = model.InstagramLink;
            photographer.ProfileImagePath = finalImagePath;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit));
        }

        private async Task<string?> SaveProfileImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new HashSet<string>
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
            {
                return null;
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadDirectory = Path.Combine(webRootPath, "images", "photographer");

            Directory.CreateDirectory(uploadDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/photographer/{fileName}";
        }
    }
}
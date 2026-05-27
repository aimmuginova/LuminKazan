using System.Globalization;
using LuminKazan.Data;
using LuminKazan.Models;
using LuminKazan.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        private static readonly string[] BookingStatuses =
        {
            "Новая",
            "В работе",
            "Подтверждена",
            "Завершена",
            "Отменена"
        };

        private static readonly TimeSpan BookingWorkdayStart = new TimeSpan(10, 0, 0);
        private static readonly TimeSpan BookingWorkdayEnd = new TimeSpan(20, 0, 0);

        private const int BookingBufferMinutes = 30;
        private const int BookingTimeSlotStepMinutes = 30;
        private const string CancelledBookingStatus = "Отменена";

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalBookings = await _context.Bookings.CountAsync(),
                TotalPortfolioItems = await _context.PortfolioItems.CountAsync(),
                TotalStudios = await _context.Studios.CountAsync(),
                TotalServices = await _context.Services.CountAsync()
            };

            return View(model);
        }

        public async Task<IActionResult> Bookings(string? status)
        {
            var query = _context.Bookings
                .Include(booking => booking.Service)
                .Include(booking => booking.Studio)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "Все")
            {
                query = query.Where(booking => booking.Status == status);
            }

            var bookings = await query
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentStatus = string.IsNullOrWhiteSpace(status) ? "Все" : status;

            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> EditBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(booking => booking.Service)
                .Include(booking => booking.Studio)
                .FirstOrDefaultAsync(booking => booking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            var model = new BookingEditViewModel
            {
                Id = booking.Id,
                ClientName = booking.ClientName,
                Phone = booking.Phone,
                Email = booking.Email,
                BookingDate = booking.BookingDate.Date,
                BookingTime = booking.BookingDate.ToString("HH:mm"),
                Comment = booking.Comment,
                ServiceId = booking.ServiceId,
                StudioId = booking.StudioId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };

            await FillBookingListsAsync(
                model,
                booking.ServiceId,
                booking.StudioId,
                booking.Status,
                model.BookingTime
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBooking(int id, BookingEditViewModel model)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(booking => booking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }
            var service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == model.ServiceId);

            if (service == null)
            {
                ModelState.AddModelError(nameof(model.ServiceId), "Выберите услугу.");
            }

            if (!TryBuildBookingStart(model.BookingDate, model.BookingTime, out var bookingStart))
            {
                ModelState.AddModelError(nameof(model.BookingTime), "Выберите корректное время съемки.");
            }

            var selectedStatus = string.IsNullOrWhiteSpace(model.Status)
                ? "Новая"
                : model.Status;

            if (service != null && bookingStart != default && selectedStatus != CancelledBookingStatus)
            {
                ValidateAdminBookingTime(bookingStart, service.DurationMinutes);

                if (ModelState.IsValid)
                {
                    var hasConflict = await HasAdminBookingConflictAsync(
                        bookingId: booking.Id,
                        bookingStart: bookingStart,
                        durationMinutes: service.DurationMinutes
                    );

                    if (hasConflict)
                    {
                        ModelState.AddModelError(
                            nameof(model.BookingTime),
                            "Выбранное время недоступно. Проверьте заявки и блокировки расписания."
                        );
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await FillBookingListsAsync(
                    model,
                    model.ServiceId,
                    model.StudioId,
                    selectedStatus,
                    model.BookingTime
                );

                return View(model);
            }

            booking.ClientName = model.ClientName;
            booking.Phone = model.Phone;
            booking.Email = model.Email;
            booking.BookingDate = bookingStart;
            booking.Comment = model.Comment;
            booking.ServiceId = model.ServiceId;
            booking.StudioId = model.StudioId;
            booking.Status = selectedStatus;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bookings));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(booking => booking.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Bookings));
        }

        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .OrderByDescending(service => service.IsActive)
                .ThenBy(service => service.Price)
                .ThenBy(service => service.Title)
                .ToListAsync();

            return View(services);
        }

        [HttpGet]
        public IActionResult CreateService()
        {
            var service = new Service
            {
                IsActive = true,
                DurationMinutes = 60,
                DeliveryDays = 7,
                RetouchedPhotosCount = 10,
                AllPhotosCount = 40
            };

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(Service service)
        {
            if (!ModelState.IsValid)
            {
                return View(service);
            }

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Services));
        }
        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(int id, Service model)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            service.Title = model.Title;
            service.Description = model.Description;
            service.DurationMinutes = model.DurationMinutes;
            service.Price = model.Price;
            service.IncludesStudio = model.IncludesStudio;
            service.RetouchedPhotosCount = model.RetouchedPhotosCount;
            service.AllPhotosCount = model.AllPhotosCount;
            service.DeliveryDays = model.DeliveryDays;
            service.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Services));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == id);

            if (service == null)
            {
                return NotFound();
            }

            service.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Services));
        }

        public async Task<IActionResult> Studios()
        {
            var studios = await _context.Studios
                .OrderByDescending(studio => studio.IsActive)
                .ThenBy(studio => studio.Name)
                .ToListAsync();

            return View(studios);
        }

        [HttpGet]
        public IActionResult CreateStudio()
        {
            var model = new StudioFormViewModel
            {
                IsActive = true,
                HasNaturalLight = true,
                HallCount = 1,
                PricePerHour = 0m
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudio(StudioFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile", "CoverImageFile"),
                "studios"
            );

            var imagePath = uploadedPath;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                imagePath = GetString(model, string.Empty, "ImagePath", "CurrentImagePath", "ExistingImagePath");
            }

            var studio = new Studio
            {
                Name = GetString(model, string.Empty, "Name"),
                District = GetString(model, string.Empty, "District"),
                Address = GetString(model, string.Empty, "Address"),
                Description = GetString(model, string.Empty, "Description"),
                PricePerHour = GetDecimal(model, 0m, "PricePerHour"),
                HallCount = GetInt(model, 1, "HallCount"),
                HasNaturalLight = GetBool(model, true, "HasNaturalLight"),
                InteriorStyle = GetString(model, string.Empty, "InteriorStyle"),
                ImagePath = imagePath,
                WebsiteUrl = GetString(model, string.Empty, "WebsiteUrl"),
                IsActive = GetBool(model, true, "IsActive")
            };

            _context.Studios.Add(studio);
            await _context.SaveChangesAsync(); return RedirectToAction(nameof(Studios));
        }

        [HttpGet]
        public async Task<IActionResult> EditStudio(int id)
        {
            var studio = await _context.Studios
                .FirstOrDefaultAsync(studio => studio.Id == id);

            if (studio == null)
            {
                return NotFound();
            }

            var model = new StudioFormViewModel
            {
                Id = studio.Id,
                Name = studio.Name,
                District = studio.District,
                Address = studio.Address,
                Description = studio.Description,
                PricePerHour = studio.PricePerHour,
                HallCount = studio.HallCount,
                HasNaturalLight = studio.HasNaturalLight,
                InteriorStyle = studio.InteriorStyle,
                ImagePath = studio.ImagePath,
                CurrentImagePath = studio.ImagePath,
                WebsiteUrl = studio.WebsiteUrl,
                IsActive = studio.IsActive
            };

            SetProperty(model, "ExistingImagePath", studio.ImagePath);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudio(int id, StudioFormViewModel model)
        {
            var studio = await _context.Studios
                .FirstOrDefaultAsync(studio => studio.Id == id);

            if (studio == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.CurrentImagePath = studio.ImagePath;
                SetProperty(model, "ExistingImagePath", studio.ImagePath);

                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile", "CoverImageFile"),
                "studios"
            );

            var typedPath = GetString(model, string.Empty, "ImagePath");

            var finalImagePath = studio.ImagePath;

            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                finalImagePath = uploadedPath;
            }
            else if (!string.IsNullOrWhiteSpace(typedPath))
            {
                finalImagePath = typedPath;
            }

            studio.Name = model.Name;
            studio.District = model.District;
            studio.Address = model.Address;
            studio.Description = model.Description;
            studio.PricePerHour = model.PricePerHour;
            studio.HallCount = model.HallCount;
            studio.HasNaturalLight = model.HasNaturalLight;
            studio.InteriorStyle = model.InteriorStyle;
            studio.ImagePath = finalImagePath;
            studio.WebsiteUrl = model.WebsiteUrl;
            studio.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Studios));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/ToggleStudioStatus/{id:int}")]
        public async Task<IActionResult> ToggleStudioStatus(int id)
        {
            var studio = await _context.Studios
                .FirstOrDefaultAsync(studio => studio.Id == id);

            if (studio == null)
            {
                return NotFound();
            }

            studio.IsActive = !studio.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Studios));
        }

        [HttpGet]
        [Route("Admin/ToggleStudioStatus/{id:int}")]
        public IActionResult ToggleStudioStatusGet(int id)
        {
            return RedirectToAction(nameof(Studios));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteStudio(int id)
        {
            var studio = await _context.Studios
                .FirstOrDefaultAsync(studio => studio.Id == id);

            if (studio == null)
            {
                return NotFound();
            }

            studio.IsActive = false; await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Studios));
        }

        public async Task<IActionResult> PortfolioItems()
        {
            var items = await _context.PortfolioItems
                .Include(item => item.Category)
                .Include(item => item.Photographer)
                .Include(item => item.Studio)
                .Include(item => item.Images)
                .OrderByDescending(item => item.ShootingDate)
                .ThenBy(item => item.Title)
                .ToListAsync();

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePortfolioItem()
        {
            var model = new PortfolioItemFormViewModel();

            SetProperty(model, "ShootingDate", DateTime.Today);
            SetProperty(model, "IsFeatured", false);

            await FillPortfolioItemListsAsync(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePortfolioItem(PortfolioItemFormViewModel model)
        {
            var categoryId = GetInt(model, 0, "CategoryId", "PortfolioCategoryId");
            var photographerId = GetInt(model, 0, "PhotographerId");
            var studioId = GetNullableInt(model, "StudioId");

            if (!ModelState.IsValid)
            {
                await FillPortfolioItemListsAsync(model, categoryId, photographerId, studioId);
                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile", "CoverImageFile"),
                "portfolio"
            );

            var typedPath = GetString(model, string.Empty, "ImagePath", "CoverImagePath");

            var imagePath = uploadedPath;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                imagePath = typedPath;
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                ModelState.AddModelError(string.Empty, "Добавьте обложку фотосерии.");
                await FillPortfolioItemListsAsync(model, categoryId, photographerId, studioId);

                return View(model);
            }

            var item = new PortfolioItem
            {
                Title = GetString(model, string.Empty, "Title"),
                Description = GetString(model, string.Empty, "Description"),
                ImagePath = imagePath,
                ShootingDate = GetNullableDateTime(model, "ShootingDate"),
                IsFeatured = GetBool(model, false, "IsFeatured"),
                CategoryId = categoryId,
                PhotographerId = photographerId,
                StudioId = studioId
            };

            _context.PortfolioItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioItems));
        }

        [HttpGet]
        public async Task<IActionResult> EditPortfolioItem(int id)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var model = new PortfolioItemFormViewModel(); SetProperty(model, "Id", item.Id);
            SetProperty(model, "Title", item.Title);
            SetProperty(model, "Description", item.Description);
            SetProperty(model, "ImagePath", item.ImagePath);
            SetProperty(model, "CoverImagePath", item.ImagePath);
            SetProperty(model, "CurrentImagePath", item.ImagePath);
            SetProperty(model, "CurrentCoverImagePath", item.ImagePath);
            SetProperty(model, "ShootingDate", item.ShootingDate);
            SetProperty(model, "IsFeatured", item.IsFeatured);
            SetProperty(model, "CategoryId", item.CategoryId);
            SetProperty(model, "PortfolioCategoryId", item.CategoryId);
            SetProperty(model, "PhotographerId", item.PhotographerId);
            SetProperty(model, "StudioId", item.StudioId);

            await FillPortfolioItemListsAsync(model, item.CategoryId, item.PhotographerId, item.StudioId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPortfolioItem(int id, PortfolioItemFormViewModel model)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            var categoryId = GetInt(model, item.CategoryId, "CategoryId", "PortfolioCategoryId");
            var photographerId = GetInt(model, item.PhotographerId, "PhotographerId");
            var studioId = GetNullableInt(model, "StudioId");

            if (!ModelState.IsValid)
            {
                SetProperty(model, "CurrentImagePath", item.ImagePath);
                SetProperty(model, "CurrentCoverImagePath", item.ImagePath);

                await FillPortfolioItemListsAsync(model, categoryId, photographerId, studioId);

                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile", "CoverImageFile"),
                "portfolio"
            );

            var typedPath = GetString(model, string.Empty, "ImagePath", "CoverImagePath");

            var finalImagePath = item.ImagePath;

            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                finalImagePath = uploadedPath;
            }
            else if (!string.IsNullOrWhiteSpace(typedPath))
            {
                finalImagePath = typedPath;
            }

            item.Title = GetString(model, item.Title, "Title");
            item.Description = GetString(model, item.Description ?? string.Empty, "Description");
            item.ImagePath = finalImagePath;
            item.ShootingDate = GetNullableDateTime(model, "ShootingDate");
            item.IsFeatured = GetBool(model, item.IsFeatured, "IsFeatured");
            item.CategoryId = categoryId;
            item.PhotographerId = photographerId;
            item.StudioId = studioId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioItems));
        }

        [HttpGet]
        public async Task<IActionResult> DeletePortfolioItem(int id)
        {
            var item = await _context.PortfolioItems
                .Include(item => item.Images)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            _context.PortfolioItems.Remove(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioItems));
        }

        [HttpGet]
        public async Task<IActionResult> PortfolioImages(int portfolioItemId)
        {
            var item = await _context.PortfolioItems
                .Include(item => item.Category)
                .Include(item => item.Photographer)
                .Include(item => item.Studio)
                .Include(item => item.Images)
                .FirstOrDefaultAsync(item => item.Id == portfolioItemId); if (item == null)
            {
                return NotFound();
            }

            item.Images = item.Images
                .OrderBy(image => image.SortOrder)
                .ThenBy(image => image.Id)
                .ToList();

            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> CreatePortfolioImage(int portfolioItemId)
        {
            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(item => item.Id == portfolioItemId);

            if (item == null)
            {
                return NotFound();
            }

            var maxSortOrder = await _context.PortfolioImages
                .Where(image => image.PortfolioItemId == portfolioItemId)
                .Select(image => (int?)image.SortOrder)
                .MaxAsync();

            var model = new PortfolioImageFormViewModel
            {
                PortfolioItemId = item.Id,
                PortfolioItemTitle = item.Title,
                SortOrder = (maxSortOrder ?? 0) + 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePortfolioImage(PortfolioImageFormViewModel model)
        {
            var portfolioItemId = GetInt(model, 0, "PortfolioItemId");

            var item = await _context.PortfolioItems
                .FirstOrDefaultAsync(item => item.Id == portfolioItemId);

            if (item == null)
            {
                return NotFound();
            }

            model.PortfolioItemTitle = item.Title;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile"),
                "portfolio"
            );

            var typedPath = GetString(model, string.Empty, "ImagePath");

            var imagePath = uploadedPath;

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                imagePath = typedPath;
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                ModelState.AddModelError(string.Empty, "Добавьте фотографию.");
                return View(model);
            }

            var image = new PortfolioImage
            {
                PortfolioItemId = portfolioItemId,
                ImagePath = imagePath,
                Caption = GetString(model, string.Empty, "Caption"),
                SortOrder = GetInt(model, 1, "SortOrder")
            };

            _context.PortfolioImages.Add(image);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioImages), new { portfolioItemId });
        }

        [HttpGet]
        public async Task<IActionResult> EditPortfolioImage(int id)
        {
            var image = await _context.PortfolioImages
                .Include(image => image.PortfolioItem)
                .FirstOrDefaultAsync(image => image.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var model = new PortfolioImageFormViewModel
            {
                Id = image.Id,
                PortfolioItemId = image.PortfolioItemId,
                PortfolioItemTitle = image.PortfolioItem?.Title ?? string.Empty,
                ImagePath = image.ImagePath,
                CurrentImagePath = image.ImagePath,
                Caption = image.Caption,
                SortOrder = image.SortOrder
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPortfolioImage(int id, PortfolioImageFormViewModel model)
        {
            var image = await _context.PortfolioImages
                .Include(image => image.PortfolioItem)
                .FirstOrDefaultAsync(image => image.Id == id);

            if (image == null)
            {
                return NotFound();
            }
            model.PortfolioItemId = image.PortfolioItemId;
            model.PortfolioItemTitle = image.PortfolioItem?.Title ?? string.Empty;
            model.CurrentImagePath = image.ImagePath;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uploadedPath = await SaveUploadedFileAsync(
                GetFile(model, "ImageFile"),
                "portfolio"
            );

            var typedPath = GetString(model, string.Empty, "ImagePath");

            var finalImagePath = image.ImagePath;

            if (!string.IsNullOrWhiteSpace(uploadedPath))
            {
                finalImagePath = uploadedPath;
            }
            else if (!string.IsNullOrWhiteSpace(typedPath))
            {
                finalImagePath = typedPath;
            }

            image.ImagePath = finalImagePath;
            image.Caption = GetString(model, string.Empty, "Caption");
            image.SortOrder = GetInt(model, image.SortOrder, "SortOrder");

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioImages), new { portfolioItemId = image.PortfolioItemId });
        }

        [HttpGet]
        public async Task<IActionResult> DeletePortfolioImage(int id)
        {
            var image = await _context.PortfolioImages
                .FirstOrDefaultAsync(image => image.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            var portfolioItemId = image.PortfolioItemId;

            _context.PortfolioImages.Remove(image);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PortfolioImages), new { portfolioItemId });
        }


        public async Task<IActionResult> Reviews()
        {
            var reviews = await _context.Reviews
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishReview(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(review => review.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            review.IsPublished = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideReview(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(review => review.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            review.IsPublished = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reviews));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(review => review.Id == id);

            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reviews));
        }

        private async Task FillBookingListsAsync(
            BookingEditViewModel model,
            int selectedServiceId = 0,
            int? selectedStudioId = null,
            string? selectedStatus = null,
            string? selectedTime = null)
        {
            model.Services = await _context.Services
                .OrderBy(service => service.Title)
                .Select(service => new SelectListItem
                {
                    Value = service.Id.ToString(),
                    Text = service.Title + " - " + service.DurationMinutes + " мин",
                    Selected = service.Id == selectedServiceId
                })
                .ToListAsync();

            model.Studios = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Без студии",
                    Selected = !selectedStudioId.HasValue
                }
            };

            var studioItems = await _context.Studios
                .OrderBy(studio => studio.Name)
                .Select(studio => new SelectListItem
                {
                    Value = studio.Id.ToString(),
                    Text = studio.Name,
                    Selected = selectedStudioId.HasValue && studio.Id == selectedStudioId.Value
                })
                .ToListAsync();

            model.Studios.AddRange(studioItems);

            model.Statuses = BookingStatuses
                .Select(status => new SelectListItem
                {
                    Value = status,
                    Text = status,
                    Selected = string.Equals(status, selectedStatus, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            model.TimeSlots = BuildAdminBookingTimeSlotItems(selectedTime);
        }
        private async Task FillPortfolioItemListsAsync(
            object model,
            int selectedCategoryId = 0,
            int selectedPhotographerId = 0,
            int? selectedStudioId = null)
        {
            var categories = await _context.PortfolioCategories
                .OrderBy(category => category.Name)
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name,
                    Selected = category.Id == selectedCategoryId
                })
                .ToListAsync();

            var photographers = await _context.Photographers
                .OrderBy(photographer => photographer.FullName)
                .Select(photographer => new SelectListItem
                {
                    Value = photographer.Id.ToString(),
                    Text = photographer.FullName,
                    Selected = photographer.Id == selectedPhotographerId
                })
                .ToListAsync();

            var studios = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Без студии",
                    Selected = !selectedStudioId.HasValue
                }
            };

            var studioItems = await _context.Studios
                .OrderBy(studio => studio.Name)
                .Select(studio => new SelectListItem
                {
                    Value = studio.Id.ToString(),
                    Text = studio.Name,
                    Selected = selectedStudioId.HasValue && studio.Id == selectedStudioId.Value
                })
                .ToListAsync();

            studios.AddRange(studioItems);

            ViewBag.Categories = categories;
            ViewBag.CategoryOptions = categories;
            ViewBag.Photographers = photographers;
            ViewBag.PhotographerOptions = photographers;
            ViewBag.Studios = studios;
            ViewBag.StudioOptions = studios;

            SetProperty(model, "Categories", categories);
            SetProperty(model, "CategoryOptions", categories);
            SetProperty(model, "Photographers", photographers);
            SetProperty(model, "PhotographerOptions", photographers);
            SetProperty(model, "Studios", studios);
            SetProperty(model, "StudioOptions", studios);
        }

        private async Task<string?> SaveUploadedFileAsync(IFormFile? file, string folder)
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

            var uploadDirectory = Path.Combine(webRootPath, "images", folder);

            Directory.CreateDirectory(uploadDirectory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadDirectory, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/{folder}/{fileName}";
        }

        private static List<SelectListItem> BuildAdminBookingTimeSlotItems(string? selectedTime)
        {
            var items = new List<SelectListItem>();

            var time = BookingWorkdayStart;
            var lastStartTime = BookingWorkdayEnd.Subtract(TimeSpan.FromMinutes(BookingTimeSlotStepMinutes)); while (time <= lastStartTime)
            {
                var value = time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

                items.Add(new SelectListItem
                {
                    Value = value,
                    Text = value,
                    Selected = string.Equals(value, selectedTime, StringComparison.OrdinalIgnoreCase)
                });

                time = time.Add(TimeSpan.FromMinutes(BookingTimeSlotStepMinutes));
            }

            return items;
        }

        private static bool TryBuildBookingStart(DateTime date, string? timeText, out DateTime bookingStart)
        {
            bookingStart = default;

            if (string.IsNullOrWhiteSpace(timeText))
            {
                return false;
            }

            if (!TimeSpan.TryParseExact(timeText, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                if (!TimeSpan.TryParse(timeText, CultureInfo.InvariantCulture, out time))
                {
                    return false;
                }
            }

            bookingStart = date.Date.Add(time);

            return true;
        }

        private void ValidateAdminBookingTime(DateTime bookingStart, int durationMinutes)
        {
            if (bookingStart <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(BookingEditViewModel.BookingDate), "Выберите будущую дату и время съемки.");
            }

            if (bookingStart.TimeOfDay < BookingWorkdayStart)
            {
                ModelState.AddModelError(nameof(BookingEditViewModel.BookingTime), "Запись доступна с 10:00.");
            }

            var bookingEndWithBuffer = bookingStart.AddMinutes(durationMinutes + BookingBufferMinutes);

            if (bookingEndWithBuffer.Date != bookingStart.Date || bookingEndWithBuffer.TimeOfDay > BookingWorkdayEnd)
            {
                ModelState.AddModelError(
                    nameof(BookingEditViewModel.BookingTime),
                    "Выберите время, при котором съемка укладывается в рабочий день."
                );
            }
        }

        private async Task<bool> HasAdminBookingConflictAsync(
            int? bookingId,
            DateTime bookingStart,
            int durationMinutes)
        {
            var bookingEnd = bookingStart.AddMinutes(durationMinutes + BookingBufferMinutes);

            var dayStart = bookingStart.Date;
            var dayEnd = dayStart.AddDays(1);

            var existingBookings = await _context.Bookings
                .Include(booking => booking.Service)
                .Where(booking =>
                    booking.Status != CancelledBookingStatus &&
                    booking.BookingDate >= dayStart &&
                    booking.BookingDate < dayEnd)
                .ToListAsync();

            if (bookingId.HasValue)
            {
                existingBookings = existingBookings
                    .Where(booking => booking.Id != bookingId.Value)
                    .ToList();
            }

            foreach (var existingBooking in existingBookings)
            {
                var existingDuration = existingBooking.Service?.DurationMinutes ?? 60;
                var existingStart = existingBooking.BookingDate;
                var existingEnd = existingStart.AddMinutes(existingDuration + BookingBufferMinutes);

                var hasIntersection = existingStart < bookingEnd && existingEnd > bookingStart;

                if (hasIntersection)
                {
                    return true;
                }
            }

            var hasScheduleBlockConflict = await _context.ScheduleBlocks
                .AnyAsync(block =>
                    block.IsActive &&
                    block.StartAt < bookingEnd &&
                    block.EndAt > bookingStart);

            return hasScheduleBlockConflict;
        }
        private static void SetProperty(object target, string propertyName, object? value)
        {
            var property = target.GetType().GetProperty(propertyName);

            if (property == null || !property.CanWrite)
            {
                return;
            }

            if (value == null)
            {
                property.SetValue(target, null);
                return;
            }

            var propertyType = property.PropertyType;
            var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (propertyType.IsAssignableFrom(value.GetType()))
            {
                property.SetValue(target, value);
                return;
            }

            try
            {
                var convertedValue = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
                property.SetValue(target, convertedValue);
            }
            catch
            {
            }
        }

        private static string GetString(object source, string fallback, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                return value.ToString() ?? fallback;
            }

            return fallback;
        }

        private static int GetInt(object source, int fallback, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is int intValue)
                {
                    return intValue;
                }

                if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
                {
                    return parsedInvariant;
                }

                if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var parsedCurrent))
                {
                    return parsedCurrent;
                }
            }

            return fallback;
        }

        private static int? GetNullableInt(object source, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is int intValue)
                {
                    return intValue;
                }

                if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedInvariant))
                {
                    return parsedInvariant;
                }

                if (int.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out var parsedCurrent))
                {
                    return parsedCurrent;
                }
            }

            return null;
        }

        private static decimal GetDecimal(object source, decimal fallback, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName); if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is decimal decimalValue)
                {
                    return decimalValue;
                }

                var textValue = value.ToString();

                if (string.IsNullOrWhiteSpace(textValue))
                {
                    continue;
                }

                textValue = textValue.Replace(" ", "").Replace(",", ".");

                if (decimal.TryParse(
                    textValue,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsedValue))
                {
                    return parsedValue;
                }
            }

            return fallback;
        }

        private static bool GetBool(object source, bool fallback, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is bool boolValue)
                {
                    return boolValue;
                }

                if (bool.TryParse(value.ToString(), out var parsedValue))
                {
                    return parsedValue;
                }
            }

            return fallback;
        }

        private static DateTime GetDateTime(object source, DateTime fallback, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is DateTime dateTimeValue)
                {
                    return dateTimeValue;
                }

                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedCurrent))
                {
                    return parsedCurrent;
                }

                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedInvariant))
                {
                    return parsedInvariant;
                }
            }

            return fallback;
        }

        private static DateTime? GetNullableDateTime(object source, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value == null)
                {
                    continue;
                }

                if (value is DateTime dateTimeValue)
                {
                    return dateTimeValue;
                }

                if (DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out var parsedCurrent))
                {
                    return parsedCurrent;
                }

                if (DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedInvariant))
                {
                    return parsedInvariant;
                }
            }

            return null;
        }
        private static IFormFile? GetFile(object source, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                var property = source.GetType().GetProperty(propertyName);

                if (property == null)
                {
                    continue;
                }

                var value = property.GetValue(source);

                if (value is IFormFile file && file.Length > 0)
                {
                    return file;
                }
            }

            return null;
        }
    }
}
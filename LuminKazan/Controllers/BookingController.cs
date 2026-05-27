using System.Globalization;
using LuminKazan.Data;
using LuminKazan.Models;
using LuminKazan.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly TimeSpan WorkdayStart = new TimeSpan(10, 0, 0);
        private static readonly TimeSpan WorkdayEnd = new TimeSpan(20, 0, 0);

        private const int BookingBufferMinutes = 30;
        private const int TimeSlotStepMinutes = 30;
        private const string CancelledStatus = "Отменена";

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? studioId, int? serviceId)
        {
            var model = new BookingFormViewModel
            {
                Booking = new Booking
                {
                    BookingDate = DateTime.Today.AddDays(3),
                    StudioId = studioId,
                    ServiceId = serviceId ?? 0
                },
                BookingTime = "10:00"
            };

            await FillBookingFormListsAsync(model, serviceId, studioId, model.BookingTime);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingFormViewModel model)
        {
            await FillBookingFormListsAsync(
                model,
                model.Booking.ServiceId,
                model.Booking.StudioId,
                model.BookingTime
            );

            var service = await _context.Services
                .FirstOrDefaultAsync(service => service.Id == model.Booking.ServiceId && service.IsActive);

            if (service == null)
            {
                ModelState.AddModelError("Booking.ServiceId", "Выберите доступную услугу.");
            }

            if (!TryBuildBookingStart(model.Booking.BookingDate, model.BookingTime, out var bookingStart))
            {
                ModelState.AddModelError(nameof(model.BookingTime), "Выберите корректное время съемки.");
            }

            if (service != null && bookingStart != default)
            {
                ValidateBookingTime(bookingStart, service.DurationMinutes);

                if (ModelState.IsValid)
                {
                    var hasConflict = await HasBookingConflictAsync(
                        bookingId: null,
                        bookingStart: bookingStart,
                        durationMinutes: service.DurationMinutes
                    );

                    if (hasConflict)
                    {
                        ModelState.AddModelError(
                            nameof(model.BookingTime),
                            "Выбранное время недоступно. Выберите другое время съемки."
                        );
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Booking.BookingDate = bookingStart;
            model.Booking.CreatedAt = DateTime.Now;
            model.Booking.Status = "Новая";

            _context.Bookings.Add(model.Booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка отправлена. Мы свяжемся с вами для подтверждения съемки.";

            return RedirectToAction(nameof(Create), new { studioId = model.Booking.StudioId });
        }

        public async Task<IActionResult> List()
        {
            var bookings = await _context.Bookings
                .Include(booking => booking.Service)
                .Include(booking => booking.Studio)
                .OrderByDescending(booking => booking.CreatedAt)
                .ToListAsync();

            return View(bookings);
        }
        private async Task FillBookingFormListsAsync(
            BookingFormViewModel model,
            int? selectedServiceId = null,
            int? selectedStudioId = null,
            string? selectedTime = null)
        {
            model.Services = await _context.Services
                .Where(service => service.IsActive)
                .OrderBy(service => service.Price)
                .ThenBy(service => service.Title)
                .Select(service => new SelectListItem
                {
                    Value = service.Id.ToString(),
                    Text = service.Title + " - " + service.DurationMinutes + " мин",
                    Selected = selectedServiceId.HasValue && service.Id == selectedServiceId.Value
                })
                .ToListAsync();

            model.Studios = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = string.Empty,
                    Text = "Без студии / выберу позже",
                    Selected = !selectedStudioId.HasValue
                }
            };

            var studioItems = await _context.Studios
                .Where(studio => studio.IsActive)
                .OrderBy(studio => studio.Name)
                .Select(studio => new SelectListItem
                {
                    Value = studio.Id.ToString(),
                    Text = studio.Name,
                    Selected = selectedStudioId.HasValue && studio.Id == selectedStudioId.Value
                })
                .ToListAsync();

            model.Studios.AddRange(studioItems);

            model.TimeSlots = BuildTimeSlotItems(selectedTime);
        }

        private static List<SelectListItem> BuildTimeSlotItems(string? selectedTime)
        {
            var items = new List<SelectListItem>();

            var time = WorkdayStart;
            var lastStartTime = WorkdayEnd.Subtract(TimeSpan.FromMinutes(TimeSlotStepMinutes));

            while (time <= lastStartTime)
            {
                var value = time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

                items.Add(new SelectListItem
                {
                    Value = value,
                    Text = value,
                    Selected = string.Equals(value, selectedTime, StringComparison.OrdinalIgnoreCase)
                });

                time = time.Add(TimeSpan.FromMinutes(TimeSlotStepMinutes));
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

        private void ValidateBookingTime(DateTime bookingStart, int durationMinutes)
        {
            if (bookingStart <= DateTime.Now)
            {
                ModelState.AddModelError("Booking.BookingDate", "Выберите будущую дату и время съемки.");
            }

            if (bookingStart.TimeOfDay < WorkdayStart)
            {
                ModelState.AddModelError(nameof(BookingFormViewModel.BookingTime), "Запись доступна с 10:00.");
            }

            var bookingEndWithBuffer = bookingStart.AddMinutes(durationMinutes + BookingBufferMinutes);

            if (bookingEndWithBuffer.Date != bookingStart.Date || bookingEndWithBuffer.TimeOfDay > WorkdayEnd)
            {
                ModelState.AddModelError(
                    nameof(BookingFormViewModel.BookingTime),
                    "Выберите время, при котором съемка укладывается в рабочий день."
                );
            }
        }
        private async Task<bool> HasBookingConflictAsync(
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
                    booking.Status != CancelledStatus &&
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
    }
}
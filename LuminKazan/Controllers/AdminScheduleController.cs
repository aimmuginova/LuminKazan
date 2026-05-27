using System.Globalization;
using LuminKazan.Data;
using LuminKazan.Models;
using LuminKazan.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly TimeSpan WorkdayStart = new TimeSpan(10, 0, 0);
        private static readonly TimeSpan WorkdayEnd = new TimeSpan(20, 0, 0);
        private const int TimeSlotStepMinutes = 30;

        public AdminScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string period = "30", int? year = null, int? month = null, string? selectedDate = null)
        {
            var today = DateTime.Today;
            var normalizedPeriod = NormalizePeriod(period);
            var selectedDateValue = ParseSelectedDate(selectedDate);

            var calendarMonth = selectedDateValue.HasValue
                ? new DateTime(selectedDateValue.Value.Year, selectedDateValue.Value.Month, 1)
                : BuildCalendarMonth(year, month);

            var periodEnd = GetPeriodEnd(today, normalizedPeriod);
            var calendarStart = GetCalendarStart(calendarMonth);
            var calendarEnd = calendarStart.AddDays(42);

            var bookingsQuery = _context.Bookings
                .Include(booking => booking.Service)
                .Include(booking => booking.Studio)
                .Where(booking =>
                    booking.BookingDate >= today &&
                    booking.Status != "Отменена");

            if (selectedDateValue.HasValue)
            {
                var dayStart = selectedDateValue.Value.Date;
                var dayEnd = dayStart.AddDays(1);

                bookingsQuery = bookingsQuery.Where(booking =>
                    booking.BookingDate >= dayStart &&
                    booking.BookingDate < dayEnd);
            }
            else if (periodEnd.HasValue)
            {
                bookingsQuery = bookingsQuery.Where(booking => booking.BookingDate < periodEnd.Value);
            }

            var upcomingBookings = await bookingsQuery
                .OrderBy(booking => booking.BookingDate)
                .ThenBy(booking => booking.CreatedAt)
                .ToListAsync();

            var scheduleBlocks = await _context.ScheduleBlocks
                .OrderByDescending(block => block.StartAt)
                .ToListAsync();

            var calendarBookings = await _context.Bookings
                .Include(booking => booking.Service)
                .Include(booking => booking.Studio)
                .Where(booking =>
                    booking.BookingDate >= calendarStart &&
                    booking.BookingDate < calendarEnd &&
                    booking.Status != "Отменена")
                .OrderBy(booking => booking.BookingDate)
                .ToListAsync();

            var calendarBlocks = await _context.ScheduleBlocks
                .Where(block =>
                    block.StartAt < calendarEnd &&
                    block.EndAt >= calendarStart &&
                    block.IsActive)
                .OrderBy(block => block.StartAt)
                .ToListAsync();

            var model = new AdminScheduleIndexViewModel
            {
                CurrentPeriod = normalizedPeriod,
                PeriodStart = today,
                PeriodEnd = periodEnd,
                CalendarMonth = calendarMonth,
                PreviousMonth = calendarMonth.AddMonths(-1),
                NextMonth = calendarMonth.AddMonths(1),
                SelectedDate = selectedDateValue,
                UpcomingBookings = upcomingBookings,
                ScheduleBlocks = scheduleBlocks,
                CalendarDays = BuildCalendarDays(calendarMonth, calendarStart, calendarBookings, calendarBlocks, selectedDateValue)
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ScheduleBlockFormViewModel
            {
                Date = DateTime.Today,
                StartTime = "10:00",
                EndTime = "20:00",
                IsFullDay = false,
                IsActive = true,
                TimeSlots = BuildTimeSlotItems()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleBlockFormViewModel model)
        {
            NormalizeFullDayTime(model);

            if (!TryBuildDateTimeRange(model, out var startAt, out var endAt))
            {
                ModelState.AddModelError(string.Empty, "Выберите корректный период недоступности.");
            }

            ValidateScheduleBlockTime(model, startAt, endAt);

            if (!ModelState.IsValid)
            {
                model.TimeSlots = BuildTimeSlotItems(model.StartTime, model.EndTime);
                return View(model);
            }

            var scheduleBlock = new ScheduleBlock
            {
                StartAt = startAt,
                EndAt = endAt,
                Reason = model.Reason,
                IsActive = model.IsActive,
                CreatedAt = DateTime.Now
            };

            _context.ScheduleBlocks.Add(scheduleBlock);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var block = await _context.ScheduleBlocks
                .FirstOrDefaultAsync(block => block.Id == id);

            if (block == null)
            {
                return NotFound();
            }

            var isFullDay =
                block.StartAt.TimeOfDay == WorkdayStart &&
                block.EndAt.TimeOfDay == WorkdayEnd;

            var model = new ScheduleBlockFormViewModel
            {
                Id = block.Id,
                Date = block.StartAt.Date,
                StartTime = block.StartAt.ToString("HH:mm"),
                EndTime = block.EndAt.ToString("HH:mm"),
                IsFullDay = isFullDay,
                Reason = block.Reason,
                IsActive = block.IsActive,
                TimeSlots = BuildTimeSlotItems(block.StartAt.ToString("HH:mm"), block.EndAt.ToString("HH:mm"))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ScheduleBlockFormViewModel model)
        {
            var block = await _context.ScheduleBlocks
                .FirstOrDefaultAsync(block => block.Id == id);

            if (block == null)
            {
                return NotFound();
            }

            NormalizeFullDayTime(model);

            if (!TryBuildDateTimeRange(model, out var startAt, out var endAt))
            {
                ModelState.AddModelError(string.Empty, "Выберите корректный период недоступности.");
            }

            ValidateScheduleBlockTime(model, startAt, endAt);

            if (!ModelState.IsValid)
            {
                model.Id = id;
                model.TimeSlots = BuildTimeSlotItems(model.StartTime, model.EndTime);
                return View(model);
            }

            block.StartAt = startAt;
            block.EndAt = endAt;
            block.Reason = model.Reason;
            block.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var block = await _context.ScheduleBlocks
                .FirstOrDefaultAsync(block => block.Id == id);

            if (block == null)
            {
                return NotFound();
            }

            block.IsActive = !block.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var block = await _context.ScheduleBlocks
                .FirstOrDefaultAsync(block => block.Id == id);

            if (block == null)
            {
                return NotFound();
            }

            _context.ScheduleBlocks.Remove(block);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private static string NormalizePeriod(string? period)
        {
            return period switch
            {
                "today" => "today",
                "7" => "7",
                "30" => "30",
                "all" => "all",
                _ => "30"
            };
        }

        private static DateTime? GetPeriodEnd(DateTime today, string period)
        {
            return period switch
            {
                "today" => today.AddDays(1),
                "7" => today.AddDays(7),
                "30" => today.AddDays(30),
                "all" => null,
                _ => today.AddDays(30)
            };
        }

        private static DateTime BuildCalendarMonth(int? year, int? month)
        {
            var today = DateTime.Today;

            var selectedYear = year ?? today.Year;
            var selectedMonth = month ?? today.Month;

            if (selectedMonth < 1 || selectedMonth > 12)
            {
                selectedMonth = today.Month;
            }

            if (selectedYear < 2000 || selectedYear > 2100)
            {
                selectedYear = today.Year;
            }

            return new DateTime(selectedYear, selectedMonth, 1);
        }

        private static DateTime GetCalendarStart(DateTime calendarMonth)
        {
            var firstDayOfMonth = new DateTime(calendarMonth.Year, calendarMonth.Month, 1);
            var dayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            var mondayBasedOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;

            return firstDayOfMonth.AddDays(-mondayBasedOffset);
        }

        private static DateTime? ParseSelectedDate(string? selectedDate)
        {
            if (string.IsNullOrWhiteSpace(selectedDate))
            {
                return null;
            }

            if (DateTime.TryParseExact(
                    selectedDate,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                return parsedDate.Date;
            }

            return null;
        }

        private static List<AdminScheduleCalendarDayViewModel> BuildCalendarDays(
            DateTime calendarMonth,
            DateTime calendarStart,
            List<Booking> bookings,
            List<ScheduleBlock> blocks,
            DateTime? selectedDate)
        {
            var days = new List<AdminScheduleCalendarDayViewModel>();

            for (var index = 0; index < 42; index++)
            {
                var date = calendarStart.AddDays(index);

                var dayBookings = bookings
                    .Where(booking => booking.BookingDate.Date == date.Date)
                    .OrderBy(booking => booking.BookingDate)
                    .ToList();

                var dayBlocks = blocks
                    .Where(block => block.StartAt.Date == date.Date)
                    .OrderBy(block => block.StartAt)
                    .ToList();

                days.Add(new AdminScheduleCalendarDayViewModel
                {
                    Date = date,
                    IsCurrentMonth = date.Month == calendarMonth.Month && date.Year == calendarMonth.Year,
                    IsToday = date.Date == DateTime.Today,
                    IsSelected = selectedDate.HasValue && date.Date == selectedDate.Value.Date,
                    Bookings = dayBookings,
                    Blocks = dayBlocks
                });
            }

            return days;
        }

        private static void NormalizeFullDayTime(ScheduleBlockFormViewModel model)
        {
            if (!model.IsFullDay)
            {
                return;
            }

            model.StartTime = WorkdayStart.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
            model.EndTime = WorkdayEnd.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }

        private static bool TryBuildDateTimeRange(
            ScheduleBlockFormViewModel model,
            out DateTime startAt,
            out DateTime endAt)
        {
            startAt = default;
            endAt = default;

            if (!TimeSpan.TryParseExact(model.StartTime, @"hh\:mm", CultureInfo.InvariantCulture, out var startTime))
            {
                return false;
            }

            if (!TimeSpan.TryParseExact(model.EndTime, @"hh\:mm", CultureInfo.InvariantCulture, out var endTime))
            {
                return false;
            }

            startAt = model.Date.Date.Add(startTime);
            endAt = model.Date.Date.Add(endTime);

            return true;
        }

        private void ValidateScheduleBlockTime(
            ScheduleBlockFormViewModel model,
            DateTime startAt,
            DateTime endAt)
        {
            if (startAt == default || endAt == default)
            {
                return;
            }

            if (endAt <= startAt)
            {
                ModelState.AddModelError(nameof(model.EndTime), "Время окончания должно быть позже времени начала.");
            }

            if (startAt.TimeOfDay < WorkdayStart)
            {
                ModelState.AddModelError(nameof(model.StartTime), "Время начала не может быть раньше 10:00.");
            }

            if (endAt.TimeOfDay > WorkdayEnd)
            {
                ModelState.AddModelError(nameof(model.EndTime), "Время окончания не может быть позже 20:00.");
            }

            if (startAt.Date != endAt.Date)
            {
                ModelState.AddModelError(nameof(model.EndTime), "Период недоступности должен быть в рамках одного дня.");
            }
        }

        private static List<SelectListItem> BuildTimeSlotItems(
            string? selectedStartTime = null,
            string? selectedEndTime = null)
        {
            var items = new List<SelectListItem>();

            var currentTime = WorkdayStart;

            while (currentTime <= WorkdayEnd)
            {
                var value = currentTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);

                items.Add(new SelectListItem
                {
                    Value = value,
                    Text = value,
                    Selected = string.Equals(value, selectedStartTime, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(value, selectedEndTime, StringComparison.OrdinalIgnoreCase)
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(TimeSlotStepMinutes));
            }

            return items;
        }
    }
}
using LuminKazan.Models;

namespace LuminKazan.ViewModels
{
    public class AdminScheduleIndexViewModel
    {
        public List<Booking> UpcomingBookings { get; set; } = new List<Booking>();

        public List<ScheduleBlock> ScheduleBlocks { get; set; } = new List<ScheduleBlock>();

        public List<AdminScheduleCalendarDayViewModel> CalendarDays { get; set; } = new List<AdminScheduleCalendarDayViewModel>();

        public string CurrentPeriod { get; set; } = "30";

        public DateTime PeriodStart { get; set; }

        public DateTime? PeriodEnd { get; set; }

        public DateTime CalendarMonth { get; set; } = DateTime.Today;

        public DateTime PreviousMonth { get; set; }

        public DateTime NextMonth { get; set; }

        public DateTime? SelectedDate { get; set; }
    }

    public class AdminScheduleCalendarDayViewModel
    {
        public DateTime Date { get; set; }

        public bool IsCurrentMonth { get; set; }

        public bool IsToday { get; set; }

        public bool IsSelected { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();

        public List<ScheduleBlock> Blocks { get; set; } = new List<ScheduleBlock>();
    }
}
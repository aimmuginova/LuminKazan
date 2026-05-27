using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuminKazan.ViewModels
{
    public class ScheduleBlockFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Выберите дату")]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Выберите время начала")]
        public string StartTime { get; set; } = "10:00";

        [Required(ErrorMessage = "Выберите время окончания")]
        public string EndTime { get; set; } = "20:00";

        public bool IsFullDay { get; set; }

        [StringLength(300, ErrorMessage = "Причина не должна быть длиннее 300 символов")]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        public List<SelectListItem> TimeSlots { get; set; } = new List<SelectListItem>();
    }
}
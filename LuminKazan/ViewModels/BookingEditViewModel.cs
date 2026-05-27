using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuminKazan.ViewModels
{
    public class BookingEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100, ErrorMessage = "Имя не должно быть длиннее 100 символов")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите телефон")]
        [StringLength(30, ErrorMessage = "Телефон не должен быть длиннее 30 символов")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [StringLength(100, ErrorMessage = "Email не должен быть длиннее 100 символов")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите дату съемки")]
        public DateTime BookingDate { get; set; }

        [Required(ErrorMessage = "Выберите время съемки")]
        public string BookingTime { get; set; } = "10:00";

        [Required(ErrorMessage = "Выберите услугу")]
        public int ServiceId { get; set; }

        public int? StudioId { get; set; }

        [Required(ErrorMessage = "Выберите статус заявки")]
        public string Status { get; set; } = "Новая";

        [StringLength(500, ErrorMessage = "Комментарий не должен быть длиннее 500 символов")]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Studios { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> TimeSlots { get; set; } = new List<SelectListItem>();
    }
}
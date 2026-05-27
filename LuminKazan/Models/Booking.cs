using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        [StringLength(100)]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите телефон")]
        [StringLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите дату")]
        public DateTime BookingDate { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [ValidateNever]
        public Service? Service { get; set; }

        public int? StudioId { get; set; }

        [ValidateNever]
        public Studio? Studio { get; set; }

        public string Status { get; set; } = "Новая";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
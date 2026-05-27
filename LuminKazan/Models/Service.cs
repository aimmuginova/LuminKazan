using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название услуги")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите описание услуги")]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите длительность съемки")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Укажите цену")]
        public decimal Price { get; set; }

        public bool IncludesStudio { get; set; }

        public int RetouchedPhotosCount { get; set; }

        public int AllPhotosCount { get; set; }

        public int DeliveryDays { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
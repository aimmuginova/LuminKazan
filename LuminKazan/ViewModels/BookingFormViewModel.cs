using System.ComponentModel.DataAnnotations;
using LuminKazan.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LuminKazan.ViewModels
{
    public class BookingFormViewModel
    {
        public Booking Booking { get; set; } = new Booking();

        [Required(ErrorMessage = "Выберите время съемки")]
        public string BookingTime { get; set; } = "10:00";

        public List<SelectListItem> Services { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> Studios { get; set; } = new List<SelectListItem>();

        public List<SelectListItem> TimeSlots { get; set; } = new List<SelectListItem>();
    }
}
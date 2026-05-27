using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class Studio
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название студии")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите адрес студии")]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(1500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PricePerHour { get; set; }

        [Range(1, 50, ErrorMessage = "Количество залов должно быть от 1 до 50")]
        public int HallCount { get; set; }

        public bool HasNaturalLight { get; set; }

        [StringLength(100)]
        public string? InteriorStyle { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [StringLength(255)]
        public string? WebsiteUrl { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();

        [ValidateNever]
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
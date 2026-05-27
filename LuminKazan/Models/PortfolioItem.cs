using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class PortfolioItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название фотосерии")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Укажите путь к обложке фотосерии")]
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        public DateTime? ShootingDate { get; set; }

        public bool IsFeatured { get; set; } = false;

        [Required(ErrorMessage = "Выберите категорию")]
        public int CategoryId { get; set; }

        [ValidateNever]
        public PortfolioCategory Category { get; set; } = null!;

        [Required(ErrorMessage = "Выберите фотографа")]
        public int PhotographerId { get; set; }

        [ValidateNever]
        public Photographer Photographer { get; set; } = null!;

        public int? StudioId { get; set; }

        [ValidateNever]
        public Studio? Studio { get; set; }

        [ValidateNever]
        public ICollection<PortfolioImage> Images { get; set; } = new List<PortfolioImage>();
    }
}
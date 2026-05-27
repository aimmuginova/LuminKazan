using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class PortfolioImage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите путь к фотографии")]
        [StringLength(255)]
        public string ImagePath { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Caption { get; set; }

        public int SortOrder { get; set; }

        [Required]
        public int PortfolioItemId { get; set; }

        [ValidateNever]
        public PortfolioItem PortfolioItem { get; set; } = null!;
    }
}
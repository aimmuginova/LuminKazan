using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LuminKazan.ViewModels
{
    public class PortfolioImageFormViewModel
    {
        public int Id { get; set; }

        [Required]
        public int PortfolioItemId { get; set; }

        public string PortfolioItemTitle { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public IFormFile? ImageFile { get; set; }

        [StringLength(300)]
        public string? Caption { get; set; }

        [Range(1, 1000, ErrorMessage = "Порядок отображения должен быть от 1 до 1000")]
        public int SortOrder { get; set; } = 1;

        public string? CurrentImagePath { get; set; }
    }
}
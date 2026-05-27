using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace LuminKazan.Models
{
    public class Photographer
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(150)]
        public string? Specialization { get; set; }

        [StringLength(2000)]
        public string? Biography { get; set; }

        [StringLength(255)]
        public string? ProfileImagePath { get; set; }

        [Range(0, 60)]
        public int ExperienceYears { get; set; }

        [Phone]
        [StringLength(30)]
        public string? Phone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? TelegramLink { get; set; }

        [StringLength(255)]
        public string? InstagramLink { get; set; }

        [ValidateNever]
        public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();
    }
}
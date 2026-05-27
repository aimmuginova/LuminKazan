using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LuminKazan.ViewModels
{
    public class PhotographerProfileViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите имя фотографа")]
        [StringLength(100, ErrorMessage = "Имя не должно быть длиннее 100 символов")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Специализация не должна быть длиннее 150 символов")]
        public string? Specialization { get; set; }

        [StringLength(2000, ErrorMessage = "Описание не должно быть длиннее 2000 символов")]
        public string? Biography { get; set; }

        [Range(0, 60, ErrorMessage = "Опыт должен быть от 0 до 60 лет")]
        public int ExperienceYears { get; set; }

        [Phone(ErrorMessage = "Введите корректный телефон")]
        [StringLength(30, ErrorMessage = "Телефон не должен быть длиннее 30 символов")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Введите корректный email")]
        [StringLength(100, ErrorMessage = "Email не должен быть длиннее 100 символов")]
        public string? Email { get; set; }

        [StringLength(255, ErrorMessage = "Ссылка не должна быть длиннее 255 символов")]
        public string? TelegramLink { get; set; }

        [StringLength(255, ErrorMessage = "Ссылка не должна быть длиннее 255 символов")]
        public string? InstagramLink { get; set; }

        [StringLength(255)]
        public string? ProfileImagePath { get; set; }

        [StringLength(255)]
        public string? CurrentProfileImagePath { get; set; }

        public IFormFile? ProfileImageFile { get; set; }
    }
}

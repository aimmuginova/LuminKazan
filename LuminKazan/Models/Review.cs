using System.ComponentModel.DataAnnotations;

namespace LuminKazan.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Укажите имя")]
        [StringLength(100, ErrorMessage = "Имя не должно быть длиннее 100 символов")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Напишите текст отзыва")]
        [StringLength(1200, ErrorMessage = "Отзыв не должен быть длиннее 1200 символов")]
        public string Text { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5")]
        public int Rating { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = false;
    }
}
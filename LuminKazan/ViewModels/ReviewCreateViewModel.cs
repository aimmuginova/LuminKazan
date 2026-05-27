using System.ComponentModel.DataAnnotations;

namespace LuminKazan.ViewModels
{
    public class ReviewCreateViewModel
    {
        [Required(ErrorMessage = "Укажите имя")]
        [StringLength(100, ErrorMessage = "Имя не должно быть длиннее 100 символов")]
        public string ClientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите текст отзыва")]
        [StringLength(1200, ErrorMessage = "Отзыв не должен быть длиннее 1200 символов")]
        public string Text { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Выберите оценку от 1 до 5")]
        public int Rating { get; set; } = 5;
    }
}
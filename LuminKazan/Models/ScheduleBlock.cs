using System.ComponentModel.DataAnnotations;

namespace LuminKazan.Models
{
    public class ScheduleBlock
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartAt { get; set; }

        [Required]
        public DateTime EndAt { get; set; }

        [StringLength(300)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
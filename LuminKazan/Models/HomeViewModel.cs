namespace LuminKazan.Models
{
    public class HomeViewModel
    {
        public Photographer? Photographer { get; set; }
        public List<Service> Services { get; set; } = new();
        public List<Studio> Studios { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
    }
}

using LuminKazan.Models;

namespace LuminKazan.ViewModels
{
    public class HomeViewModel
    {
        public Photographer? Photographer { get; set; }

        public List<Service> Services { get; set; } = new List<Service>();

        public List<Studio> Studios { get; set; } = new List<Studio>();

        public List<Review> Reviews { get; set; } = new List<Review>();
    }
}
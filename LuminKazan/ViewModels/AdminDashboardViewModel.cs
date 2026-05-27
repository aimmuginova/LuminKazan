namespace LuminKazan.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalBookings { get; set; }

        public int TotalPortfolioItems { get; set; }

        public int TotalStudios { get; set; }

        public int TotalServices { get; set; }

        public int BookingsCount
        {
            get => TotalBookings;
            set => TotalBookings = value;
        }

        public int PortfolioItemsCount
        {
            get => TotalPortfolioItems;
            set => TotalPortfolioItems = value;
        }

        public int StudiosCount
        {
            get => TotalStudios;
            set => TotalStudios = value;
        }

        public int ServicesCount
        {
            get => TotalServices;
            set => TotalServices = value;
        }
    }
}
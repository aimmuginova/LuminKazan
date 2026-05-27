using LuminKazan.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Photographer> Photographers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Studio> Studios { get; set; }
        public DbSet<PortfolioCategory> PortfolioCategories { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<PortfolioImage> PortfolioImages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<ScheduleBlock> ScheduleBlocks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Service>()
                .Property(service => service.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Studio>()
                .Property(studio => studio.PricePerHour)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Booking>()
                .HasOne(booking => booking.Service)
                .WithMany(service => service.Bookings)
                .HasForeignKey(booking => booking.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(booking => booking.Studio)
                .WithMany(studio => studio.Bookings)
                .HasForeignKey(booking => booking.StudioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PortfolioItem>()
                .HasOne(item => item.Category)
                .WithMany(category => category.PortfolioItems)
                .HasForeignKey(item => item.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PortfolioItem>()
                .HasOne(item => item.Photographer)
                .WithMany(photographer => photographer.PortfolioItems)
                .HasForeignKey(item => item.PhotographerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PortfolioItem>()
                .HasOne(item => item.Studio)
                .WithMany(studio => studio.PortfolioItems)
                .HasForeignKey(item => item.StudioId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PortfolioImage>()
                .HasOne(image => image.PortfolioItem)
                .WithMany(item => item.Images)
                .HasForeignKey(image => image.PortfolioItemId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ScheduleBlock>()
            .Property(block => block.Reason)
            .HasMaxLength(300);

            modelBuilder.Entity<ScheduleBlock>()
                .HasIndex(block => new { block.StartAt, block.EndAt, block.IsActive });

            modelBuilder.Entity<Photographer>().HasData(
                new Photographer
                {
                    Id = 1,
                    FullName = "Алина Валеева",
                    Specialization = "Современный художественный и коммерческий фотограф",
                    Biography = "Профессиональный фотограф из Казани, работающий в жанрах индивидуальной, семейной, fashion и контент-съемки. Основной акцент - на эстетике, свете и живой эмоциональной подаче.",
                    ProfileImagePath = "",
                    ExperienceYears = 5,
                    Phone = "",
                    Email = "",
                    TelegramLink = "",
                    InstagramLink = ""
                }
            ); modelBuilder.Entity<Service>().HasData(
                new Service
                {
                    Id = 1,
                    Title = "Мини-съемка",
                    Description = "Короткая фотосессия для обновления контента или личных фото.",
                    DurationMinutes = 30,
                    Price = 3500m,
                    RetouchedPhotosCount = 10,
                    AllPhotosCount = 40,
                    DeliveryDays = 5,
                    IncludesStudio = true,
                    IsActive = true
                },
                new Service
                {
                    Id = 2,
                    Title = "Индивидуальная съемка",
                    Description = "Полноценная персональная фотосессия с проработкой образа и локации.",
                    DurationMinutes = 60,
                    Price = 7000m,
                    RetouchedPhotosCount = 20,
                    AllPhotosCount = 80,
                    DeliveryDays = 7,
                    IncludesStudio = false,
                    IsActive = true
                },
                new Service
                {
                    Id = 3,
                    Title = "Love Story",
                    Description = "Эмоциональная и эстетичная фотосессия для пары.",
                    DurationMinutes = 90,
                    Price = 9500m,
                    RetouchedPhotosCount = 25,
                    AllPhotosCount = 100,
                    DeliveryDays = 7,
                    IncludesStudio = false,
                    IsActive = true
                },
                new Service
                {
                    Id = 4,
                    Title = "Семейная съемка",
                    Description = "Фотосессия для семьи в студии или на выездной локации.",
                    DurationMinutes = 90,
                    Price = 9000m,
                    RetouchedPhotosCount = 25,
                    AllPhotosCount = 100,
                    DeliveryDays = 7,
                    IncludesStudio = false,
                    IsActive = true
                },
                new Service
                {
                    Id = 5,
                    Title = "Контент-съемка для бренда",
                    Description = "Создание фото-контента для бизнеса, блогеров и экспертов.",
                    DurationMinutes = 120,
                    Price = 12000m,
                    RetouchedPhotosCount = 30,
                    AllPhotosCount = 150,
                    DeliveryDays = 10,
                    IncludesStudio = false,
                    IsActive = true
                },
                new Service
                {
                    Id = 6,
                    Title = "Бизнес-портрет",
                    Description = "Съемка для резюме, сайта, социальных сетей, личного бренда и профессионального профиля.",
                    DurationMinutes = 60,
                    Price = 4500m,
                    RetouchedPhotosCount = 15,
                    AllPhotosCount = 50,
                    DeliveryDays = 3,
                    IncludesStudio = false,
                    IsActive = true
                }
            ); modelBuilder.Entity<Studio>().HasData(
                new Studio
                {
                    Id = 1,
                    Name = "White Hall Studio",
                    District = "Вахитовский",
                    Address = "Казань, ул. Баумана, 15",
                    Description = "Светлая минималистичная студия с большими окнами и мягким естественным светом.",
                    PricePerHour = 1800m,
                    HallCount = 2,
                    HasNaturalLight = true,
                    InteriorStyle = "Минимализм",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                },
                new Studio
                {
                    Id = 2,
                    Name = "Studio1884",
                    District = "Вахитовский",
                    Address = "Казань, ул. Габдуллы Тукая, 84",
                    Description = "Интерьерная фотостудия в центре Казани с несколькими залами для портретных, семейных и коммерческих съемок.",
                    PricePerHour = 2200m,
                    HallCount = 4,
                    HasNaturalLight = true,
                    InteriorStyle = "Классика, минимализм, интерьерные залы",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                },
                new Studio
                {
                    Id = 3,
                    Name = "SEVER",
                    District = "Вахитовский",
                    Address = "Казань, ул. Вишневского, 2А лит К",
                    Description = "Фотостудия для портретных, семейных и контент-съемок с интерьерными зонами и удобным расположением в центральной части города.",
                    PricePerHour = 1600m,
                    HallCount = 2,
                    HasNaturalLight = true,
                    InteriorStyle = "Светлый интерьер",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                },
                new Studio
                {
                    Id = 4,
                    Name = "Photobaza",
                    District = "Вахитовский",
                    Address = "Казань, ул. Меховщиков, 80 к2",
                    Description = "Пространство для контент-съемок, портретов и коммерческих кадров с несколькими фотозонами.",
                    PricePerHour = 2500m,
                    HallCount = 4,
                    HasNaturalLight = false,
                    InteriorStyle = "Современный интерьер",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                },
                new Studio
                {
                    Id = 5,
                    Name = "Delight Studio",
                    District = "Московский",
                    Address = "Казань, ул. Васильченко, 1 к153А, 4 этаж",
                    Description = "Пространство для портретных, fashion и коммерческих съемок с современными залами и возможностью работы с естественным и студийным светом.",
                    PricePerHour = 1700m,
                    HallCount = 2,
                    HasNaturalLight = true,
                    InteriorStyle = "Циклорама, современный интерьер, коммерческая съемка",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                },
                new Studio
                {
                    Id = 6,
                    Name = "2Click",
                    District = "Кировский",
                    Address = "Казань, ул. Воскресенская, 20А",
                    Description = "Фотостудия для портретных, семейных и контент-съемок. Подходит для лаконичных кадров, съемок для личного бренда и небольших коммерческих проектов.",
                    PricePerHour = 1200m,
                    HallCount = 2,
                    HasNaturalLight = true,
                    InteriorStyle = "Современный интерьер",
                    ImagePath = "",
                    WebsiteUrl = "",
                    IsActive = true
                }
            );

            modelBuilder.Entity<PortfolioCategory>().HasData(
                new PortfolioCategory
                {
                    Id = 1,
                    Name = "Индивидуальная съемка",
                    Description = ""
                },
                new PortfolioCategory
                {
                    Id = 2,
                    Name = "Семейная съемка",
                    Description = ""
                },
                new PortfolioCategory
                {
                    Id = 3,
                    Name = "Love Story",
                    Description = ""
                },
                new PortfolioCategory
                {
                    Id = 4,
                    Name = "Fashion",
                    Description = ""
                },
                new PortfolioCategory
                {
                    Id = 5,
                    Name = "Контент-съемка",
                    Description = ""
                },
                new PortfolioCategory
                {
                    Id = 6,
                    Name = "Студийная съемка",
                    Description = ""
                }
            ); modelBuilder.Entity<PortfolioItem>().HasData(
                new PortfolioItem
                {
                    Id = 1,
                    Title = "Minimal Studio",
                    Description = "Студийная съемка в минималистичном интерьере с акцентом на форму и свет.",
                    ImagePath = "/images/portfolio/portfolio6.jpg",
                    ShootingDate = new DateTime(2026, 3, 19),
                    IsFeatured = true,
                    CategoryId = 6,
                    PhotographerId = 1,
                    StudioId = 1
                },
                new PortfolioItem
                {
                    Id = 2,
                    Title = "Городская история",
                    Description = "Эмоциональная love story съемка с теплой цветокоррекцией.",
                    ImagePath = "/images/portfolio/portfolio2.jpg",
                    ShootingDate = new DateTime(2026, 1, 29),
                    IsFeatured = true,
                    CategoryId = 3,
                    PhotographerId = 1,
                    StudioId = 2
                },
                new PortfolioItem
                {
                    Id = 3,
                    Title = "Светлый портрет",
                    Description = "Индивидуальная студийная съемка в мягком естественном свете.",
                    ImagePath = "/images/portfolio/portfolio3.jpg",
                    ShootingDate = new DateTime(2025, 12, 29),
                    IsFeatured = true,
                    CategoryId = 1,
                    PhotographerId = 1,
                    StudioId = 1
                },
                new PortfolioItem
                {
                    Id = 4,
                    Title = "Семейное утро",
                    Description = "Теплая семейная съемка в спокойной атмосфере.",
                    ImagePath = "/images/portfolio/portfolio4.jpg",
                    ShootingDate = new DateTime(2025, 11, 29),
                    IsFeatured = true,
                    CategoryId = 2,
                    PhotographerId = 1,
                    StudioId = 3
                },
                new PortfolioItem
                {
                    Id = 5,
                    Title = "Контент для бренда",
                    Description = "Съемка визуального контента для экспертного профиля и социальных сетей.",
                    ImagePath = "/images/portfolio/portfolio5.jpg",
                    ShootingDate = new DateTime(2026, 3, 9),
                    IsFeatured = false,
                    CategoryId = 5,
                    PhotographerId = 1,
                    StudioId = 2
                },
                new PortfolioItem
                {
                    Id = 6,
                    Title = "Fashion Frame",
                    Description = "Стилизованная fashion-съемка для персонального бренда.",
                    ImagePath = "/images/portfolio/portfolio1.jpg",
                    ShootingDate = new DateTime(2026, 2, 28),
                    IsFeatured = false,
                    CategoryId = 4,
                    PhotographerId = 1,
                    StudioId = 4
                }
            );

            modelBuilder.Entity<PortfolioImage>().HasData(
                new PortfolioImage
                {
                    Id = 1,
                    PortfolioItemId = 1,
                    ImagePath = "/images/portfolio/minimal-1.jpg",
                    Caption = "Портрет в естественном свете",
                    SortOrder = 1
                },
                new PortfolioImage
                {
                    Id = 2,
                    PortfolioItemId = 1,
                    ImagePath = "/images/portfolio/minimal-2.jpg",
                    Caption = "Минималистичный кадр",
                    SortOrder = 2
                }
            ); modelBuilder.Entity<Review>().HasData(
                new Review
                {
                    Id = 1,
                    ClientName = "Анна",
                    Text = "Съемка прошла спокойно и профессионально. Получились живые фотографии, которые действительно похожи на меня.",
                    Rating = 5,
                    CreatedAt = new DateTime(2026, 3, 12, 12, 0, 0),
                    IsPublished = true
                },
                new Review
                {
                    Id = 2,
                    ClientName = "Мария",
                    Text = "Понравилось, что фотограф помогла с позированием и выбором студии. Результат получился очень аккуратным.",
                    Rating = 5,
                    CreatedAt = new DateTime(2026, 3, 18, 15, 30, 0),
                    IsPublished = true
                },
                new Review
                {
                    Id = 3,
                    ClientName = "Екатерина",
                    Text = "Заказывала съемку для личного бренда. Фотографии подошли и для сайта, и для социальных сетей.",
                    Rating = 5,
                    CreatedAt = new DateTime(2026, 4, 2, 10, 20, 0),
                    IsPublished = true
                }
            );
        }
    }
}
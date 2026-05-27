using LuminKazan.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LuminKazan.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            SeedAsync(context).GetAwaiter().GetResult();
        }

        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await SeedAsync(context);
        }

        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            SeedAsync(context).GetAwaiter().GetResult();
        }

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedAsync(context);
        }

        public static void Initialize(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            SeedAsync(context).GetAwaiter().GetResult();
        }

        public static async Task InitializeAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await SeedAsync(context);
        }

        private static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.Photographers.AnyAsync())
            {
                context.Photographers.Add(
                    new Photographer
                    {
                        FullName = "Алина Валеева",
                        Specialization = "Современный художественный и коммерческий фотограф",
                        Biography = "Профессиональный фотограф из Казани, работающий в жанрах индивидуальной, семейной, fashion и контент-съемки. Основной акцент - на эстетике, свете и живой эмоциональной подаче."
                    }
                );

                await context.SaveChangesAsync();
            }
            if (!await context.Services.AnyAsync())
            {
                context.Services.AddRange(
                    new Service
                    {
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
                );

                await context.SaveChangesAsync();
            }
            if (!await context.Studios.AnyAsync())
            {
                context.Studios.AddRange(
                    new Studio
                    {
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

                await context.SaveChangesAsync();
            }

            if (!await context.PortfolioCategories.AnyAsync())
            {
                context.PortfolioCategories.AddRange(
                    new PortfolioCategory
                    {
                        Name = "Индивидуальная съемка"
                    },
                    new PortfolioCategory
                    {
                        Name = "Семейная съемка"
                    },
                    new PortfolioCategory
                    {
                        Name = "Love Story"
                    },
                    new PortfolioCategory
                    {
                        Name = "Fashion"
                    },
                    new PortfolioCategory
                    {
                        Name = "Контент-съемка"
                    },
                    new PortfolioCategory
                    {
                        Name = "Студийная съемка"
                    }
                );

                await context.SaveChangesAsync();
            }

            if (!await context.PortfolioItems.AnyAsync())
            {
                var photographer = await context.Photographers.FirstOrDefaultAsync();

                var individualCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Индивидуальная съемка");

                var familyCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Семейная съемка");

                var loveStoryCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Love Story");

                var fashionCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Fashion");

                var contentCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Контент-съемка");

                var studioCategory = await context.PortfolioCategories
                    .FirstOrDefaultAsync(category => category.Name == "Студийная съемка");

                var whiteHallStudio = await context.Studios
                    .FirstOrDefaultAsync(studio => studio.Name == "White Hall Studio");

                var studio1884 = await context.Studios
                    .FirstOrDefaultAsync(studio => studio.Name == "Studio1884");

                var severStudio = await context.Studios
                    .FirstOrDefaultAsync(studio => studio.Name == "SEVER");

                var photobazaStudio = await context.Studios
                    .FirstOrDefaultAsync(studio => studio.Name == "Photobaza"); if (photographer != null &&
                    individualCategory != null &&
                    familyCategory != null &&
                    loveStoryCategory != null &&
                    fashionCategory != null &&
                    contentCategory != null &&
                    studioCategory != null)
                {
                    context.PortfolioItems.AddRange(
                        new PortfolioItem
                        {
                            Title = "Minimal Studio",
                            Description = "Студийная съемка в минималистичном интерьере с акцентом на форму и свет.",
                            ImagePath = "/images/portfolio/portfolio6.jpg",
                            ShootingDate = new DateTime(2026, 3, 19),
                            IsFeatured = true,
                            CategoryId = studioCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = whiteHallStudio?.Id
                        },
                        new PortfolioItem
                        {
                            Title = "Городская история",
                            Description = "Эмоциональная love story съемка с теплой цветокоррекцией.",
                            ImagePath = "/images/portfolio/portfolio2.jpg",
                            ShootingDate = new DateTime(2026, 1, 29),
                            IsFeatured = true,
                            CategoryId = loveStoryCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = studio1884?.Id
                        },
                        new PortfolioItem
                        {
                            Title = "Светлый портрет",
                            Description = "Индивидуальная студийная съемка в мягком естественном свете.",
                            ImagePath = "/images/portfolio/portfolio3.jpg",
                            ShootingDate = new DateTime(2025, 12, 29),
                            IsFeatured = true,
                            CategoryId = individualCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = whiteHallStudio?.Id
                        },
                        new PortfolioItem
                        {
                            Title = "Семейное утро",
                            Description = "Теплая семейная съемка в спокойной атмосфере.",
                            ImagePath = "/images/portfolio/portfolio4.jpg",
                            ShootingDate = new DateTime(2025, 11, 29),
                            IsFeatured = true,
                            CategoryId = familyCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = severStudio?.Id
                        },
                        new PortfolioItem
                        {
                            Title = "Контент для бренда",
                            Description = "Съемка визуального контента для экспертного профиля и социальных сетей.",
                            ImagePath = "/images/portfolio/portfolio5.jpg",
                            ShootingDate = new DateTime(2026, 3, 9),
                            IsFeatured = false,
                            CategoryId = contentCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = studio1884?.Id
                        },
                        new PortfolioItem
                        {
                            Title = "Fashion Frame",
                            Description = "Стилизованная fashion-съемка для персонального бренда.",
                            ImagePath = "/images/portfolio/portfolio1.jpg",
                            ShootingDate = new DateTime(2026, 2, 28),
                            IsFeatured = false,
                            CategoryId = fashionCategory.Id,
                            PhotographerId = photographer.Id,
                            StudioId = photobazaStudio?.Id
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }

            if (!await context.PortfolioImages.AnyAsync())
            {
                var minimalStudio = await context.PortfolioItems
                    .FirstOrDefaultAsync(item => item.Title == "Minimal Studio");

                if (minimalStudio != null)
                {
                    context.PortfolioImages.AddRange(
                        new PortfolioImage
                        {
                            PortfolioItemId = minimalStudio.Id,
                            ImagePath = "/images/portfolio/minimal-1.jpg",
                            Caption = "Портрет в естественном свете",
                            SortOrder = 1
                        },
                        new PortfolioImage
                        {
                            PortfolioItemId = minimalStudio.Id,
                            ImagePath = "/images/portfolio/minimal-2.jpg",
                            Caption = "Минималистичный кадр",
                            SortOrder = 2
                        }
                    );

                    await context.SaveChangesAsync();
                }
            }

            if (!await context.Reviews.AnyAsync())
            {
                context.Reviews.AddRange(
                    new Review
                    {
                        ClientName = "Анна",
                        Text = "Съемка прошла спокойно и профессионально. Получились живые фотографии, которые действительно похожи на меня.",
                        Rating = 5,
                        CreatedAt = new DateTime(2026, 3, 12, 12, 0, 0),
                        IsPublished = true
                    },
                    new Review
                    {
                        ClientName = "Мария",
                        Text = "Понравилось, что фотограф помогла с позированием и выбором студии. Результат получился очень аккуратным.",
                        Rating = 5,
                        CreatedAt = new DateTime(2026, 3, 18, 15, 30, 0),
                        IsPublished = true
                    },
                    new Review
                    {
                        ClientName = "Екатерина",
                        Text = "Заказывала съемку для личного бренда. Фотографии подошли и для сайта, и для социальных сетей.",
                        Rating = 5,
                        CreatedAt = new DateTime(2026, 4, 2, 10, 20, 0),
                        IsPublished = true
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
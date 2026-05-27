using LuminKazan.Data;
using LuminKazan.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                Photographer = await _context.Photographers
                    .FirstOrDefaultAsync(),

                Services = await _context.Services
                    .Where(service => service.IsActive)
                    .OrderBy(service => service.Price)
                    .Take(6)
                    .ToListAsync(),

                Studios = await _context.Studios
                    .Where(studio => studio.IsActive)
                    .OrderBy(studio => studio.Name)
                    .Take(6)
                    .ToListAsync(),

                Reviews = await _context.Reviews
                    .Where(review => review.IsPublished)
                    .OrderByDescending(review => review.CreatedAt)
                    .Take(6)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
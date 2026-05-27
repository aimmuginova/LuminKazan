using LuminKazan.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [AllowAnonymous]
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PortfolioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var portfolioItems = await _context.PortfolioItems
                .Include(p => p.Category)
                .Include(p => p.Studio)
                .Include(p => p.Photographer)
                .Include(p => p.Images)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.ShootingDate)
                .ToListAsync();

            return View(portfolioItems);
        }

        public async Task<IActionResult> Details(int id)
        {
            var portfolioItem = await _context.PortfolioItems
                .Include(p => p.Category)
                .Include(p => p.Studio)
                .Include(p => p.Photographer)
                .Include(p => p.Images.OrderBy(i => i.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (portfolioItem == null)
            {
                return NotFound();
            }

            return View(portfolioItem);
        }
    }
}
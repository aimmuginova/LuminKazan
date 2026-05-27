using LuminKazan.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [AllowAnonymous]
    public class StudiosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudiosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var studios = await _context.Studios
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(studios);
        }

        public async Task<IActionResult> Details(int id)
        {
            var studio = await _context.Studios
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (studio == null)
            {
                return NotFound();
            }

            return View(studio);
        }
    }
}
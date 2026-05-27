using LuminKazan.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LuminKazan.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Reviews.AsQueryable();

            if (status == "published")
            {
                query = query.Where(review => review.IsPublished);
            }

            if (status == "hidden")
            {
                query = query.Where(review => !review.IsPublished);
            }

            var reviews = await query
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();

            ViewBag.CurrentStatus = status ?? "all";

            return View(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            review.IsPublished = true;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Hide(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            review.IsPublished = false;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            _context.Reviews.Remove(review);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
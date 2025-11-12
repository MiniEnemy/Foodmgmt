using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;

namespace FoodMgmt.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;
        public CategoriesController(AppDbContext context) => _context = context;

        // GET: /Categories
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Categories.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            var list = await query.AsNoTracking().ToListAsync();
            return View(list);
        }

        // GET: /Categories/Create
        public IActionResult Create() => View();

        // POST: /Categories/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid) return View(category);

            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error saving category: " + ex.Message);
                return View(category);
            }
        }

        // GET: /Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Categories/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) return BadRequest();
            if (!ModelState.IsValid) return View(category);

            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == category.Id))
                    return NotFound();
                throw;
            }
        }

        // GET: /Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            var category = await _context.Categories
                .Include(c => c.FoodItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null) return NotFound();
            return View(category);
        }

        // POST: /Categories/DeleteConfirmed/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.FoodItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null) return NotFound();

            if (category.FoodItems != null && category.FoodItems.Any())
            {
                ModelState.AddModelError("", "Cannot delete category with existing food items. Remove them first.");
                return View(category);
            }

            try
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error deleting category: " + ex.Message);
                return View(category);
            }
        }
    }
}

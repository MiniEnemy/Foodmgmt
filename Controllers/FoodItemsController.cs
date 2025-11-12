using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;

namespace FoodMgmt.Controllers
{
    public class FoodItemsController : Controller
    {
        private readonly AppDbContext _context;
        public FoodItemsController(AppDbContext context) => _context = context;

        // List all items
        public async Task<IActionResult> Index(string search)
        {
            var q = _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(f => f.Name.Contains(search) ||
                                 f.Category.Name.Contains(search) ||
                                 (f.Supplier != null && f.Supplier.Name.Contains(search)));

            return View(await q.AsNoTracking().ToListAsync());
        }

        // GET: Create form
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View();
        }

        // POST: Create new food item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItem item)
        {
            // Debug: show which fields failed
            if (!ModelState.IsValid)
            {
                ViewBag.Debug = string.Join("; ",
                    ModelState
                        .Where(m => m.Value.Errors.Count > 0)
                        .Select(m => $"{m.Key}: {string.Join(", ", m.Value.Errors.Select(e => e.ErrorMessage))}")
                );

                PopulateDropdowns();
                return View(item);
            }

            try
            {
                _context.Add(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Food item added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Save failed: " + ex.Message);
                PopulateDropdowns();
                return View(item);
            }
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var item = await _context.FoodItems.FindAsync(id);
            if (item == null) return NotFound();
            PopulateDropdowns();
            return View(item);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItem item)
        {
            if (id != item.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                PopulateDropdowns();
                return View(item);
            }

            try
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Food item updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FoodItems.AnyAsync(f => f.Id == item.Id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Update failed: " + ex.Message);
                PopulateDropdowns();
                return View(item);
            }
        }

        // GET: Delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var item = await _context.FoodItems
                .Include(f => f.Category)
                .Include(f => f.Supplier)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.FoodItems
                .Include(f => f.OrderDetails)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (item == null) return NotFound();

            if (item.OrderDetails != null && item.OrderDetails.Any())
            {
                ModelState.AddModelError("", "Cannot delete item with existing orders.");
                return View(item);
            }

            try
            {
                _context.FoodItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Item deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Delete failed: " + ex.Message);
                return View(item);
            }
        }

        // Helper for dropdowns
        private void PopulateDropdowns()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
            ViewBag.SupplierId = new SelectList(_context.Suppliers.OrderBy(s => s.Name), "Id", "Name");
        }
    }
}

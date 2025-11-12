using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;

namespace FoodMgmt.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly AppDbContext _context;
        public SuppliersController(AppDbContext context) => _context = context;

        // List all suppliers + optional search
        public async Task<IActionResult> Index(string search)
        {
            var q = _context.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.Name.Contains(search) || s.Email.Contains(search) || s.Phone.Contains(search));
            return View(await q.AsNoTracking().ToListAsync());
        }

        // Display create form
        public IActionResult Create() => View();

        // Handle create form submission
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (!ModelState.IsValid)
                return View(supplier);

            try
            {
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Supplier added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Unable to save supplier: " + ex.Message);
                return View(supplier);
            }
        }

        // Display edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // Handle edit form submission
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id) return BadRequest();
            if (!ModelState.IsValid) return View(supplier);

            try
            {
                _context.Update(supplier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Supplier updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Suppliers.AnyAsync(s => s.Id == supplier.Id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Update failed: " + ex.Message);
                return View(supplier);
            }
        }

        // Show delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();
            var supplier = await _context.Suppliers
                .Include(s => s.FoodItems)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();
            return View(supplier);
        }

        // Handle delete confirmation
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers
                .Include(s => s.FoodItems)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null) return NotFound();

            if (supplier.FoodItems != null && supplier.FoodItems.Any())
            {
                ModelState.AddModelError("", "Cannot delete supplier with existing food items. Remove them first.");
                return View(supplier);
            }

            try
            {
                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Supplier deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Delete failed: " + ex.Message);
                return View(supplier);
            }
        }
    }
}

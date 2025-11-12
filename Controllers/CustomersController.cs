using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;

namespace FoodMgmt.Controllers
{
    public class CustomersController : Controller
    {
        private readonly AppDbContext _context;
        public CustomersController(AppDbContext context) => _context = context;

        // GET: Customers
        public async Task<IActionResult> Index(string search)
        {
            var q = _context.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(c => c.FullName.Contains(search) || c.Email.Contains(search));
            return View(await q.AsNoTracking().ToListAsync());
        }

        // GET: Create
        public IActionResult Create() => View();

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error while saving: " + ex.Message);
                return View(customer);
            }
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return BadRequest();
            if (!ModelState.IsValid) return View(customer);

            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Customers.AnyAsync(c => c.Id == customer.Id))
                    return NotFound();
                throw;
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Error while updating: " + ex.Message);
                return View(customer);
            }
        }

        // POST: Delete (no confirmation page)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction(nameof(Index));
            }

            if (customer.Orders != null && customer.Orders.Any())
            {
                TempData["Error"] = "Cannot delete customer with existing orders.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer deleted successfully.";
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "Delete failed: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

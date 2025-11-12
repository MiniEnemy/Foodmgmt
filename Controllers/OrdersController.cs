using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;
using FoodMgmt.Models.ViewModels;

namespace FoodMgmt.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context) => _context = context;

        // LIST (Pending Orders)
        public async Task<IActionResult> Index(string search)
        {
            var q = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .Where(o => !o.IsCompleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(o => o.Customer.FullName.Contains(search) || o.Id.ToString().Contains(search));

            return View(await q.OrderByDescending(o => o.OrderDate).ToListAsync());
        }

        // COMPLETED ORDERS
        public async Task<IActionResult> Completed()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .Where(o => o.IsCompleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        // MARK AS COMPLETE
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.IsCompleted = true;
            _context.Update(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order marked as completed.";
            return RedirectToAction(nameof(Index));
        }

        // CREATE
        public IActionResult Create()
        {
            PopulateCustomersAndItems();
            return View(new OrderCreateVm { Items = new List<OrderItemVm> { new OrderItemVm() } });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderCreateVm vm)
        {
            PopulateCustomersAndItems();
            if (!ModelState.IsValid) return View(vm);

            if (vm.Items == null || !vm.Items.Any())
            {
                ModelState.AddModelError("", "Add at least one item.");
                return View(vm);
            }

            using var txn = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = new Order { CustomerId = vm.CustomerId, OrderDate = DateTime.UtcNow };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var item in vm.Items)
                {
                    var fi = await _context.FoodItems.FindAsync(item.FoodItemId);
                    if (fi == null || item.Quantity <= 0)
                    {
                        ModelState.AddModelError("", "Invalid food item or quantity.");
                        return View(vm);
                    }

                    if (fi.Stock < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {fi.Name}. Stock: {fi.Stock}");
                        return View(vm);
                    }

                    fi.Stock -= item.Quantity;
                    _context.Update(fi);

                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        FoodItemId = fi.Id,
                        Quantity = item.Quantity,
                        UnitPrice = fi.Price
                    };
                    _context.OrderDetails.Add(detail);
                    total += fi.Price * item.Quantity;
                }

                order.GrandTotal = total;
                _context.Update(order);
                await _context.SaveChangesAsync();
                await txn.CommitAsync();

                TempData["Success"] = "Order created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await txn.RollbackAsync();
                ModelState.AddModelError("", "Save failed: " + ex.Message);
                return View(vm);
            }
        }

        // EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();

            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            var vm = new OrderCreateVm
            {
                CustomerId = order.CustomerId,
                Items = order.OrderDetails.Select(od => new OrderItemVm
                {
                    FoodItemId = od.FoodItemId,
                    Quantity = od.Quantity
                }).ToList()
            };

            ViewBag.OrderId = order.Id;
            PopulateCustomersAndItems();
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrderCreateVm vm)
        {
            PopulateCustomersAndItems();
            if (!ModelState.IsValid) return View(vm);

            var order = await _context.Orders.Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            using var txn = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var od in order.OrderDetails)
                {
                    var fi = await _context.FoodItems.FindAsync(od.FoodItemId);
                    if (fi != null)
                    {
                        fi.Stock += od.Quantity;
                        _context.Update(fi);
                    }
                }

                _context.OrderDetails.RemoveRange(order.OrderDetails);
                await _context.SaveChangesAsync();

                decimal total = 0;
                foreach (var item in vm.Items)
                {
                    var fi = await _context.FoodItems.FindAsync(item.FoodItemId);
                    if (fi == null || item.Quantity <= 0)
                    {
                        ModelState.AddModelError("", "Invalid food item or quantity.");
                        return View(vm);
                    }

                    if (fi.Stock < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {fi.Name}. Stock: {fi.Stock}");
                        return View(vm);
                    }

                    fi.Stock -= item.Quantity;
                    _context.Update(fi);

                    var detail = new OrderDetail
                    {
                        OrderId = order.Id,
                        FoodItemId = fi.Id,
                        Quantity = item.Quantity,
                        UnitPrice = fi.Price
                    };
                    _context.OrderDetails.Add(detail);
                    total += fi.Price * item.Quantity;
                }

                order.CustomerId = vm.CustomerId;
                order.GrandTotal = total;
                order.OrderDate = DateTime.UtcNow;

                _context.Update(order);
                await _context.SaveChangesAsync();
                await txn.CommitAsync();

                TempData["Success"] = "Order updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await txn.RollbackAsync();
                ModelState.AddModelError("", "Update failed: " + ex.Message);
                return View(vm);
            }
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return BadRequest();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return BadRequest();

            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.FoodItem)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            using var txn = await _context.Database.BeginTransactionAsync();
            try
            {
                // Restore stock
                foreach (var od in order.OrderDetails)
                {
                    var fi = await _context.FoodItems.FindAsync(od.FoodItemId);
                    if (fi != null)
                    {
                        fi.Stock += od.Quantity;
                        _context.Update(fi);
                    }
                }

                // Remove details + order
                _context.OrderDetails.RemoveRange(order.OrderDetails);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await txn.CommitAsync();
                TempData["Success"] = "Order deleted and stock restored.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await txn.RollbackAsync();
                ModelState.AddModelError("", "Delete failed: " + ex.Message);
                return View(order);
            }
        }

        private void PopulateCustomersAndItems()
        {
            ViewBag.Customers = new SelectList(_context.Customers.OrderBy(c => c.FullName), "Id", "FullName");
            ViewBag.FoodItems = new SelectList(_context.FoodItems.OrderBy(f => f.Name), "Id", "Name");
        }
    }
}

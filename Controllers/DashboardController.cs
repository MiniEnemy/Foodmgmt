using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;
using FoodMgmt.Models;
using FoodMgmt.Models.ViewModels;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FoodMgmt.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard main page
        public async Task<IActionResult> Index()
        {
            var totalCustomers = await _context.Customers.CountAsync();
            var totalSuppliers = await _context.Suppliers.CountAsync();
            var totalFoodItems = await _context.FoodItems.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var completedOrders = await _context.Orders.Where(o => o.IsCompleted).CountAsync();
            var pendingOrders = totalOrders - completedOrders;
            var lowStockItems = await _context.FoodItems
                                    .Where(f => f.Stock < 5)
                                    .ToListAsync();

            var model = new DashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalSuppliers = totalSuppliers,
                TotalFoodItems = totalFoodItems,
                TotalOrders = totalOrders,
                CompletedOrders = completedOrders,
                PendingOrders = pendingOrders,
                LowStockItems = lowStockItems
            };

            return View(model);
        }

        // Excel export of Food Items
        public IActionResult ExportFoodItems()
        {
            ExcelPackage.License.SetNonCommercialPersonal("Jatin");

            var items = _context.FoodItems
                        .Include(f => f.Category)
                        .Include(f => f.Supplier)
                        .ToList();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("FoodItems");
                ws.Cells[1, 1].Value = "Name";
                ws.Cells[1, 2].Value = "Category";
                ws.Cells[1, 3].Value = "Supplier";
                ws.Cells[1, 4].Value = "Price";
                ws.Cells[1, 5].Value = "stock";

                int row = 2;
                foreach (var item in items)
                {
                    ws.Cells[row, 1].Value = item.Name;
                    ws.Cells[row, 2].Value = item.Category?.Name ?? "";
                    ws.Cells[row, 3].Value = item.Supplier?.Name ?? "";
                    ws.Cells[row, 4].Value = item.Price;
                    ws.Cells[row, 5].Value = item.Stock;
                    row++;
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"FoodItems-{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

    }
}

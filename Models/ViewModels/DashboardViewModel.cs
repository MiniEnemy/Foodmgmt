using FoodMgmt.Models;
using System.Collections.Generic;

namespace FoodMgmt.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalFoodItems { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public List<FoodItem> LowStockItems { get; set; }
    }
}

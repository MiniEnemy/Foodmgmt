using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models.ViewModels
{
    public class OrderItemVm
    {
        [Required]
        public int FoodItemId { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; }
    }

    public class OrderCreateVm
    {
        [Required]
        public int CustomerId { get; set; }

        public List<OrderItemVm> Items { get; set; } = new();
    }
}

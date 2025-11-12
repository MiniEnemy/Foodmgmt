using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required]
        public int FoodItemId { get; set; }
        public FoodItem FoodItem { get; set; }

        [Range(1, 10000)]
        public int Quantity { get; set; }

        [Range(0.0, 100000)]
        public decimal UnitPrice { get; set; } // snapshot of price at order time

        public decimal Total => UnitPrice * Quantity;
    }
}

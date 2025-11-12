using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal GrandTotal { get; set; }
        public bool IsCompleted { get; set; } = false;


        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

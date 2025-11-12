using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class FoodItem
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        // Foreign keys
        [Required]
        public int CategoryId { get; set; }
        [ValidateNever]
            public Category Category { get; set; }

        public int? SupplierId { get; set; }
        [ValidateNever]
        public Supplier Supplier { get; set; }
        [ValidateNever]
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}

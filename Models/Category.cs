using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(300)]
        public string Description { get; set; }
        [ValidateNever] 
        public ICollection<FoodItem> FoodItems { get; set; }
    }
}

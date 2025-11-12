using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Phone, StringLength(20)]
        public string Phone { get; set; }

        [EmailAddress, StringLength(150)]
        public string Email { get; set; }
        [ValidateNever]
        public ICollection<FoodItem> FoodItems { get; set; }
    }
}

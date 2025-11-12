using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodMgmt.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; }

        [EmailAddress, StringLength(150)]
        public string Email { get; set; }

        [Phone, StringLength(20)]
        public string Phone { get; set; }
        [ValidateNever]
        public ICollection<Order> Orders { get; set; }
    }
}

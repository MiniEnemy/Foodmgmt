using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodMgmt.Data;

namespace FoodMgmt.Controllers.Api
{
    [Route("api/fooditems")]
    [ApiController]
    public class FoodItemsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FoodItemsApiController(AppDbContext context) => _context = context;

        [HttpGet("price/{id}")]
        public async Task<IActionResult> Price(int id)
        {
            var fi = await _context.FoodItems.FindAsync(id);
            if (fi == null) return NotFound();
            return Ok(new { price = fi.Price });
        }
    }
}

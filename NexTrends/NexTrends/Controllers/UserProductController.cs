using Microsoft.AspNetCore.Mvc;
using NexTrends.Models;

namespace NexTrends.Controllers
{
    public class UserProductController : Controller
    {
        private readonly NexTrendsContext _context;

        public UserProductController(NexTrendsContext context)
        {
            _context = context;
        }
        public IActionResult ProductsList(int id)
        {
            var products = _context.Products
                                    .Where(p => p.CategoryId == id)
                                    .ToList();

            if (products.Any())
            {
                return View(products);
            }
            else
            {
                ViewBag.Result = "There are no products in this category.";
                return View(new List<Product>()); 
            }
        }

        public IActionResult GetImage(int id)
        {
            var product = _context.Products
                .Where(p => p.Id == id)
                .Select(p => p.Image)
                .FirstOrDefault();

            if (product != null)
            {
                return File(product, "image/jpeg");
            }

            return NotFound();
        }
        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                                  .FirstOrDefault(p => p.Id == id);

            if (product != null)
            {
                return View(product);
            }

            return NotFound();
        }

    }
}

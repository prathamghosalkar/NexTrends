using Microsoft.AspNetCore.Mvc;
using NexTrends.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                ViewData["ProductId"] = id; // Pass the product ID to the view for review purposes
                return View(product);
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult AddReview(Review review)
        {

            // Check if user is logged in
            if (!HttpContext.Session.TryGetValue("UserId", out _))
            {
                return Json(new { success = false, message = "You must be logged in to submit a review." });
            }

            try
            {
                // Set additional review properties
                review.CustomerId = HttpContext.Session.GetInt32("UserId").Value;
                review.CreatedAt = DateTime.Now;


                // Add review to database
                _context.Reviews.Add(review);
                _context.SaveChanges();

                return Json(new { success = true, message = "Review submitted successfully!" });
            }
            catch (Exception ex)
            {
                // Log the exception (not shown here) and return an error response
                return Json(new { success = false, message = "An error occurred while submitting the review." });
            }
        }

        public IActionResult ViewReviews(int productId)
        {
            var reviews = _context.Reviews
                                   .Where(r => r.ProductId == productId)
                                   .Select(r => new
                                   {
                                       r.Rating,
                                       r.Comment,
                                       CustomerName = _context.Customers.FirstOrDefault(c => c.Id == r.CustomerId).Name,
                                       r.CreatedAt
                                   })
                                   .ToList();

            return PartialView("_ReviewsList", reviews);
        }
    }
}

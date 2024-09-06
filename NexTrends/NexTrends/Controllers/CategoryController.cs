using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;

namespace NexTrends.Controllers
{
   
    public class CategoryController : Controller
    {

        NexTrendsContext context = new NexTrendsContext();

        public IActionResult Category()
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                Category cat = new Category();
                var context = new NexTrendsContext();
                IEnumerable<Category> categories = context.Categories.ToList();
                return View(categories);
            }
            return RedirectToAction("Login", "Account");
            
        }

        [HttpPost]
        public IActionResult AddCategory(string CName)
        {

            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                string name = CName;
                Category category = new Category();
                var context = new NexTrendsContext();
                category.Name = name;
                context.Add(category);
                var re = context.SaveChanges();
                if (re > 0)
                {
                    TempData["SuccessMessage"] = "Category added successfully!";
                    return RedirectToAction("Category", "Category");
                }
                return RedirectToAction("Category");
            }
            return RedirectToAction("Login", "Account");
            
        }

        
    }
}

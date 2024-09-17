using Microsoft.AspNetCore.Mvc;
using NexTrends.Models;

namespace NexTrends.Controllers
{
    public class CSearchController : Controller
    {
        NexTrendsContext context =new NexTrendsContext();   
        public IActionResult SearchResult()
        {
            return View();
        }
        [HttpPost]
        public IActionResult SearchResult(string SearchPr)
        {
            // Convert search parameter to lowercase for case-insensitive comparison
            var searchLower = SearchPr?.ToLower();

            var result = from p in context.Products
                         join c in context.Categories on p.CategoryId equals c.Id
                         where p.Name.ToLower().Contains(searchLower) ||
                               c.Name.ToLower().Contains(searchLower)
                         select new
                         {
                             Pid = p.Id,
                             Pname = p.Name,
                             Pprice = p.Price,
                             PImage = p.Image
                         };

            // Check if any results were found
            if (result.Any())
            {
                var resultPR = result.ToList();
                return View(resultPR);
            }

            TempData["MSG"] = "No matching products found!";
            return View();
        }

    }
}

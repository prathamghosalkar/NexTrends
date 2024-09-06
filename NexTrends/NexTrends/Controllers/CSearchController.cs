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
            var result = from p in context.Products
                         join c in context.Categories on p.CategoryId equals c.Id
                         where p.Name.Contains(SearchPr) ||
                               p.Name.StartsWith(SearchPr) ||
                               p.Name.EndsWith(SearchPr) ||
                               c.Name.Contains(SearchPr) ||
                               c.Name.StartsWith(SearchPr) ||
                               c.Name.EndsWith(SearchPr)
                         select new 
                         {
                             Pid=p.Id,
                             Pname=p.Name,
                             Pprice=p.Price,
                             PImage=p.Image
                         };
            if (result !=null)
            {
                var resultPR = result.ToList();
                return View(resultPR);
            }
            TempData["MSG"] = "No match products found !";
            return View();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;

namespace NexTrends.Controllers
{
    public class HomeController : Controller
    {
        NexTrendsContext _context = new NexTrendsContext();
        public IActionResult Index()
        {
            IEnumerable<Category> categories = _context.Categories.ToList();
            return View(categories);
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace NexTrends.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using Microsoft.AspNetCore.Mvc;

namespace NexTrends.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Men()
        {
            return View();
        }

        public IActionResult Women()
        {
            return View();
        }

        public IActionResult Kids()
        {
            return View();
        }
    }
}

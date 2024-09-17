using Microsoft.AspNetCore.Mvc;

namespace NexTrends.Controllers
{
    public class flipcardController : Controller
    {
        public IActionResult FlipCard()
        {
            return View();
        }
    }
}

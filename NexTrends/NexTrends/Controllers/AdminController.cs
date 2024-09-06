using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    public IActionResult Index()
    {

        string userRole = HttpContext.Session.GetString("UserRole");


        if (userRole == "admin")
        {
            return View("AdminDashboard");
        }

        return RedirectToAction("Login", "Account");
    }
}

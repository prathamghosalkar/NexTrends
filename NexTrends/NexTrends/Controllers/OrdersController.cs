using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System.Linq;


public class OrdersController : Controller
{
    private readonly NexTrendsContext _context;

    public OrdersController(NexTrendsContext context)
    {
        _context = context;
    }

    // GET: Orders
    public IActionResult DisplayOrder()
    {
        string userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "admin")
        {
            var orders = _context.Orders
             .Include(o => o.Cart)
             .ThenInclude(c => c.Customer)
             .OrderByDescending(o => o.Status == "Pending").ToList();
            return View(orders);
        }
        return RedirectToAction("Login", "Account");
        
    }


    [HttpPost]
    public IActionResult Deliver(int id)
    {
        string userRole = HttpContext.Session.GetString("UserRole");
        if (userRole == "admin")
        {
            var order = _context.Orders.Find(id);
            if (order != null && order.Status == "Pending")
            {
                order.Status = "Completed";
                _context.SaveChanges();
                TempData["Message"] = "Order has been completed successfully.";
            }
            return RedirectToAction(nameof(DisplayOrder));
        }
        return RedirectToAction("Login", "Account");
       
    }
}

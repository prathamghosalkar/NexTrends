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

            var returnRequests = _context.ReturnRequests
           .Include(rr => rr.Order)
           .ThenInclude(o => o.Cart)
           .ThenInclude(c => c.Product)
           .OrderByDescending(o => o.Status == "Pending")
           .ToList();

            ViewData["ReturnRequest"] = returnRequests;
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
                order.DeliveryDate = DateTime.Now;
                _context.SaveChanges();
                TempData["Message"] = "Order has been completed successfully.";
            }
            return RedirectToAction(nameof(DisplayOrder));
        }
        return RedirectToAction("Login", "Account");

    }

    [HttpPost]
    public IActionResult ApproveReturnRequest(int Id)
    {
        try
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                var returnRequest = _context.ReturnRequests.Find(Id);
                if (returnRequest != null && returnRequest.Status == "Pending")
                {
                    returnRequest.Status = "Approved";
                    var order = _context.Orders.Find(returnRequest.OrderId);
                    if (order != null)
                    {
                        var cart = _context.Carts.Find(order.CartId);
                        if (returnRequest.Quantity == cart.Quantity)
                        {
                            order.Status = "Returned";
                        }
                        else
                        {
                            cart.Quantity = cart.Quantity - returnRequest.Quantity;
                        }
                        var Product = _context.Products.Find(cart.ProductId);
                        if (Product != null)
                        {
                            Product.Quantity = Product.Quantity + returnRequest.Quantity;
                            _context.SaveChanges();
                            TempData["Message"] = "Request has been approved !";
                            return RedirectToAction("DisplayOrder");
                        }
                    }
                }
                else
                {
                    TempData["Message"] = "Something error  !";
                    return RedirectToAction("DisplayOrder");
                }
            }
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Something error  !";
            return RedirectToAction("DisplayOrder");
        }

        return RedirectToAction("Login", "Account");
    }
}

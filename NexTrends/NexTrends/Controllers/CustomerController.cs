using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NexTrends.Models;
using System.Linq;

namespace NexTrends.Controllers
{
    public class CustomerController : Controller
    {
        private readonly NexTrendsContext _context;

        public CustomerController(NexTrendsContext context)
        {
            _context = context;
        }

        public IActionResult CProfile()
        {
            string? email = HttpContext.Session.GetString("Email");

            if (email == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers
                .Where(c => c.Email == email)
                .Select(c => new
                {
                    Customer = c,
                    Orders = _context.Orders
                        .Where(o => o.Cart.Customer.Email == email && o.Status == "Pending")
                        .ToList(),
                    
                    UsedCoupons = _context.CouponUsages
                        .Where(cu => cu.Customer.Email == email)
                        .Select(cu => cu.Coupon)
                        .Distinct()
                        .ToList(),
                    PastOrders = _context.Orders
                        .Where(o => o.Cart.Customer.Email == email && o.Status == "Completed") 
                        .ToList()
                })
                .FirstOrDefault();

            if (customer == null)
            {
                return NotFound();
            }

            var allCoupons = _context.Coupons.ToList();

            var unusedCoupons = allCoupons
                .Where(coupon => !customer.UsedCoupons.Any(usedCoupon => usedCoupon.Id == coupon.Id))
                .ToList();

            var viewModel = new CustomerProfileViewModel
            {
                Customer = customer.Customer,
                Orders = customer.Orders,
                Coupons = unusedCoupons, 
                PastOrders = customer.PastOrders
            };

            return View(viewModel);
        }
    }
}

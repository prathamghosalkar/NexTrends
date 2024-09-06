using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexTrends.Controllers
{
    public class ConfirmOrderController : Controller
    {
        private readonly NexTrendsContext _context;

        public ConfirmOrderController(NexTrendsContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var totalPrice = HttpContext.Session.GetString("TotalPrice");
            var discount = HttpContext.Session.GetString("Discount");
            var finalPrice = HttpContext.Session.GetString("FinalPrice");
            var numberOfProducts = HttpContext.Session.GetString("NumberOfProducts");
            var userName = HttpContext.Session.GetString("UserName");
            var email = HttpContext.Session.GetString("Email");
            var address = HttpContext.Session.GetString("Address");

            var viewModel = new ConfirmOrderViewModel
            {
                TotalPrice = decimal.Parse(totalPrice),
                Discount = decimal.Parse(discount),
                FinalPrice = decimal.Parse(finalPrice),
                NumberOfProducts = int.Parse(numberOfProducts),
                UserName = userName,
                Email = email,
                Address = address
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(string paymentMethod)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? couponId = HttpContext.Session.GetInt32("AppliedCouponId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var carts = await _context.Carts
                .Where(c => c.CustomerId == userId.Value && c.Status == "Active")
                .ToListAsync();

            if (carts.Count == 0)
            {
                TempData["CartEmpty"] = "Your cart is empty.";
                return RedirectToAction("ShowCart", "Cart");
            }

            foreach (var cart in carts)
            {
                var order = new Order
                {
                    CartId = cart.Id,
                    Status = "Pending",
                    Amount = decimal.Parse(HttpContext.Session.GetString("FinalPrice")),
                    ModeOfPayment = paymentMethod,
                    CouponId = couponId
                };

                _context.Orders.Add(order);

                cart.Status = "Ordered";

            }

            if (couponId.HasValue)
            {
                var couponUsage = new CouponUsage
                {
                    CustomerId = userId.Value,
                    CouponId = couponId.Value,
                    UsageDate = DateOnly.FromDateTime(DateTime.Now)
                };

                _context.CouponUsages.Add(couponUsage);

                var coupon = await _context.Coupons.FindAsync(couponId.Value);
                if (coupon != null)
                {
                    coupon.Prize = 0; 
                    _context.Update(coupon);
                }
            }

            await _context.SaveChangesAsync();

            return View();
        }
    }
}

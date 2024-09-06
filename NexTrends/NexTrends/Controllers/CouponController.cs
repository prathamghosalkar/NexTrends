using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;

namespace NexTrends.Controllers
{

    public class CouponController : Controller
    {
       
        private readonly NexTrendsContext _context;

        public CouponController(NexTrendsContext context)
        {
            _context = context;
        }

        public IActionResult DisplayCoupon(bool showAddCoupon = false)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                var coupons = _context.Coupons.ToList();
                var categories = _context.Categories.ToList();

                ViewBag.Coupons = coupons;
                ViewBag.Categories = categories;
                ViewBag.ShowAddCoupon = showAddCoupon;

                return View();

            }

           return RedirectToAction("Login", "Account");
        }



        public IActionResult AddCoupon()
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                var categories = _context.Categories.ToList();
                ViewBag.Categories = categories;
                return PartialView("_AddCouponPartial");

            }
            return RedirectToAction("Login", "Account");

           
        }



        [HttpPost]
        public IActionResult AddCoupon(string coupancode, decimal discount, DateOnly expirydate, string? occasion, int? prize, int? categoryId)
        {

            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                if (string.IsNullOrWhiteSpace(coupancode))
                {
                    ModelState.AddModelError("coupancode", "Coupon code is required.");
                    return View();
                }

                var coupon = new Coupon
                {
                    CouponCode = coupancode,
                    DiscountPercentage = discount,
                    ExpiryDate = expirydate,
                    Occasion = occasion,
                    Prize = prize,
                    CategoryId = categoryId
                };

                _context.Add(coupon);
                try
                {
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Coupon added successfully!";
                    return RedirectToAction("DisplayCoupon");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while adding the coupon.";
                }

                return View();

            }
            return RedirectToAction("Login", "Account");

           
        }

        public IActionResult Delete(int id)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                var coupon = _context.Coupons.Find(id);
                if (coupon == null)
                {
                    return NotFound();
                }

                return View(coupon);

            }
            return RedirectToAction("Login", "Account");

             
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                var coupon = _context.Coupons.Find(id);
                if (coupon != null)
                {
                    _context.Coupons.Remove(coupon);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Coupon deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Coupon not found.";
                }

                return RedirectToAction("DisplayCoupon");

            }
            return RedirectToAction("Login", "Account");
            
        }


        public IActionResult Edit(int id)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {

                var coupon = _context.Coupons.Find(id);
                if (coupon == null)
                {
                    return NotFound();
                }
                return View(coupon);

            }
            return RedirectToAction("Login", "Account");

        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Coupon coupon)
        {
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin")
            {
                if (!ModelState.IsValid)
                {
                    return View(coupon);
                }

                // Update the coupon
                _context.Coupons.Update(coupon);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Coupon updated successfully!";
                return RedirectToAction("DisplayCoupon");

            }
            return RedirectToAction("Login", "Account");
            
        }
    }



}




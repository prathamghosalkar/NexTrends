using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NexTrends.Controllers
{
    
    public class CartController : Controller
    {
        private readonly NexTrendsContext context;

        public CartController(NexTrendsContext context)
        {
            this.context = context;
        }

        
        // Method to display the cart with items and available coupons
        public async Task<IActionResult> ShowCart()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

          
            var cartItems = await context.Carts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == userId.Value && c.Status == "Active")
                .ToListAsync();

            var categoryIdsInCart = cartItems.Select(c => c.Product.CategoryId).Distinct().ToList();
            var usedCouponIds = await context.CouponUsages
                .Where(cu => cu.CustomerId == userId.Value)
                .Select(cu => cu.CouponId)
                .ToListAsync();
            var availableCoupons = await context.Coupons
                .Where(c => categoryIdsInCart.Contains(c.CategoryId.Value)
                            && !usedCouponIds.Contains(c.Id)
                            && c.ExpiryDate > DateOnly.FromDateTime(DateTime.Now))
                .ToListAsync();

            var viewModel = new CartViewModel
            {
                CartItems = cartItems,
                AvailableCoupons = availableCoupons
            };

            var appliedCouponId = HttpContext.Session.GetInt32("AppliedCouponId");
            if (appliedCouponId.HasValue)
            {
                var discountPercentage = HttpContext.Session.GetInt32("DiscountPercentage");
                if (discountPercentage.HasValue)
                {
                    viewModel.AppliedCouponId = appliedCouponId.Value;
                    viewModel.DiscountPercentage = discountPercentage.Value;
                    viewModel.DiscountAmount = viewModel.CartItems.Sum(i => i.Product.Price * i.Quantity) * (discountPercentage.Value / 100m);
                }
            }

            HttpContext.Session.SetString("CartData", Newtonsoft.Json.JsonConvert.SerializeObject(viewModel));

            return View("AddToCart", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToCart(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { redirectTo = Url.Action("Login", "Account") });
            }

            var product = await context.Products.FirstOrDefaultAsync(s => s.Id == id);

            if (product != null && product.Quantity > 0)
            {
                var existingCartItem = await context.Carts
                    .FirstOrDefaultAsync(c => c.ProductId == id && c.CustomerId == userId.Value && c.Status == "Active");

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity = Math.Min(existingCartItem.Quantity + 1, 5);
                    existingCartItem.Price = product.Price * existingCartItem.Quantity;
                    context.Carts.Update(existingCartItem);
                }
                else
                {
                    Cart cart = new Cart
                    {
                        ProductId = id,
                        Quantity = 1,
                        Price = product.Price,
                        Status = "Active",
                        CustomerId = userId.Value
                    };

                    context.Carts.Add(cart);
                }

                await context.SaveChangesAsync();
                return Json(new { success = true, message = "Product added to cart." });
            }

            return Json(new { success = false, message = "Product not found or out of stock." });
        }


        public async Task<IActionResult> UpdateQuantity(int cartId, int newQuantity)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId && c.CustomerId == userId.Value && c.Status == "Active");

            if (cartItem != null && newQuantity > 0 && newQuantity <= 5)
            {
                var product = await context.Products.FirstOrDefaultAsync(p => p.Id == cartItem.ProductId);
                if (product != null)
                {
                    cartItem.Quantity = newQuantity;
                    cartItem.Price = product.Price * newQuantity;
                    context.Carts.Update(cartItem);
                    await context.SaveChangesAsync();
                }
            }

            return RedirectToAction("ShowCart");
        }

        // Method to remove a product from the cart
        
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cartItem = await context.Carts
                .FirstOrDefaultAsync(c => c.Id == cartId && c.CustomerId == userId.Value && c.Status == "Active");

            if (cartItem != null)
            {
                context.Carts.Remove(cartItem);
                await context.SaveChangesAsync();
            }

            return RedirectToAction("ShowCart");
        }

        // Method to apply a coupon
        
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(int couponId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var coupon = await context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon != null)
            {
                HttpContext.Session.SetInt32("AppliedCouponId", couponId);
                HttpContext.Session.SetInt32("DiscountPercentage", Convert.ToInt32(coupon.DiscountPercentage));

                return RedirectToAction("ShowCart");
            }

            TempData["CouponError"] = "Invalid or expired coupon.";
            return RedirectToAction("ShowCart");
        }


        // Add this method to CartController
       
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await context.Customers.FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null)
            {
                return RedirectToAction("ShowCart");
            }

            var cartItems = await context.Carts
                .Include(c => c.Product)
                .Where(c => c.CustomerId == userId.Value && c.Status == "Active")
                .ToListAsync();

            if (cartItems.Count == 0)
            {
                TempData["CartEmpty"] = "Your cart is empty.";
                return RedirectToAction("ShowCart");
            }

            var totalPrice = cartItems.Sum(i => i.Product.Price * i.Quantity);
            var discountAmount = 0m;

            var appliedCouponId = HttpContext.Session.GetInt32("AppliedCouponId");
            if (appliedCouponId.HasValue)
            {
                var discountPercentage = HttpContext.Session.GetInt32("DiscountPercentage") ?? 0;
                discountAmount = totalPrice * (discountPercentage / 100m);
            }

            var finalPrice = totalPrice - discountAmount;
            var numberOfProducts = cartItems.Sum(i => i.Quantity);

            HttpContext.Session.SetString("TotalPrice", totalPrice.ToString());
            HttpContext.Session.SetString("Discount", discountAmount.ToString());
            HttpContext.Session.SetString("FinalPrice", finalPrice.ToString());
            HttpContext.Session.SetString("NumberOfProducts", numberOfProducts.ToString());
            HttpContext.Session.SetString("UserName", user.Name);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Address", user.Address);

            return RedirectToAction("Index", "ConfirmOrder");
        }


    }
}

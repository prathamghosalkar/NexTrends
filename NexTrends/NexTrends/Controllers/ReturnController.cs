using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;

namespace NexTrends.Controllers
{
    
    public class ReturnController : Controller
    {
        NexTrendsContext _context = new NexTrendsContext();

        [HttpGet]
        public IActionResult ReturnOrder(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            var cart=_context.Carts.FirstOrDefault(o => o.Id == order.CartId);
            var product=_context.Products.FirstOrDefault(o => o.Id == cart.ProductId);
            var model = new ReturnProductViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Order = order,
                Product_Q = cart.Quantity,
                PricePerUnit = product.Price
            };
            return View(model);
        }

        [HttpPost]  
        public IActionResult SubmitReturnRequest(int OrderId, int QuantityToReturn, decimal TotalAmount, string Reasons, string AdditionalD)
        {
            try
            {
                var returnRequest = new ReturnRequest
                {
                    OrderId = OrderId,
                    Quantity = QuantityToReturn,
                    TotalAmount = TotalAmount,
                    ReturnReason = Reasons,
                    AdditionalDetails = AdditionalD,
                    RequestDate = DateTime.Now,
                    Status="Pending",
                    Order = _context.Orders.FirstOrDefault(s => s.Id == OrderId)

                };

                _context.ReturnRequests.Add(returnRequest);
                int result = _context.SaveChanges();
                if (result > 0)
                {
                    TempData["MSG2"] = "Request Sent Successfully!";
                    return RedirectToAction("CProfile", "Customer");
                }


            }
            catch (Exception ex) {
                TempData["MSG2"] = "Request is not sent ,try again!";
            }


            TempData["MSG2"] = "Request is not sent ,try again!";
            return RedirectToAction("ReturnOrder"); }
    }
}

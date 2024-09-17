using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NexTrends.Controllers
{
    public class ConfirmOrderController : Controller
    {
        private readonly NexTrendsContext _context;
        private readonly IEmailService _emailService;
        private List<Order> OrderItems;  // Ensure initialization


        public ConfirmOrderController(NexTrendsContext context, IEmailService emailService, IConfiguration config)
        {
            _context = context;
            _emailService = emailService;
            OrderItems = new List<Order>();
            StripeConfiguration.ApiKey = config.GetSection("Stripe")["SecretKey"];

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

            // Create Stripe payment session
            var domain = "https://localhost:7259/";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(decimal.Parse(HttpContext.Session.GetString("FinalPrice")) * 100),
                            Currency = "inr",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Order Payment",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + $"ConfirmOrder/OrderSuccess?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = domain + $"Home/Index",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public async Task<IActionResult> OrderSuccess(string session_id)
        {
            // Retrieve the session to confirm the payment status
            var service = new SessionService();
            var session = service.Get(session_id);

            if (session.PaymentStatus == "paid")
            {
                // Get user and cart data
                int? userId = HttpContext.Session.GetInt32("UserId");
                int? couponId = HttpContext.Session.GetInt32("AppliedCouponId");

                var carts = await _context.Carts
                    .Where(c => c.CustomerId == userId.Value && c.Status == "Active")
                    .ToListAsync();

                if (carts.Count == 0)
                {
                    return View("Failure");
                }

                // Add orders to the database after successful payment
                foreach (var cart in carts)
                {
                    var order = new Order
                    {
                        CartId = cart.Id,
                        Status = "Completed",  // Mark the order as completed after payment success
                        Amount = decimal.Parse(HttpContext.Session.GetString("FinalPrice")),
                        ModeOfPayment = "Card",
                        CouponId = couponId,
                        OrderDate = DateTime.Now
                    };

                    _context.Orders.Add(order);
                    OrderItems.Add(order);
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

                // After saving the order, generate PDF and send invoice email
                await SendInvoiceEmail(OrderItems);

                return View("OrderSuccess");
            }

            return View("Failure");
        }

        public byte[] GeneratePDF(List<Order> orderItems)
        {
            var totalPrice = HttpContext.Session.GetString("TotalPrice");
            var discount = HttpContext.Session.GetString("Discount");
            var finalPrice = HttpContext.Session.GetString("FinalPrice");
            var userName = HttpContext.Session.GetString("UserName");

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Add Header
                document.Add(new Paragraph("NexTrends")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(24)
                    .SetBold());

                document.Add(new Paragraph("Order Invoice")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetBold());

                document.Add(new Paragraph("Customer Name: " + userName)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFontSize(16)
                    .SetBold());

                document.Add(new Paragraph("Date: " + DateTime.Now.ToShortDateString())
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFontSize(16)
                    .SetBold());

                document.Add(new Paragraph("Order Details")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT)
                    .SetFontSize(16)
                    .SetBold()
                    .SetMarginTop(20));

                var table = new Table(3).UseAllAvailableWidth();
                table.AddHeaderCell("Product")
                     .AddHeaderCell("Quantity")
                     .AddHeaderCell("Price");

                foreach (var order in orderItems)
                {
                    var cart = _context.Carts.FirstOrDefault(s => s.Id == order.CartId);
                    if (cart != null)
                    {
                        var productObj = _context.Products.FirstOrDefault(s => s.Id == cart.ProductId);
                        if (productObj != null)
                        {
                            table.AddCell(new Paragraph(productObj.Name).SetFontSize(12));
                            table.AddCell(new Paragraph(cart.Quantity.ToString()).SetFontSize(12));
                            table.AddCell(new Paragraph(cart.Price.ToString()).SetFontSize(12));
                        }
                    }
                }

                document.Add(table);

                var totalsTable = new Table(2).UseAllAvailableWidth();
                totalsTable.AddCell(new Paragraph("Total Price:").SetBold().SetFontSize(12));
                totalsTable.AddCell(new Paragraph(totalPrice).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));

                totalsTable.AddCell(new Paragraph("Discount:").SetBold().SetFontSize(12));
                totalsTable.AddCell(new Paragraph(discount).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));

                totalsTable.AddCell(new Paragraph("Final Price:").SetBold().SetFontSize(12));
                totalsTable.AddCell(new Paragraph(finalPrice).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetFontSize(12));

                document.Add(totalsTable);

                document.Close();

                return ms.ToArray();
            }
        }

        public async Task SendInvoiceEmail(List<Order> orderItems)
        {
            var email = HttpContext.Session.GetString("Email");
            var user = _context.Customers.FirstOrDefault(s => s.Email == email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var pdfInvoice = GeneratePDF(orderItems);
            var subject = "Your Order Invoice";
            var bodyText = $"Dear {user.Name},\n\nPlease find attached your invoice for your order(s).\n\nThank you for shopping with us!";
            string attachments = "Invoice.pdf";
            //var attachments = new List<Attachment>
            //{
            //    new Attachment(new MemoryStream(pdfInvoice), "OrderInvoice.pdf", "application/pdf")
            //};

            await _emailService.SendAsyncWithBody(email, subject, bodyText, pdfInvoice, attachments);
        }
    }
}

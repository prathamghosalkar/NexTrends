using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;
using NexTrends.Models;
using System.Security.Cryptography;

namespace NexTrends.Controllers
{
   
    public class ProductController : Controller
    {
        NexTrendsContext context = new NexTrendsContext();
        public IActionResult Products(int Id)
        {
            int Cid = Convert.ToInt32(Id);
            Category cat = new Category();
            var context = new NexTrendsContext();
            cat = context.Categories.Find(Cid);
            return View(cat);
        }

        public IActionResult AddProduct(string Name, decimal Price, string Desc, int Quantity, int Cid, IFormFile Image)
        {
            // Check if the quantity is greater than 0
            if (Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Product quantity must be greater than 0.";
                return RedirectToAction("Category", "Category");
            }

            Product pr = new Product();
            var context = new NexTrendsContext();
            pr.Name = Name;
            pr.Price = Price;
            pr.Description = Desc;
            pr.Quantity = Quantity;
            pr.CategoryId = Cid;

            if (Image != null && Image.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    Image.CopyTo(memoryStream);
                    pr.Image = memoryStream.ToArray();
                }
            }

            context.Products.Add(pr);
            var re = context.SaveChanges();

            if (re > 0)
            {
                TempData["SuccessMessage"] = "Product added successfully!";
                return RedirectToAction("Category", "Category");
            }

            // In case saving to the database fails for other reasons
            TempData["ErrorMessage"] = "Failed to add the product. Please try again.";
            return RedirectToAction("Category", "Category");
        }


        public IActionResult ProductsList(int id, string SerchPr = "")
        {
            var productsQuery = context.Products.Where(p => p.CategoryId == id);
            if (!string.IsNullOrEmpty(SerchPr))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(SerchPr) ||
                    p.Name.EndsWith(SerchPr) ||
                    p.Name.StartsWith(SerchPr)
                );
            }

            var products = productsQuery.ToList();


            ViewBag.CID = id;

            if (products.Any())
            {
                return View(products);
            }
            else
            {
                ViewBag.ResultSr = "There are no products matching your search criteria.";
                return View(new List<Product>());
            }
        }



        public IActionResult GetImage(int id)
        {
            var product = context.Products
                .Where(p => p.Id == id)
                .Select(p => p.Image)
                .FirstOrDefault();

            if (product != null)
            {
                return File(product, "image/jpeg");
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult Delete(int Id)
        {
            var Pr = context.Products.FirstOrDefault(p => p.Id == Id);
            if (Pr != null)
            {
                context.Products.Remove(Pr);
                var Re = context.SaveChanges();
                if (Re > 0)
                {
                    TempData["DeleteMessage"] = "Product Deleted!";
                    return RedirectToAction("Category", "Category");
                }
            }

            return RedirectToAction("Category", "Category");
        }

        public IActionResult EditProduct(int Id)
        {
            var Pr = context.Products.FirstOrDefault(p => p.Id == Id);
            if (Pr != null)
            {
                return View(Pr);
            }
            return RedirectToAction("ProductsList");

        }

        [HttpPost]
        public IActionResult EditProduct(int PID, string Name, decimal Price, string Desc, int Quantity, int Cid)
        {
            var pr = context.Products.FirstOrDefault(p => p.Id == PID);
            if (pr != null)
            {
                pr.Name = Name;
                pr.Price = Price;
                pr.Description = Desc;
                pr.Quantity = Quantity;
                pr.CategoryId = Cid;
                var re = context.SaveChanges();
                if (re > 0)
                {
                    TempData["SuccessMessage"] = "Product Updated successfully!";
                    return RedirectToAction("Category", "Category");
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Product Is Not Updated!";
                return RedirectToAction("Category", "Category");
            }

            return RedirectToAction("Category", "Category");
        }


    }
}

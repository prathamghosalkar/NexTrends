using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NexTrends.Controllers
{
    public class AccountController : Controller
    {
        private readonly NexTrendsContext _context;

        public AccountController(NexTrendsContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View(); 
        }


        [HttpPost]
        public async Task<IActionResult> Register(string Name,string Email,string Phone,string Address,string Gender,int Pincode,string Pass)
        {
            Customer customer = new Customer();
            if (ModelState.IsValid)
            {
                // Check if the email already exists in either Customer or Admin list
                bool emailExists = await _context.Customers.AnyAsync(c => c.Email == Email) ||
                                   await _context.Admins.AnyAsync(a => a.Email == Email);
                
                if (emailExists)
                {

                    TempData["MSG"] = "Email already exists.";
                    return View(); // Return the current customer model with the error message
                }

                
                customer.Name = Name;
                customer.Address=Address;
                customer.Gender=Gender;
                customer.Phone = Phone;
                customer.Pincode = Convert.ToString(Pincode);
                customer.Email = Email;
                customer.Password= Pass;
                customer.Role = "customer";
               
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // After registration, redirect to the Login page
                TempData["MSG"] = "Registered Sucessfully, Now Login";
                return RedirectToAction("Login", "Account");
            }
            TempData["MSG"] = "Unable to register";
            return View(customer); // Return the view with model errors if any
        }



        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (ModelState.IsValid)
            {
                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
                if (admin != null && admin.Password == password)
                {
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetString("UserRole", "admin");
                    return RedirectToAction("Index", "Admin"); 
                }

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email && c.Password == password);
                if (customer != null)
                {
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetString("UserRole", "customer"); 
                    HttpContext.Session.SetInt32("UserId", customer.Id);
                    
                    return RedirectToAction("Index", "Home");
                }

                
            }
            TempData["MSG"] = "Invalid email or password.";

            return View(); 
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Login", "Account");
        }
    }
}

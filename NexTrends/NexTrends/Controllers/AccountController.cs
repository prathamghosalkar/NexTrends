using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexTrends.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;

namespace NexTrends.Controllers
{
    public class AccountController : Controller
    {
        private readonly NexTrendsContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(NexTrendsContext context, IEmailService emailService, ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public IActionResult Register()
        {
            return View();
        }

        // For register
        [HttpPost]
        public async Task<IActionResult> Register(string Name, string Email, string Phone, string Address, string Gender, int Pincode, string Pass)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists in either Customer or Admin list
                bool emailExists = await _context.Customers.AnyAsync(c => c.Email == Email) ||
                                   await _context.Admins.AnyAsync(a => a.Email == Email);

                if (emailExists)
                {
                    TempData["MSG"] = "Email already exists.";
                    return View();
                }

                try
                {
                    // Generate a 4-digit OTP
                    var otp = GenerateOTP();

                    // Store OTP and generation time in session
                    HttpContext.Session.SetString("OTP", otp);
                    HttpContext.Session.SetString("OTPGeneratedAt", DateTime.Now.ToString());
                    HttpContext.Session.SetString("REmail", Email);

                    // Send the OTP via email
                    await SendVerificationEmail(Email, otp);

                    // Create customer record with OTP and IsEmailVerified initially set to false
                    var customer = new Customer
                    {
                        Name = Name,
                        Email = Email,
                        Phone = Phone,
                        Address = Address,
                        Gender = Gender,
                        Pincode = Pincode.ToString(),
                        Password = Pass,
                        EmailVerificationToken = otp,
                        IsEmailVerified = false,
                        Role = "customer"
                    };
                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();

                    TempData["MSG"] = "A verification email has been sent. Please enter the OTP to complete registration.";
                    return RedirectToAction("EmailVerificationNotice");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Registration failed: {ex.Message}");
                    TempData["MSG"] = $"Registration failed: {ex.Message}";
                    return View();
                }
            }

            TempData["MSG"] = "Unable to register. Please check the form for errors.";
            return View();
        }

        // OTP verification page
        public IActionResult EmailVerificationNotice()
        {
            return View();
        }

        // Handle OTP submission
        [HttpPost]
        public IActionResult VerifyOTP(string otp0, string otp1, string otp2, string otp3)
        {
            try
            {
                string? otpFromSession = HttpContext.Session.GetString("OTP");
                string? otpGeneratedAtString = HttpContext.Session.GetString("OTPGeneratedAt");
                string? email = HttpContext.Session.GetString("REmail");

                if (otpFromSession == null || otpGeneratedAtString == null || email == null)
                {
                    TempData["MSG"] = "Session expired. Please request a new OTP.";
                    return RedirectToAction("EmailVerificationNotice");
                }

                DateTime otpGeneratedAt = DateTime.Parse(otpGeneratedAtString);
                TimeSpan otpValidityDuration = TimeSpan.FromMinutes(5); // OTP is valid for 5 minutes

                if (DateTime.Now - otpGeneratedAt > otpValidityDuration)
                {
                    TempData["MSG"] = "OTP has expired. Please request a new OTP.";
                    return RedirectToAction("EmailVerificationNotice");
                }

                string otp = $"{otp0}{otp1}{otp2}{otp3}";
                if (otpFromSession == otp)
                {
                    // Update email verification status in the database
                    var customer = _context.Customers.FirstOrDefault(c => c.Email == email);
                    if (customer != null)
                    {
                        customer.IsEmailVerified = true;
                        customer.EmailVerificationToken = null;
                        _context.SaveChanges();
                    }

                    TempData["MSG"] = "Email verified successfully!";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    TempData["MSG"] = "Invalid OTP. Please try again.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during OTP verification: {ex.Message}");
                TempData["MSG"] = $"Error during OTP verification: {ex.Message}";
            }

            return RedirectToAction("EmailVerificationNotice");
        }

        // Resend OTP
        public async Task<IActionResult> ResendOTP()
        {
            string? email = HttpContext.Session.GetString("REmail");

            if (email != null)
            {
                // Generate a new OTP and send it via email
                var otp = GenerateOTP();
                HttpContext.Session.SetString("OTP", otp);
                HttpContext.Session.SetString("OTPGeneratedAt", DateTime.Now.ToString());

                await SendVerificationEmail(email, otp);
                TempData["MSG"] = "A new OTP has been sent to your email.";
            }
            else
            {
                TempData["MSG"] = "Session expired. Please register again.";
            }

            return RedirectToAction("EmailVerificationNotice");
        }

        // Generate a 4-digit OTP
        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(1000, 9999).ToString("D4");
        }

        // Send OTP verification email
        private async Task SendVerificationEmail(string email, string otp)
        {
            var message = $"Your OTP for email verification is: {otp}.";
            await _emailService.SendAsync(email, "Verify your email", message);
        }

        public IActionResult Login()
        {
            // Generate CAPTCHA for login page
            var captchaText = GenerateRandomText();
            HttpContext.Session.SetString("CaptchaText", captchaText);
            var captchaImage = GenerateCaptchaImage(captchaText);
            ViewData["CaptchaImage"] = Convert.ToBase64String(captchaImage);

            return View();
        }

        // Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string captchaInput)
        {
            if (ModelState.IsValid)
            {
                var captchaValid = VerifyCaptcha(captchaInput);
                if (!captchaValid)
                {
                    TempData["MSG"] = "CAPTCHA validation failed.";
                    // Generate new CAPTCHA
                    var newcaptchaText = GenerateRandomText();
                    HttpContext.Session.SetString("CaptchaText", newcaptchaText);
                    var newcaptchaImage = GenerateCaptchaImage(newcaptchaText);
                    ViewData["CaptchaImage"] = Convert.ToBase64String(newcaptchaImage);

                    return View(); // Return the view with the new CAPTCHA image
                }

                var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
                if (admin != null && admin.Password == password)
                {
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetString("UserRole", "admin");
                    return RedirectToAction("Index", "Admin");
                }

                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email && c.Password == password && c.IsEmailVerified);
                if (customer != null)
                {
                    HttpContext.Session.SetString("Email", email);
                    HttpContext.Session.SetString("UserRole", "customer");
                    HttpContext.Session.SetInt32("UserId", customer.Id);

                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["MSG"] = "Invalid email or password.";
            // Generate new CAPTCHA
            var newCaptchaText = GenerateRandomText();
            HttpContext.Session.SetString("CaptchaText", newCaptchaText);
            var newCaptchaImage = GenerateCaptchaImage(newCaptchaText);
            ViewData["CaptchaImage"] = Convert.ToBase64String(newCaptchaImage);

            return View();
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Email");
            return RedirectToAction("Login", "Account");
        }

        private bool VerifyCaptcha(string captchaInput)
        {
            var storedCaptcha = HttpContext.Session.GetString("CaptchaText");
            return !string.IsNullOrEmpty(storedCaptcha) && storedCaptcha.Equals(captchaInput, StringComparison.OrdinalIgnoreCase);
        }

        private byte[] GenerateCaptchaImage(string captchaText, int width = 120, int height = 40)
        {
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);

                var font = new Font("Arial", 16, FontStyle.Bold);
                var brush = new SolidBrush(Color.Black);

                graphics.DrawString(captchaText, font, brush, 10, 10);

                // Add some noise
                var random = new Random();
                for (int i = 0; i < 50; i++)
                {
                    graphics.DrawRectangle(new Pen(Color.Gray), random.Next(width), random.Next(height), 1, 1);
                }

                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        private string GenerateRandomText(int length = 5)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        // Forgot Password and Reset Password process
        public IActionResult ForgotPass()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPass(string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer != null)
            {
                var otp = GenerateOTP();
                HttpContext.Session.SetString("ResetOTP", otp);
                HttpContext.Session.SetString("OTPGeneratedAt", DateTime.Now.ToString());
                HttpContext.Session.SetString("REmail", email);

                await SendVerificationEmail(email, otp);

                return RedirectToAction("VerifyResetOTP");
            }

            TempData["MSG"] = "Email not found.";
            return View();
        }

        public IActionResult VerifyResetOTP()
        {
            return View();
        }

        [HttpPost]
        public IActionResult VerifyResetOTP(string otp0, string otp1, string otp2, string otp3)
        {
            string otp = $"{otp0}{otp1}{otp2}{otp3}";
            string? resetOtp = HttpContext.Session.GetString("ResetOTP");
            string? otpGeneratedAtString = HttpContext.Session.GetString("OTPGeneratedAt");

            if (resetOtp == null || otpGeneratedAtString == null)
            {
                TempData["MSG"] = "Session expired. Please request a new OTP.";
                return RedirectToAction("ForgotPass");
            }

            DateTime otpGeneratedAt = DateTime.Parse(otpGeneratedAtString);
            TimeSpan otpValidityDuration = TimeSpan.FromMinutes(5); // OTP is valid for 5 minutes

            if (DateTime.Now - otpGeneratedAt > otpValidityDuration)
            {
                TempData["MSG"] = "OTP has expired. Please request a new OTP.";
                return RedirectToAction("ForgotPass");
            }

            if (resetOtp == otp)
            {
                TempData["MSG"] = "OTP verified successfully!";
                return RedirectToAction("ResetPassword");
            }
            else
            {
                TempData["MSG"] = "Invalid OTP. Please try again.";
                return View();
            }
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string newPassword, string confirmPassword)
        {
            string? email = HttpContext.Session.GetString("REmail");

            if (newPassword == confirmPassword && email != null)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
                if (customer != null)
                {
                    customer.Password = newPassword;
                    _context.SaveChanges();
                    TempData["MSG"] = "Password reset successfully.";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["MSG"] = "Password reset failed. Please try again.";
            return View();
        }
    }
}

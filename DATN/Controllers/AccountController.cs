using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Security.Cryptography;
using DATN.Migrations;
using Microsoft.AspNetCore.Authorization;


namespace DATN.Controllers
{
    public class AccountController : Controller
    {
        private readonly DATNDbContext _context;
        private readonly EmailService _emailService;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AccountController(DATNDbContext context, EmailService emailService, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _emailService = emailService;
            _hostEnvironment = hostEnvironment;
        }



        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Handle login form submission
        [HttpPost]
        public async Task<IActionResult> LoginAsync(Account model)
        {
            // Fetch the user from the database by email
            var user = _context.Account.FirstOrDefault(u => u.Email == model.Email);

            if (user == null || user.Enabled==false)
            {
                // User not found, return error
                ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
                return View();
            }

            // Hash the entered password
            var hashedPassword = HashPassword(model.Password);

            // Compare the hashed password with the stored hashed password
            if (user.Password != hashedPassword)
            {
                // Password does not match
                ModelState.AddModelError(string.Empty, "Sai tài khoản hoặc mật khẩu.");
                return View();
            }

            // Create the user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, user.Name),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.StreetAddress, user.Address),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.UserData, user.CCCD),
                new Claim("UserId", user.ID.ToString()) // Add User ID as a claim
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in and create the cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
            return RedirectToAction("Index", "Home");
            
        }


        public async Task<IActionResult> Logout()
        {
            // Đăng xuất và xóa cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Info()
        {
            // Get the current user's email from the Identity
            var userEmail = User.Identity.Name;

            // Fetch the user from the database using their email
            var user = _context.Account.SingleOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return NotFound(); // If user is not found, return an error or redirect
            }

            return View(user); // Pass the user object to the view
        }

        [HttpPost]
        public async Task<IActionResult> Info(Account model)
        {
            var userEmail = User.Identity.Name;
            var user = await _context.Account.SingleOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound($"Không tồn tại người dùng với email '{userEmail}'.");
            }

            // Update user information
            user.Name = model.Name;
            user.CCCD = model.CCCD;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            string wwwRootPath = _hostEnvironment.WebRootPath;

            // Check if a new image is uploaded
            if (model.ImageFile != null)
            {
                // Path to the old image
                string oldImagePath = Path.Combine(wwwRootPath + "/Images/Account/", user.Avatar);

                // Delete the old image if it exists
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                // Create a new file name for the image
                string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
                string extension = Path.GetExtension(model.ImageFile.FileName);
                user.Avatar = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                string newPath = Path.Combine(wwwRootPath + "/Images/Account/", user.Avatar);

                // Save the new image to the directory
                using (var fileStream = new FileStream(newPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            }

            // Update the user in the database
            _context.Update(user);
            await _context.SaveChangesAsync();
            await UpdateUserClaims(user);

            // Redirect to the home page after successful update
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Account account)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(account.ImageFile.FileName);
            string extension = Path.GetExtension(account.ImageFile.FileName);
            account.Avatar = fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/Images/Account/", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await account.ImageFile.CopyToAsync(fileStream);
            }
            // Hash the user's password before saving it to the database
            account.Password = HashPassword(account.Password);

            // Set the default role as 'User'
            account.Role = "User";

            // Add the account to the database and save changes
            _context.Add(account);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Account.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy email.");
                return View();
            }

            // Generate a reset token
            var resetToken = Guid.NewGuid().ToString();

            // Save the token in the ResetPasswordTokens table
            var tokenEntity = new ResetPasswordToken
            {
                Email = email,
                Token = resetToken,
                ExpiryTime = DateTime.Now.AddHours(1) // Token expires in 1 hour
            };

            _context.resetPasswordTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            // Construct the reset password link
            var resetPasswordUrl = Url.Action("ResetPassword", "Account", new { token = resetToken }, Request.Scheme);

            // Send reset email
            var body = $"<p>Để thay đổi mật khẩu của bạn, nhấn vào <a href='{resetPasswordUrl}'>đây</a></p>";
            await _emailService.SendEmailAsync(email, "THAY ĐỔI MẬT KHẨU", body);

            // Redirect to confirmation page
            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }

            // Optionally, you can check if the token exists in the database before showing the form
            var resetToken = _context.resetPasswordTokens.SingleOrDefault(t => t.Token == token);
            if (resetToken == null || resetToken.ExpiryTime < DateTime.Now)
            {
                return BadRequest("Không tìm thấy yêu cầu.");
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {

            var resetToken = await _context.resetPasswordTokens.SingleOrDefaultAsync(t => t.Token == token);
             
            if (resetToken == null || resetToken.ExpiryTime < DateTime.Now)
            {
                return BadRequest("Không tìm thấy yêu cầu.");
            }

            // Proceed with resetting the password
            var user = await _context.Account.SingleOrDefaultAsync(u => u.Email == resetToken.Email);
            if (user == null)
            {
                return BadRequest("Người dùng không tồn tại.");
            }

            user.Password = HashPassword(newPassword); // Ensure you hash the new password
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Remove the token after successful password reset
            _context.resetPasswordTokens.Remove(resetToken);
            await _context.SaveChangesAsync();

            return RedirectToAction("ResetPasswordSuccess");
        }


        // Success page after password reset
        public IActionResult ResetPasswordSuccess()
        {
            return View();
        }

        private async Task UpdateUserClaims(Account user)
        {
            // Get the current principal (logged-in user)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, user.Name),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            // Add other claims if necessary
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign the user in again with updated claims
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
        }




        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}

using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DATN.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AccessAmController : Controller
    {
        private readonly DATNDbContext _context;

        public AccessAmController(DATNDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ListAccount()
        {
            var accounts = _context.Account.ToList(); // Fetch all accounts from the database
            return View(accounts);
        }

        // Action to load the update view
        [HttpGet]
        public IActionResult Update(int id)
        {
            var account = _context.Account.FirstOrDefault(a => a.ID == id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account); // Pass the selected account to the update view
        }

        // Action to handle the update form submission
        [HttpPost]
        public async Task<IActionResult> Update(Account account)
        {
            var user = await _context.Account.FindAsync(account.ID);

            if (user == null)
            {
                return NotFound();
            }

            // Update user information
            user.Email = account.Email;
            user.Role = account.Role;
            user.Enabled = account.Enabled;  // The updated Enable value is handled here

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListAccount");
        }
    }
}

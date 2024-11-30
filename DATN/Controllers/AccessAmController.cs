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

        public IActionResult Index(int? year)
        {
            // Nếu không có năm được chọn, mặc định lấy năm hiện tại            
            int selectedYear = year ?? DateTime.Now.Year;

            // Tổng số phòng
            int totalRooms = _context.Rooms.Count();

            // Số phòng đã đặt trong năm đã chọn
            int bookedRooms = _context.BookingRoom
                .Where(br => br.Booking.CreatedAt.HasValue && br.Booking.Payment == "Đã thanh toán" && br.Booking.CreatedAt.Value.Year == selectedYear)
                .Sum(br => br.Quantity);

            // Số dịch vụ đã đặt trong năm đã chọn
            int bookedServices = _context.BookingService
                .Where(bs => bs.Booking.CreatedAt.HasValue && bs.Booking.Payment == "Đã thanh toán" && bs.Booking.CreatedAt.Value.Year == selectedYear)
                .Count();

            // Truyền dữ liệu sang View
            ViewBag.TotalRooms = totalRooms;
            ViewBag.BookedRooms = bookedRooms;
            ViewBag.BookedServices = bookedServices;

            // Truy vấn doanh thu theo tháng trong năm được chọn
            var revenueByMonth = _context.Booking
                .Where(b => b.CreatedAt.HasValue && b.Payment == "Đã thanh toán" && b.CreatedAt.Value.Year == selectedYear)
                .GroupBy(b => b.CreatedAt.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalRevenue = g.Sum(b => b.Total)
                })
                .ToList();

            // Tạo danh sách 12 tháng với giá trị mặc định là 0
            var fullRevenueByMonth = Enumerable.Range(1, 12) // Từ tháng 1 đến tháng 12
                .Select(m => new
                {
                    Month = m,
                    TotalRevenue = revenueByMonth.FirstOrDefault(r => r.Month == m)?.TotalRevenue ?? 0
                })
                .OrderBy(r => r.Month)
                .ToList();
            // Truyền dữ liệu vào ViewBag
            ViewBag.RevenueByMonth = fullRevenueByMonth;
            ViewBag.SelectedYear = selectedYear;
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

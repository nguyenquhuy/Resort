using DATN.Migrations;
using DATN.Models;
using DATN.Models.Context;
using DATN.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DATN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DATNDbContext _context;

        public HomeController(ILogger<HomeController> logger, DATNDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

		//Danh sách phòng
		public IActionResult ListRoom()
        {
            return View(_context.Rooms.ToList());
        }

        //Chi tiết phòng
        public IActionResult DetailRoom(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the room details along with the RoomType
            var room = _context.Rooms
                                .Include(r => r.RoomType)
                                .FirstOrDefault(r => r.ID == id);

            if (room == null)
            {
                return NotFound();
            }

            // Fetch the gallery images for the room
            var galleryImages = _context.GalleryRooms
                                        .Where(g => g.RoomId == id)
                                        .ToList();

            // Fetch bookings for the room and convert to list first
            var bookings = _context.BookingRoom
                                   .Where(br => br.RoomId == id)
                                   .Include(br => br.Booking)
                                   .Where(br => br.Booking.CheckOut >= DateTime.Now)
                                   .ToList(); // Fetch data into memory

            // Generate the list of booked date ranges
            var bookedDates = bookings
                .Where(br => br.Booking.CheckIn.HasValue && br.Booking.CheckOut.HasValue)
                .SelectMany(br => GenerateDateRange(br.Booking.CheckIn.Value, br.Booking.CheckOut.Value))
                .Select(date => date.ToString("dd-MM-yyyy"))
                .ToList();

            // Create and initialize the ViewModel
            var viewModel = new RoomGal
            {
                Room = room,
                RoomGalleries = galleryImages ?? new List<GalleryRooms>(),
                Images = new List<IFormFile>()
            };

            // Pass the booked dates to the View
            ViewBag.BookedDates = bookedDates;

            return View(viewModel);
        }

        // Helper function to generate a range of dates
        private static IEnumerable<DateTime> GenerateDateRange(DateTime checkIn, DateTime checkOut)
        {
            for (var date = checkIn; date <= checkOut; date = date.AddDays(1))
            {
                yield return date;
            }
        }





        //Danh sách bài viết
        public IActionResult ListArticle()
        {
            return View(_context.Articles.ToList());
        }

        //Chi tiết bài viết
        public IActionResult DetailArticle(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var articles =  _context.Articles
                .FirstOrDefault(m => m.Id == id);
            if (articles == null)
            {
                return NotFound();
            }

            return View(articles);

        }
        //Danh sách dịch vụ
        public IActionResult ListService()
        {
            return View(_context.Services.ToList());
        }
        //Chi tiết dịch vụ
        public IActionResult DetailService(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var services = _context.Services
                .FirstOrDefault(m => m.Id == id);
            if (services == null)
            {
                return NotFound();
            }
            return View(services);
        }

        public IActionResult About()
        {
            var name = "About";
            return View(_context.Articles.FirstOrDefault(a=>a.Title==name));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

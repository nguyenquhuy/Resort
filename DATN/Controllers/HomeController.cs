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

            // Fetch the room details
            var room = _context.Rooms.Find(id);

            // Fetch the gallery images for the room
            var galleryImages = _context.GalleryRooms
                                        .Where(g => g.RoomId == id)
                                        .ToList();

            // Create and initialize the ViewModel
            var viewModel = new RoomGal
            {
                Room = room,
                RoomGalleries = galleryImages ?? new List<GalleryRooms>(), // Ensure RoomGalleries is not null
                Images = new List<IFormFile>() // Initialize Images if necessary
            };

            return View(viewModel);

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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using DATN.Models;
using DATN.Models.Context;
using DATN.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DATN.Controllers
{
    public class BookingController : Controller
    {
        private readonly DATNDbContext _context;

        public BookingController(DATNDbContext context)
        {
            _context = context;
        }

		[HttpPost]
		public IActionResult Index(int roomTypeId, DateTime checkInDate, DateTime checkOutDate)
		{
			// Lấy danh sách các phòng có loại phòng tương ứng
			var rooms = _context.Rooms
								.Where(r => r.RoomTypeId == roomTypeId)
								.ToList();

			// Lọc các phòng không bị đặt trùng với khoảng thời gian người dùng chọn
			var availableRooms = rooms
				.Where(r => !_context.BookingRoom
					.Any(br => br.RoomId == r.ID &&
							   br.Booking.CheckIn < checkOutDate &&
							   br.Booking.CheckOut > checkInDate))
				.ToList();

			// Trả lại view với danh sách phòng còn trống
			return View(availableRooms);
		}


		public IActionResult Index()
		{
			// Lấy tất cả các loại phòng để hiển thị trong dropdown
			var roomTypes = _context.RoomTypes.ToList();
			ViewBag.RoomTypes = roomTypes;
			return View();
		}

		public IActionResult MakeReservation(int roomId)
        {
            // Retrieve the room details from the database
            var room = _context.Rooms.Find(roomId);

            if (room == null)
            {
                return NotFound();
            }

            // Initialize BookRoom with Booking and Room
            BookRoom br = new BookRoom
            {
                Booking = new Booking(), // Create a new Booking instance
                Room = room // Set the Room instance to the retrieved room
            };

            return View(br);
        }

        [HttpPost]
        public IActionResult SubmitReservation(BookRoom bookRoom)
        {
            // Set the CreatedAt date
            bookRoom.Booking.CreatedAt = DateTime.Now;

            // Add the booking to the database
            _context.Booking.Add(bookRoom.Booking);
            _context.SaveChanges();

            // Create a BookingRoom entry to link the booking with the room
            var bookingRoom = new BookingRoom
            {
                BookingId = bookRoom.Booking.Id,
                RoomId = bookRoom.Room.ID
            };
            _context.BookingRoom.Add(bookingRoom);
            _context.SaveChanges();

            return RedirectToAction("BookingService");
        }

        public IActionResult BookingService() { return View(); }
        public IActionResult ReservationConfirmation()
        {
            return View();
        }

    }
}

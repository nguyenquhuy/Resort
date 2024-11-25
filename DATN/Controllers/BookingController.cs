using DATN.Migrations;
using DATN.Models;
using DATN.Models.Context;
using DATN.Models.Services;
using DATN.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;

namespace DATN.Controllers
{
    public class BookingController : Controller
    {
        private readonly DATNDbContext _context;
        private readonly EmailService _emailService;
        private readonly IVnPayService _vnPayService;

        public BookingController(DATNDbContext context, EmailService emailService, IVnPayService vnPayService)
        {
            _context = context;
            _emailService = emailService;
            _vnPayService = vnPayService;
        }


        //Danh sách đặt phòng Admin
        public IActionResult ListBooking() 
        {
            var booking = _context.Booking
                      .Include(b => b.BookingRooms)
                      .ThenInclude(br => br.Room)
                      .Include(s => s.BookingServices)
                      .ThenInclude(bs => bs.service)
                      .Include(a=>a.Account)
                      .ToList(); 
            return View(booking);
        }

        //Chi tiết đặt phòng admin
        public IActionResult Detail(int id) 
        {
            if(id == 0)
            {
                return NotFound();
            }
            var booking = _context.Booking
                                  .Include(b => b.BookingRooms)
                                  .ThenInclude(br => br.Room)
                                  .Include(s => s.BookingServices)
                                  .ThenInclude(bs => bs.service)
                                  .FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);

        }

        //Trang tìm kiếm phòng trống
        [HttpPost]
        public async Task<IActionResult> Index(int? roomTypeId, DateTime? checkInDate, DateTime? checkOutDate)
        {
            if(checkInDate==null || checkOutDate==null)
            {
                checkInDate = DateTime.Now;
                checkOutDate = DateTime.Now.AddDays(1);
            }
            // Get the list of all room types for the dropdown
            ViewBag.RoomTypes = await _context.RoomTypes.ToListAsync();
            ViewBag.CheckInDate = checkInDate;
            ViewBag.CheckOutDate = checkOutDate;

            // Query for available rooms based on filter criteria
            var roomsQuery = _context.Rooms
                                    .Include(r => r.RoomAmenities)
                                    .ThenInclude(ra => ra.Amenity) // Include amenities for each room
                                    .AsQueryable();

            if (roomTypeId.HasValue)
            {
                roomsQuery = roomsQuery.Where(r => r.RoomTypeId == roomTypeId.Value);
            }

            if (checkInDate.HasValue && checkOutDate.HasValue)
            {
                roomsQuery = roomsQuery.Where(r => !_context.BookingRoom
                    .Any(br => br.RoomId == r.ID &&
                               br.Booking.CheckIn < checkOutDate &&
                               br.Booking.CheckOut > checkInDate));
            }

            var availableRooms = await roomsQuery.ToListAsync();

            return View(availableRooms);
        }

        //Điền thông tin đặt phòng
        [HttpPost]
        public IActionResult MakeReservation(DateTime checkInDate, DateTime checkOutDate, Dictionary<int, int> quantities)
        {
            var roomsToBook = new List<(string RoomName, int Quantity, decimal Price, int RoomID)>();
            decimal totalPrice = 0;
            var services = _context.Services.ToList();

            // Tính số đêm
            int numberOfNights = (checkOutDate - checkInDate).Days;
            if (numberOfNights <= 0)
            {
                // Xử lý lỗi nếu ngày trả phòng <= ngày nhận phòng
                ModelState.AddModelError("", "Ngày trả phòng phải sau ngày nhận phòng.");
                return RedirectToAction("Index"); // Chuyển hướng về trang đặt phòng
            }

            foreach (var quantity in quantities)
            {
                var roomId = quantity.Key;
                var roomQuantity = quantity.Value;

                if (roomQuantity > 0)
                {
                    var room = _context.Rooms.FirstOrDefault(r => r.ID == roomId);
                    if (room != null)
                    {
                        // Tính tổng tiền cho từng phòng
                        decimal roomTotalPrice = room.Price * roomQuantity * numberOfNights;
                        totalPrice += roomTotalPrice;

                        roomsToBook.Add((room.Name, roomQuantity, room.Price, room.ID));
                    }
                }
            }

            ViewBag.Services = services; // Đưa danh sách dịch vụ vào ViewBag
            ViewBag.CheckInDate = checkInDate;
            ViewBag.CheckOutDate = checkOutDate;
            ViewBag.NumberOfNights = numberOfNights; // Đưa số đêm vào ViewBag
            ViewBag.TotalPrice = totalPrice; // Tổng tiền cho tất cả phòng

            return View(roomsToBook);
        }

        //Xác nhận và thanh toán
        [HttpPost]
        public async Task<IActionResult> SubmitReservation(string name, string address, string email, string phone, int child, int adult,  Dictionary<int, int> quantities, string checkin, string checkout, double total, List<int> selectedServices)
        {
            // Lấy userId từ Claim nếu có
            var userIdClaim = User.FindFirst("UserId");
            int? userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId)
                ? parsedUserId
                : (int?)null;
            // Chuyển đổi chuỗi thành DateTime
            DateTime checkinDate = DateTime.ParseExact(checkin, "dd/MM/yyyy", null);
            DateTime checkoutDate = DateTime.ParseExact(checkout, "dd/MM/yyyy", null);
            var totals = total;
            //}
            // Nếu có các dịch vụ đã chọn, cộng giá dịch vụ vào tổng
            if (selectedServices != null)
            {
                foreach (var serviceId in selectedServices)
                {
                    // Lấy dịch vụ từ cơ sở dữ liệu
                    var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
                    if (service != null)
                    {
                        totals += service.Price; // Cộng giá dịch vụ vào tổng
                    }
                }
            }

            var reservationData = new ReservationData
            {
                Name = name,
                Address = address,
                Email = email,
                Phone = phone,
                Child = child,
                Adult = adult,
                Quantities = quantities,
                CheckIn = checkin,
                CheckOut = checkout,
                Totals = totals,
                SelectedServices = selectedServices,
                UserId = userId
            };

            TempData["ReservationData"] = JsonConvert.SerializeObject(reservationData);

            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = totals,
                CreatedDate = DateTime.Now,
                Description = $"{name} thanh toán tiền đặt phòng",
                FullName = name,
                OrderId = new Random().Next(1000, 100000),
            };

            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
        }

        //Thanh toán và trả kết quả thanh toán
        public async Task<IActionResult> PaymentCallback(string vnp_ResponseCode, string vnp_TxnRef)
        {
            if (vnp_ResponseCode == "00") // Thanh toán thành công
            {
                // Lấy dữ liệu từ TempData
                var reservationDataJson = TempData["ReservationData"]?.ToString();
                if (string.IsNullOrEmpty(reservationDataJson))
                {
                    return RedirectToAction("Error", "Home");
                }

                var reservationData = JsonConvert.DeserializeObject<ReservationData>(reservationDataJson);

                // Chuyển đổi chuỗi thành DateTime
                DateTime checkinDate = DateTime.ParseExact(reservationData.CheckIn, "dd/MM/yyyy", null);
                DateTime checkoutDate = DateTime.ParseExact(reservationData.CheckOut, "dd/MM/yyyy", null);

                // Lưu thông tin đặt phòng vào cơ sở dữ liệu
                var booking = new Booking
                {
                    Name = reservationData.Name,
                    Address = reservationData.Address,
                    Email = reservationData.Email,
                    Phone = reservationData.Phone,
                    Child = reservationData.Child,
                    Adult = reservationData.Adult,
                    CreatedAt = DateTime.Now,
                    CheckIn = checkinDate,
                    CheckOut = checkoutDate,
                    Total = reservationData.Totals,
                    AccountId = reservationData.UserId
                };
                _context.Add(booking);
                _context.SaveChanges();

                // Lưu thông tin phòng
                foreach (var quantity in reservationData.Quantities)
                {
                    var room = _context.Rooms.FirstOrDefault(r => r.ID == quantity.Key);
                    if (room != null)
                    {
                        var bookingRoom = new BookingRoom
                        {
                            BookingId = booking.Id,
                            RoomId = room.ID,
                            Quantity = quantity.Value
                        };
                        _context.BookingRoom.Add(bookingRoom);
                    }
                }

                // Lưu các dịch vụ
                foreach (var serviceId in reservationData.SelectedServices)
                {
                    var service = _context.Services.FirstOrDefault(s => s.Id == serviceId);
                    if (service != null)
                    {
                        var bookingService = new BookingService
                        {
                            BookingId = booking.Id,
                            ServiceId = service.Id
                        };
                        _context.BookingService.Add(bookingService);
                    }
                }

                _context.SaveChanges();

                // Gửi email xác nhận
                var bookingURL = Url.Action("DetailBooking", "Booking", new { id = booking.Id }, Request.Scheme);
                var body = $@"
            <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <h2 style='color: #4CAF50; text-align: center;'>Cảm ơn bạn đã đặt phòng tại Trường Sinh Resort</h2>
                <p>Xin chào <strong>{booking.Name}</strong>,</p>
                <p>Chúng tôi rất vui mừng xác nhận đơn đặt phòng của bạn tại Trường Sinh Resort. Chúng tôi đã gửi chi tiết đặt phòng của bạn vào tài khoản này.</p>
                <p>Mã đơn của bạn là <strong>{booking.Id}</strong>. Để xem chi tiết đặt phòng, vui lòng nhấp vào liên kết dưới đây:</p>
                <p style='text-align: center;'>
                    <a href='{bookingURL}' 
                       style='background-color: #4CAF50; color: white; text-decoration: none; padding: 10px 20px; font-size: 16px; border-radius: 5px;'>
                        Xem chi tiết đặt phòng
                    </a>
                </p>
                <p style='margin-top: 20px;'>Nếu bạn có bất kỳ câu hỏi nào hoặc cần hỗ trợ, vui lòng liên hệ với chúng tôi qua email hoặc số điện thoại bên dưới.</p>
                <p style='color: #555; font-size: 14px;'>Trân trọng,<br>Đội ngũ Trường Sinh Resort</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                <footer style='text-align: center; font-size: 12px; color: #777;'>
                    <p>Trường Sinh Resort | Email: support@truongsinhresort.com | SĐT: 0123 456 789</p>
                    <p>&copy; 2024 Trường Sinh Resort. Tất cả các quyền được bảo lưu.</p>
                </footer>
            </div>";

                await _emailService.SendEmailAsync(reservationData.Email, "Đơn Đặt Phòng Của Bạn", body);

                return RedirectToAction("ReservationConfirmation", new { id = booking.Id });
            }

            // Nếu thanh toán thất bại
            return RedirectToAction("PaymentFailed", "Home");
        }

        //Chi tiết đặt phòng user
        public IActionResult DetailBooking(int id)
        {
            var booking = _context.Booking
                                  .Include(b => b.BookingRooms)
                                  .ThenInclude(br => br.Room)
                                  .Include(s => s.BookingServices)
                                  .ThenInclude(bs => bs.service)
                                  .FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }


        //Đặt phòng thành công
        public IActionResult ReservationConfirmation(int id)
        {
            var booking = _context.Booking
                    .Include(b => b.BookingRooms)
                    .ThenInclude(br => br.Room)
                    .Include(s => s.BookingServices)
                    .ThenInclude(bs=>bs.service)
                    .FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        //Hủy đặt phòng
        public IActionResult DeleteBooking(int id)
        {
            // Retrieve the booking
            var booking = _context.Booking.FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if the booking can be canceled
            if (booking.CheckIn.HasValue && booking.CheckIn.Value > DateTime.Now)
            {
                // Retrieve related entities
                var bookingRooms = _context.BookingRoom.Where(r => r.BookingId == id).ToList();
                var bookingServices = _context.BookingService.Where(s => s.BookingId == id).ToList();

                // Remove related entities
                _context.BookingRoom.RemoveRange(bookingRooms);
                _context.BookingService.RemoveRange(bookingServices);

                // Remove the booking itself
                _context.Booking.Remove(booking);
                _context.SaveChanges();

                // Set success message
                TempData["SuccessMessage"] = "Đơn đặt phòng đã được hủy thành công.";
            }
            else
            {
                // Set error message
                TempData["ErrorMessage"] = "Không thể hủy đơn đặt phòng vì thời gian Check-In đã bắt đầu hoặc đã qua.";
            }

            // Redirect to a relevant page (e.g., booking list or home page)
            return RedirectToAction("Index", "Home");
        }



    }
}

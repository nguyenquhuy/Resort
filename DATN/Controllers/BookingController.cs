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
        public IActionResult ListBooking(string Status, DateTime? StartDate, DateTime? EndDate)
        {
            var query = _context.Booking
                       .Include(b => b.BookingRooms)
                       .ThenInclude(br => br.Room)
                       .Include(s => s.BookingServices)
                       .ThenInclude(bs => bs.service)
                       .Include(a => a.Account)
                       .AsQueryable();

            // Lọc theo trạng thái nếu không phải "Tất cả"
            if (!string.IsNullOrEmpty(Status) && Status != "All")
            {
                query = query.Where(b => b.Status == Status);
            }

            // Lọc theo ngày nếu có
            if (StartDate.HasValue)
            {
                query = query.Where(b => b.CreatedAt.Value >= StartDate.Value.Date);
            }
            if (EndDate.HasValue)
            {
                query = query.Where(b => b.CreatedAt.Value <= EndDate.Value.Date);
            }

            var bookings = query.ToList();
            return View(bookings);
        }
        [HttpGet]
        public IActionResult FilterRooms(DateTime checkIn, DateTime checkOut)
        {
            // Lấy danh sách các phòng không được đặt trong khoảng thời gian này
            var availableRooms = _context.Rooms
                .Where(r => !_context.BookingRoom
                    .Any(br => br.RoomId == r.ID &&
                               ((br.Booking.CheckIn < checkOut && br.Booking.CheckOut > checkIn
                               && br.Booking.Status != "Hủy đặt phòng"))))
                .ToList();

            return PartialView("_AvailableRoomsPartial", availableRooms);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Lấy danh sách dịch vụ để hiển thị
            var services = _context.Services.ToList();

            // Truyền ViewModel với các dữ liệu cần thiết
            var viewModel = new BookingViewModel
            {
                Services = services
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookingViewModel viewModel)
        {
            var userIdClaim = User.FindFirst("UserId");
            int? userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId)
                ? parsedUserId
                : (int?)null;
            double total = 0;          

            // Tạo Booking mới
            var booking = new Booking
            {
                Name = viewModel.Booking.Name,
                Address = viewModel.Booking.Address,
                Email = viewModel.Booking.Email,
                Phone = viewModel.Booking.Phone,
                CheckIn = viewModel.CheckIn,
                CheckOut = viewModel.CheckOut,
                Child = viewModel.Booking.Child,
                Adult = viewModel.Booking.Adult,
                Total = 0, // Tổng tiền sẽ tính sau
                Payment = viewModel.Booking.Payment,
                Status = viewModel.Booking.Status,
                AccountId = userId,
                CreatedAt = DateTime.Now,
            };
            var night = (booking.CheckOut.Value - booking.CheckIn.Value).Days;
            // Lưu thông tin phòng đã chọn
            foreach (var roomId in viewModel.SelectedRoomIds)
            {
                var room = _context.Rooms.Find(roomId);
                if (room != null)
                {
                    booking.BookingRooms.Add(new BookingRoom
                    {
                        BookingId = booking.Id,
                        RoomId = room.ID,
                        Price = room.Price * 1, // Giá phòng
                        Quantity = 1        // Số lượng mặc định là 1
                    });
                    total += room.Price * night;
                }
            }

            // Lưu thông tin dịch vụ đã chọn
            foreach (var serviceId in viewModel.SelectedServiceIds)
            {
                var service = _context.Services.Find(serviceId);
                if (service != null)
                {
                    booking.BookingServices.Add(new BookingService
                    {
                        BookingId = booking.Id,
                        ServiceId = service.Id,
                        Price = service.Price // Giá dịch vụ
                    });
                    total += service.Price;
                }
            }

            // Tính tổng tiền
            booking.Total = total;
            booking.depositMoney = total * 0.5;
            // Lưu vào cơ sở dữ liệu
            _context.Booking.Add(booking);
            _context.SaveChanges();

            // Chuyển hướng đến trang xác nhận hoặc danh sách booking
            return RedirectToAction("ListBooking");
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                               .Include(b => b.BookingRooms)
                                   .ThenInclude(br => br.Room)
                               .Include(s => s.BookingServices)
                                   .ThenInclude(bs => bs.service)
                               .Include(a => a.Account)
                               .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Lấy danh sách phòng trống
            var bookedRoomIds = booking.BookingRooms.Select(br => br.RoomId).ToList();
            var availableRooms = await _context.Rooms
                .Where(r => !bookedRoomIds.Contains(r.ID) &&
                            !_context.BookingRoom
                                .Any(br => br.RoomId == r.ID &&
                                           br.Booking.CheckIn < booking.CheckOut &&
                                           br.Booking.CheckOut > booking.CheckIn&&
                                           br.Booking.Status != "Hủy đặt phòng"))
                .ToListAsync();

            // Lấy danh sách dịch vụ
            //var allServices = await _context.Services.ToListAsync();
            var registeredServiceIds = booking.BookingServices.Select(bs => bs.ServiceId).ToList();
            var availableServices = await _context.Services
                .Where(s => !registeredServiceIds.Contains(s.Id))
                .ToListAsync();
            // Đưa dữ liệu vào ViewBag để sử dụng trong View
            ViewBag.AvailableRooms = availableRooms;
            ViewBag.Services = availableServices;

            return View(booking);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int? id, Booking booking, int[] AdditionalRooms, int[] AdditionalServices)
        {
            var userIdClaim = User.FindFirst("UserId");
            int? userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId)
                ? parsedUserId
                : (int?)null;

            double total = booking.Total;
            var night = (booking.CheckOut.Value - booking.CheckIn.Value).Days;
            if (id != booking.Id)
            {
                return NotFound();
            }

            try
            {
                booking.UpdatedAt = DateTime.Now;
                booking.UpdatedByAccountId = userId;
                // Thêm các phòng bổ sung
                foreach (var roomId in AdditionalRooms)
                {
                    var room = _context.Rooms.FirstOrDefault(r => r.ID == roomId);
                    var bookingRoom = new BookingRoom
                    {
                        BookingId = booking.Id,
                        RoomId = roomId,
                        Quantity = 1,
                        Price = room.Price
                    };
                    _context.BookingRoom.Add(bookingRoom);
                    total += room.Price*night;
                }

                // Thêm các dịch vụ bổ sung
                foreach (var serviceId in AdditionalServices)
                {
                    var service = _context.Services.FirstOrDefault(r => r.Id == serviceId);
                    var bookingService = new BookingService
                    {
                        BookingId = booking.Id,
                        ServiceId = serviceId,
                        Price = service.Price
                    };
                    _context.BookingService.Add(bookingService);
                    total += service.Price;
                }
                // Cập nhật tổng tiền mới vào booking
                booking.Total = total;

                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("ListBooking");
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
                               br.Booking.CheckOut > checkInDate &&
                               br.Booking.Status != "Hủy đặt phòng")); 
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
                depositMoney = totals * 0.5,
                SelectedServices = selectedServices,
                UserId = userId
            };

            TempData["ReservationData"] = JsonConvert.SerializeObject(reservationData);

            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = totals * 0.5,
                CreatedDate = DateTime.Now,
                Description = $"{name} thanh toán tiền đặt cọc phòng",
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
                    AccountId = reservationData.UserId,
                    depositMoney = reservationData.depositMoney,
                    Payment = "Đã đặt cọc",
                    Status = "Chưa nhận phòng",
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
                            Quantity = quantity.Value,
                            Price = room.Price*quantity.Value,
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
                            ServiceId = service.Id,
                            Price = service.Price,
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
        public async Task<IActionResult> DeleteBooking(int id)
        {
            // Retrieve the booking
            var booking = _context.Booking.FirstOrDefault(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = "Hủy đặt phòng";
            booking.UpdatedAt = DateTime.Now;
            _context.Update(booking);
            await _context.SaveChangesAsync();
            // Gửi email hủy bỏ đặt phòng
            var body = $@"
            <div style='font-family: Arial, sans-serif; color: #333; line-height: 1.6;'>
                <h2 style='color: #E53935; text-align: center;'>Hủy bỏ đặt phòng tại Trường Sinh Resort</h2>
                <p>Xin chào <strong>{booking.Name}</strong>,</p>
                <p>Chúng tôi xin xác nhận rằng đơn đặt phòng của bạn tại Trường Sinh Resort đã được hủy bỏ thành công theo yêu cầu.</p>
                <p>Mã đơn của bạn là <strong>{booking.Id}</strong>. Nếu bạn cần đặt phòng lại hoặc có bất kỳ câu hỏi nào, xin vui lòng liên hệ với chúng tôi.</p>
                <p style='margin-top: 20px;'>Chúng tôi rất tiếc khi không thể đón tiếp bạn lần này và hy vọng sẽ có cơ hội phục vụ bạn trong tương lai.</p>
                <p style='color: #555; font-size: 14px;'>Trân trọng,<br>Đội ngũ Trường Sinh Resort</p>
                <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                <footer style='text-align: center; font-size: 12px; color: #777;'>
                    <p>Trường Sinh Resort | Email: support@truongsinhresort.com | SĐT: 0123 456 789</p>
                    <p>&copy; 2024 Trường Sinh Resort. Tất cả các quyền được bảo lưu.</p>
                </footer>
            </div>";

            await _emailService.SendEmailAsync(booking.Email, "Xác Nhận Hủy Đặt Phòng", body);

            return RedirectToAction("DeleteConfirmation", new { id = booking.Id });
        }

        public IActionResult DeleteConfirmation(int id)
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


        //public IActionResult Delete(int id)
        //{

        //    // Check if the booking can be canceled
        //    if (booking.CheckIn.HasValue && booking.CheckIn.Value > DateTime.Now)
        //    {
        //        // Retrieve related entities
        //        var bookingRooms = _context.BookingRoom.Where(r => r.BookingId == id).ToList();
        //        var bookingServices = _context.BookingService.Where(s => s.BookingId == id).ToList();

        //        // Remove related entities
        //        _context.BookingRoom.RemoveRange(bookingRooms);
        //        _context.BookingService.RemoveRange(bookingServices);

        //        // Remove the booking itself
        //        _context.Booking.Remove(booking);
        //        _context.SaveChanges();

        //        // Set success message
        //        TempData["SuccessMessage"] = "Đơn đặt phòng đã được hủy thành công.";
        //    }
        //    else
        //    {
        //        // Set error message
        //        TempData["ErrorMessage"] = "Không thể hủy đơn đặt phòng vì thời gian Check-In đã bắt đầu hoặc đã qua.";
        //    }

        //    // Redirect to a relevant page (e.g., booking list or home page)
        //    return RedirectToAction("Index", "Home");
        //}
        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.Id == id);
        }

    }
}

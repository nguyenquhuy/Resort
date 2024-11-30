using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class Booking
    {
        public int Id { get; set; }
        [DisplayName("Họ và tên")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string Name { get; set; }
        [DisplayName("Địa chỉ")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [DisplayName("Địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email address is not correct in form")]
        [MaxLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        public string Email { get; set; }
        [DisplayName("Số điện thoại")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại chỉ được chứa số và có độ dài từ 10 đến 11 ký tự")]
        public string Phone { get; set; }
        [DisplayName("Căn cước công dân")]
        [Required(ErrorMessage = "Vui lòng nhập số căn cước công dân")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Số điện thoại chỉ được chứa số và có độ 12 ký tự")]
        public string? CCCD { get; set; }
        [DisplayName("Ngày nhận phòng")]
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        public DateTime? CheckIn { get; set; }
        [DisplayName("Ngày trả phòng")]
        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        public DateTime? CheckOut { get; set; }
        [DisplayName("Ngày tạo hóa đơn")]
        public DateTime? CreatedAt { get; set; }
        [DisplayName("Trẻ em")]
        public int Child {  get; set; }
        [DisplayName("Người lớn")]
        public int Adult { get; set; }
        [DisplayName("Tổng tiền")]
        public double Total { get; set; }

        [ForeignKey("Account")]
        public int? AccountId { get; set; }
        public Account Account { get; set; }
        [DisplayName("Thanh toán")]
        public string? Payment { get; set; }
        [DisplayName("Tình trạng")]
        public string? Status { get; set; }
        [DisplayName("Ngày cập nhật đơn")]
        public DateTime? UpdatedAt { get; set; }
        [ForeignKey("UpdatedByAccount")]
        public int? UpdatedByAccountId { get; set; }
        [DisplayName("Người cập nhật")]
        public Account UpdatedByAccount { get; set; }

        public double? depositMoney { get; set; }        
        // Navigation property
        public ICollection<BookingRoom> BookingRooms { get; set; } = new List<BookingRoom>();
        public ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>();
    }

    public class BookingRoom
    {
        public int Id { get; set; }
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public Room Room { get; set; }        
        public int Quantity { get; set; }  
        public double? Price { get; set; }
    }

    public class BookingService
    {
        public int Id { get; set; }
        [ForeignKey("Booking")]
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service service { get; set; }
        public double? Price { get; set; }
    }
}

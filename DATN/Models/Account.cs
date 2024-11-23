using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
    public class Account
    {
     
        public int ID { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [DisplayName("Địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email address is not correct in form")]
        [MaxLength(100, ErrorMessage = "Tối đa 100 ký tự")]
        public string? Email { get; set; }
        [DisplayName("Họ và tên")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string? Name { get; set; }
        [DisplayName("Địa chỉ")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string? Address { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu phải tối thiểu có 8 ký tự")]
        [DisplayName("Mật khẩu")]
        public string? Password { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [MinLength(10, ErrorMessage = "Số điện thoại phải tối thiểu có 10 chữ số")]
        [DisplayName("Số điện thoại")]
        public string? PhoneNumber { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập căn cước công dân")]
        [MinLength(12, ErrorMessage = "Căn cước công dân phải tối thiểu có 12 chữ số")]
        [DisplayName("Căn cước công dân")]
        public string? CCCD { get; set; }
        [DisplayName("Trạng thái hoạt động")]
        public bool? Enabled { get; set; } = true;

        [DisplayName("Quyèn")]
        public string? Role {  get; set; }
        [DisplayName("Ảnh đại diện")]
        public string? Avatar { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

    }
}

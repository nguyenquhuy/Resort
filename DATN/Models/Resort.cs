using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
	public class Resort
	{
		public int Id { get; set; }
		[DisplayName("Tên khu nghỉ dưỡng")]
		[MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
		[Required(ErrorMessage = "Vui lòng nhập tên khu nghỉ dưỡng")]
		public string Name { get; set; }
		[DisplayName("Địa chỉ")]
		[MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
		[Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
		public string Address { get; set; }
		[DisplayName("Số điện thoại")]
		[MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
		[Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
		public string PhoneNumber { get; set; }
		[DisplayName("Vị trí")]
		public string FrameMap { get; set; }
		[DisplayName("Ảnh")]
		public string Image { get; set; }
		[DisplayName("Ảnh")]
		[NotMapped]
		public IFormFile IFormFile { get; set; }
		[DisplayName("Mô tả")]
		[Required(ErrorMessage = "Vui lòng nhập mô tả")]
		public string Description { get; set; }
		[DisplayName("Đường dây nóng")]
		[MaxLength(20, ErrorMessage = "Tối đa 20 ký tự")]
		[Required(ErrorMessage = "Vui lòng nhập đường dây nóng")]
		public string Hotline { get; set; }
		[DisplayName("Email")]
		[MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
		[Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
		public string Email { get; set; }
		public string FAX { get; set; }

	}
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class Service
    {
        public int Id { get; set; }
        [DisplayName("Tên dịch vụ")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ")]
        public string? Title { get; set; }
        [DisplayName("Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string? Description { get; set; }
        [DisplayName("Nội dung")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string? Content { get; set; }
        public string? Image { get; set; }
        [DisplayName("Giá dịch vụ")]
        [Required(ErrorMessage = "Vui lòng nhập giá dịch vụ")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá dịch vụ phải lớn hơn 0.")]
        public int Price { get; set; }
        public DateTime CreatedDate { get; set; }
        [NotMapped]
        [DisplayName("Thêm hình ảnh")]
        public IFormFile? ImageFile { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DATN.Models
{
    public class Article
    {
        public int Id { get; set; }
        [DisplayName("Tiêu đề")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài viết")]
        public string? Title { get; set; }
        [DisplayName("Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string? Description { get; set; }
        [DisplayName("Nội dung")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        public string? Content { get; set; }
        public string? Image {  get; set; }
        public DateTime CreatedDate { get; set;}
        [NotMapped]
        [DisplayName("Thêm hình ảnh")]
        public IFormFile? ImageFile { get; set; }
		public int? AccountId { get; set; }
		public Account Account { get; set; }
	}
}

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace DATN.Models
{
	public class Gallery
	{
		public int Id { get; set; }
		[DisplayName("Tên ảnh")]
		public string Title { get; set; }
		[DisplayName("Ảnh")]
		public string Image { get; set; }
		[DisplayName("Ảnh")]
		[NotMapped]
		public IFormFile IFormFile { get; set; }

	}
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DATN.Models
{
    public class Room
    {
        public int ID { get; set; }
        [DisplayName("Tên phòng")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tên phòng")]
        public string? Name { get; set; }
        [DisplayName("Loại phòng")]
        [Required(ErrorMessage = "Vui lòng chọn loại phòng")]
        [ForeignKey("RoomType")]
        public int RoomTypeId { get; set; }
        public RoomType RoomType { get; set; } // Navigation property for RoomType

        [DisplayName("Giá phòng")]
        [Required(ErrorMessage = "Vui lòng nhập giá phòng")]
        [Range(1, double.MaxValue, ErrorMessage = "Giá phòng phải lớn hơn 0.")]
        public int Price { get; set; }
        [DisplayName("Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        public string? Description { get; set; }
        [DisplayName("Ảnh đại diện")]
        [MaxLength(300, ErrorMessage = "Tối đa 300 ký tự")]
        [Required(ErrorMessage = "Vui lòng chọn ảnh đại diện")]
        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; } // Exclude from database

        [DisplayName("Trạng thái hiển thị")]
        public bool Status { get; set; }

        public ICollection<GalleryRooms> GalleryRooms { get; set; } = new List<GalleryRooms>(); // Initializing the list
    }

    public class GalleryRooms
    {
        public int ID { get; set; } // Primary key
        public string Image { get; set; }

        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public Room Room { get; set; } // Navigation property for Room
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DATN.Models
{
    public class RoomType
    {
        public int Id { get; set; }
        [DisplayName("Tên loại phòng")]
        [MaxLength(250, ErrorMessage = "Tối đa 250 ký tự")]
        [Required(ErrorMessage = "Vui lòng nhập tên loại phòng")]
        public string? Name { get; set; }
    }
}

using System.ComponentModel;

namespace DATN.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int RoomId { get; set; }
        public Room Room { get; set; }
        [DisplayName("Bình luận")]
        public string Content {  get; set; }

        public DateTime Created { get; set; }
        [DisplayName("Trạng thái")]
        public bool Status { get; set; }
    }
}

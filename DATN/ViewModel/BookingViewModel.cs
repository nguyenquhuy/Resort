using DATN.Models;

namespace DATN.ViewModel
{
    public class BookingViewModel
    {
        public Booking Booking { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Service> Services { get; set; }
        public List<int> SelectedRoomIds { get; set; } = new List<int>();
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
    }
}

using DATN.Models;

namespace DATN.ViewModel
{
    public class RoomGal
    {
        public List<IFormFile> Images { get; set; }
        public Room Room { get; set; }

        public List<GalleryRooms> RoomGalleries { get; set; }
        public List<int> SelectedAmenities { get; set; }
        //public List<string> BookedDates { get; set; }
    }
}

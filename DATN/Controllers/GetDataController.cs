using DATN.Models;
using DATN.Models.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    public class GetDataController : Controller
    {
		public static List<RoomType> GetRoomTypes(DATNDbContext context)
		{
			return context.RoomTypes.ToList();
		}

		public static List<Room> GetRoom(DATNDbContext context)
		{
            // Lấy danh sách các phòng từ cơ sở dữ liệu
            var rooms = context.Rooms.ToList();

            // Duyệt qua từng phòng và tính số lượng đánh giá
            foreach (var room in rooms)
            {
                // Đếm số lượng đánh giá cho mỗi phòng
                room.CommentCount = context.Comments.Count(c => c.RoomId == room.ID);
            }

            return rooms;
        }

		public static List<Service> GetService(DATNDbContext context)
		{
			return context.Services.ToList();
		}

        public static List<Article> GetArticle(DATNDbContext context)
        {
            return context.Articles.ToList();
        }

        public static Resort GetResort(DATNDbContext context)
        {
			return context.Resort.FirstOrDefault();
        }

        public static List<Comment> GetCommentsWithUser(DATNDbContext context, int roomId, int limit = 3)
        {
            return context.Comments
                .Include(c => c.Account) // To load the related Account for each Comment
                .Where(c => c.RoomId == roomId && c.Status==true)
                .OrderByDescending(c => c.Created)
                .Take(limit)
                .ToList();
        }

        public static List<Booking> GetBookingByUser(DATNDbContext context, int id)
        {
            return context.Booking
                          .Where(b => b.AccountId == id)
                          .Include(b => b.BookingRooms)
                              .ThenInclude(br => br.Room) // Include Room for each BookingRoom
                          .ToList();
        }

        public static List<Amenity> GetRoomAmenity(DATNDbContext context, int id)
        {
            return context.RoomAmenity
                          .Where(a => a.RoomId == id)
                          .Include(a => a.Amenity)  // Eager load the related Amenity
                          .Select(a => a.Amenity)   // Select only the Amenity objects
                          .ToList();
        }

        public static int CountCommentRoom(DATNDbContext context, int roomId)
        {
            // Đếm số lượng đánh giá cho phòng với RoomId = roomId
            return context.Comments.Count(c => c.RoomId == roomId);
        }


    }
}

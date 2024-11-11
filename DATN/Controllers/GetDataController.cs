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
			return context.Rooms.ToList();
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
                .Where(c => c.RoomId == roomId)
                .OrderByDescending(c => c.Created)
                .Take(limit)
                .ToList();
        }
    }
}

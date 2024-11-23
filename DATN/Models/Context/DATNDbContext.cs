using Microsoft.EntityFrameworkCore;

namespace DATN.Models.Context
{
    public class DATNDbContext : DbContext
    {
        public DATNDbContext()
        {
        }

        public DATNDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Account> Account { get; set; }
        public DbSet<ResetPasswordToken> resetPasswordTokens { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<GalleryRooms> GalleryRooms { get; set; }
        public DbSet<Resort> Resort { get; set; }
        public DbSet<Gallery> Gallery { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<BookingRoom> BookingRoom { get; set; }
        public DbSet<BookingService> BookingService { get; set; }
        public DbSet<Amenity> Amenity { get; set; }
        public DbSet<RoomAmenity> RoomAmenity { get;set; }
    }
}

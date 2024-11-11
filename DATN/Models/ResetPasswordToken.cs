using System;
namespace DATN.Models
{
    public class ResetPasswordToken
    {
        public int ID { get; set; }

        public string? Email { get; set; } // The email of the user requesting password reset

        public string? Token { get; set; } // The generated token

        public DateTime ExpiryTime { get; set; } // Expiry time for the token
    }
}

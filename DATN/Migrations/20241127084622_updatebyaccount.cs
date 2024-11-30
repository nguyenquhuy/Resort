using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class updatebyaccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpdatedByAccountId",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_UpdatedByAccountId",
                table: "Booking",
                column: "UpdatedByAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Account_UpdatedByAccountId",
                table: "Booking",
                column: "UpdatedByAccountId",
                principalTable: "Account",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Account_UpdatedByAccountId",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_UpdatedByAccountId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "UpdatedByAccountId",
                table: "Booking");
        }
    }
}

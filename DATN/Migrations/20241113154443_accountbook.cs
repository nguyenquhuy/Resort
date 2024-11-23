using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class accountbook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRoom_Account_AccountId",
                table: "BookingRoom");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingService_Account_AccountId",
                table: "BookingService");

            migrationBuilder.DropIndex(
                name: "IX_BookingService_AccountId",
                table: "BookingService");

            migrationBuilder.DropIndex(
                name: "IX_BookingRoom_AccountId",
                table: "BookingRoom");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "BookingService");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "BookingRoom");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Booking_AccountId",
                table: "Booking",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Account_AccountId",
                table: "Booking",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Account_AccountId",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_AccountId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Account");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "BookingService",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "BookingRoom",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BookingService_AccountId",
                table: "BookingService",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingRoom_AccountId",
                table: "BookingRoom",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRoom_Account_AccountId",
                table: "BookingRoom",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingService_Account_AccountId",
                table: "BookingService",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

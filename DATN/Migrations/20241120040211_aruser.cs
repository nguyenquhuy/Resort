using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DATN.Migrations
{
    /// <inheritdoc />
    public partial class aruser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_AccountId",
                table: "Articles",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Articles_Account_AccountId",
                table: "Articles",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articles_Account_AccountId",
                table: "Articles");

            migrationBuilder.DropIndex(
                name: "IX_Articles_AccountId",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Articles");
        }
    }
}

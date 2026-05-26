using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyanamiConnect.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTelegramId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TelegramId",
                table: "Users");
        }
    }
}

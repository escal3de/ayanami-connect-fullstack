using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyanamiConnect.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SubscriptionUpdateMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Plans",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Subscriptions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plans",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Subscriptions");
        }
    }
}

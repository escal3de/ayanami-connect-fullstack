using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AyanamiConnect.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Inbounds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PanelInboundId = table.Column<int>(type: "integer", nullable: false),
                    Remark = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ServerAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Port = table.Column<int>(type: "integer", maxLength: 6, nullable: false),
                    Protocol = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MaxClientsLimit = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inbounds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TelegramId = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    FirstName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LastName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LanguageCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PanelClients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Uuid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SubId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExpiryTime = table.Column<long>(type: "bigint", nullable: false),
                    TotalGB = table.Column<long>(type: "bigint", nullable: false),
                    LimitIp = table.Column<int>(type: "integer", nullable: false),
                    Flow = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    Reset = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanelClients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PanelClients_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    InboundId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Inbounds_InboundId",
                        column: x => x.InboundId,
                        principalTable: "Inbounds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PanelClients_UserId",
                table: "PanelClients",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_InboundId",
                table: "Subscriptions",
                column: "InboundId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PanelClients");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Inbounds");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

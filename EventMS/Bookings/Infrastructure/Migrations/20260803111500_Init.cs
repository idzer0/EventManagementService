using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "bookings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор брони"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор события, к которому относится бронь"),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "Текущий статус брони (1=Pending, 2=Confirmed, 3=Rejected)"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Дата и время создания брони"),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Дата и время обработки брони (подтверждение/отмена/истечение)"),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                },
                comment: "Таблица бронирований мест на события");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_Status_CreatedAt",
                schema: "public",
                table: "bookings",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bookings",
                schema: "public");
        }
    }
}

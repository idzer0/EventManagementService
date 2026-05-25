using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Название события"),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Описание события"),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время начала события"),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время окончания события"),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Общее количество мест"),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Количество доступных мест")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                },
                comment: "Таблица событий");

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Уникальный идентификатор брони"),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Идентификатор события, к которому относится бронь"),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1, comment: "Текущий статус брони (1=Pending, 2=Confirmed, 3=Rejected)"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Дата и время создания брони"),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Дата и время обработки брони (подтверждение/отмена/истечение)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Events_EventId",
                        column: x => x.EventId,
                        principalSchema: "public",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Таблица бронирований мест на события");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId",
                schema: "public",
                table: "Bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "public",
                table: "Bookings",
                columns: new[] { "Status", "CreatedAt" })
                .Annotation("Npgsql:IndexInclude", new[] { "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_EndAt",
                schema: "public",
                table: "Events",
                column: "EndAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartAt",
                schema: "public",
                table: "Events",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Title",
                schema: "public",
                table: "Events",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "public");
        }
    }
}

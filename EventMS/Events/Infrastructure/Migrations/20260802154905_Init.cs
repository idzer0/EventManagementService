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

            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
                
            migrationBuilder.CreateTable(
                name: "events",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Название события"),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "Описание события"),
                    StartAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время начала события"),
                    EndAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Дата и время окончания события"),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Общее количество мест"),
                    AvailableSeats = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Количество доступных мест"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_events", x => x.Id);
                },
                comment: "Таблица событий");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EndAt",
                schema: "public",
                table: "events",
                column: "EndAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_StartAt",
                schema: "public",
                table: "events",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Title",
                schema: "public",
                table: "events",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "events",
                schema: "public");
        }
    }
}

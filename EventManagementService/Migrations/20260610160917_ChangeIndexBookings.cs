using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagementService.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "public",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "public",
                table: "Bookings",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "public",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_CreatedAt",
                schema: "public",
                table: "Bookings",
                columns: new[] { "Status", "CreatedAt" })
                .Annotation("Npgsql:IndexInclude", new[] { "Id" });
        }
    }
}

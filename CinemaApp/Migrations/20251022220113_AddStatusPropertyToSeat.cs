using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusPropertyToSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Seats",
                type: "NUMBER(10)",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Seats");
        }
    }
}

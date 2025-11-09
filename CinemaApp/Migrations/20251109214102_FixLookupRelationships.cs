using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaApp.Migrations
{
    /// <inheritdoc />
    public partial class FixLookupRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_ProjectionTypes_ProjectionTypeId",
                table: "Screenings");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_SeatTypes_SeatTypeId",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_PersonType",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PersonTypeId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PersonTypeId",
                table: "Tickets",
                column: "PersonTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_ProjectionTypes",
                table: "Screenings",
                column: "ProjectionTypeId",
                principalTable: "ProjectionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_SeatTypes",
                table: "Seats",
                column: "SeatTypeId",
                principalTable: "SeatTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_PersonType",
                table: "Tickets",
                column: "PersonTypeId",
                principalTable: "PersonTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_ProjectionTypes",
                table: "Screenings");

            migrationBuilder.DropForeignKey(
                name: "FK_Seats_SeatTypes",
                table: "Seats");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_PersonType",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_PersonTypeId",
                table: "Tickets");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_PersonTypeId",
                table: "Tickets",
                column: "PersonTypeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_ProjectionTypes_ProjectionTypeId",
                table: "Screenings",
                column: "ProjectionTypeId",
                principalTable: "ProjectionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_SeatTypes_SeatTypeId",
                table: "Seats",
                column: "SeatTypeId",
                principalTable: "SeatTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_PersonType",
                table: "Tickets",
                column: "PersonTypeId",
                principalTable: "PersonTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

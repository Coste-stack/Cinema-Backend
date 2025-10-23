using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectionTypeEnumModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectionType",
                table: "Screenings",
                newName: "ProjectionTypeId");

            migrationBuilder.CreateTable(
                name: "ProjectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectionTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_ProjectionTypeId",
                table: "Screenings",
                column: "ProjectionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Screenings_ProjectionTypes_ProjectionTypeId",
                table: "Screenings",
                column: "ProjectionTypeId",
                principalTable: "ProjectionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Screenings_ProjectionTypes_ProjectionTypeId",
                table: "Screenings");

            migrationBuilder.DropTable(
                name: "ProjectionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Screenings_ProjectionTypeId",
                table: "Screenings");

            migrationBuilder.RenameColumn(
                name: "ProjectionTypeId",
                table: "Screenings",
                newName: "ProjectionType");
        }
    }
}

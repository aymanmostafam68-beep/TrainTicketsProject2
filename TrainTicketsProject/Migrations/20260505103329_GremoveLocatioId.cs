using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketsProject.Migrations
{
    /// <inheritdoc />
    public partial class GremoveLocatioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_generalSettings_stations_LocationId",
                table: "generalSettings");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "generalSettings",
                newName: "StationId");

            migrationBuilder.RenameIndex(
                name: "IX_generalSettings_LocationId",
                table: "generalSettings",
                newName: "IX_generalSettings_StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings");

            migrationBuilder.RenameColumn(
                name: "StationId",
                table: "generalSettings",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_generalSettings_StationId",
                table: "generalSettings",
                newName: "IX_generalSettings_LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_generalSettings_stations_LocationId",
                table: "generalSettings",
                column: "LocationId",
                principalTable: "stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

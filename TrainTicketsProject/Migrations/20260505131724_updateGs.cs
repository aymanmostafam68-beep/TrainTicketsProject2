using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTicketsProject.Migrations
{
    /// <inheritdoc />
    public partial class updateGs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings");

            migrationBuilder.DropColumn(
                name: "logoUrl",
                table: "generalSettings");

            migrationBuilder.AlterColumn<int>(
                name: "StationId",
                table: "generalSettings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings");

            migrationBuilder.AlterColumn<int>(
                name: "StationId",
                table: "generalSettings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "logoUrl",
                table: "generalSettings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_generalSettings_stations_StationId",
                table: "generalSettings",
                column: "StationId",
                principalTable: "stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

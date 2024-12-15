using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingLotAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTypesDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Types",
                table: "ParkingLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "parking",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Types",
                table: "ParkingLots",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "parking");
        }
    }
}

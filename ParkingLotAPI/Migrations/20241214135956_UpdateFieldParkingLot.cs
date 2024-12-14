using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingLotAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFieldParkingLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Place_id",
                table: "ParkingLots",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Place_id",
                table: "ParkingLots");
        }
    }
}

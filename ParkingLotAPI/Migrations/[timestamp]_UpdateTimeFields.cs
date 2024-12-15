using Microsoft.EntityFrameworkCore.Migrations;

namespace ParkingLotAPI.Migrations
{
    public partial class UpdateTimeFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OpeningTime",
                table: "ParkingLots",
                type: "time",
                nullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ClosingTime",
                table: "ParkingLots",
                type: "time",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OpeningTime",
                table: "ParkingLots",
                type: "time",
                nullable: false);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ClosingTime",
                table: "ParkingLots",
                type: "time",
                nullable: false);
        }
    }
} 
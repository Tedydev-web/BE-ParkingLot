using Microsoft.EntityFrameworkCore.Migrations;

public partial class UpdateNullableFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Cập nhật các trường cho phép null
        migrationBuilder.AlterColumn<int>(
            name: "TotalSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "AvailableSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "PricePerHour",
            table: "ParkingLots",
            type: "decimal(18,2)",
            nullable: true);

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
        // Rollback changes
        migrationBuilder.AlterColumn<int>(
            name: "TotalSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<int>(
            name: "AvailableSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<decimal>(
            name: "PricePerHour",
            table: "ParkingLots",
            type: "decimal(18,2)",
            nullable: false,
            defaultValue: 0m);

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
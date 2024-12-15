using Microsoft.EntityFrameworkCore.Migrations;

public partial class UpdateNullableFieldsAndDefaults : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Cập nhật các trường số cho phép null
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

        // Cập nhật giá trị mặc định cho Types
        migrationBuilder.AlterColumn<string>(
            name: "Types",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false,
            defaultValueSql: "'parking'");

        // Cập nhật dữ liệu hiện có
        migrationBuilder.Sql("UPDATE ParkingLots SET Types = 'parking' WHERE Types IS NULL OR Types = ''");

        // Cập nhật các trường text cho phép null
        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "ContactNumber",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback changes
        migrationBuilder.AlterColumn<int>(
            name: "TotalSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: false);

        migrationBuilder.AlterColumn<int>(
            name: "AvailableSpaces",
            table: "ParkingLots",
            type: "int",
            nullable: false);

        migrationBuilder.AlterColumn<decimal>(
            name: "PricePerHour",
            table: "ParkingLots",
            type: "decimal(18,2)",
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Types",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "Description",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false);

        migrationBuilder.AlterColumn<string>(
            name: "ContactNumber",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false);
    }
} 
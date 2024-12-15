using Microsoft.EntityFrameworkCore.Migrations;

public partial class UpdateTypesDefaultValue : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Đặt giá trị mặc định cho Types
        migrationBuilder.AlterColumn<string>(
            name: "Types",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "parking");

        // Cập nhật dữ liệu hiện có
        migrationBuilder.Sql(@"
            UPDATE ParkingLots 
            SET Types = 'parking' 
            WHERE Types IS NULL OR Types = ''");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Types",
            table: "ParkingLots",
            type: "nvarchar(max)",
            nullable: false);
    }
} 
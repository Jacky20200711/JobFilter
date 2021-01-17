using Microsoft.EntityFrameworkCore.Migrations;

namespace JobFilter.Data.Migrations
{
    public partial class DelUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FilterSetting");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FilterSetting",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

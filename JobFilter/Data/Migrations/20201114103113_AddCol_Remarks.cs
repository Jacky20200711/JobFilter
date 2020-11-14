using Microsoft.EntityFrameworkCore.Migrations;

namespace JobFilter.Data.Migrations
{
    public partial class AddCol_Remarks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "FilterSetting",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "FilterSetting");
        }
    }
}

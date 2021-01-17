using Microsoft.EntityFrameworkCore.Migrations;

namespace JobFilter.Data.Migrations
{
    public partial class ExtendIgnoreCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IgnoreCompany",
                table: "FilterSetting",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IgnoreCompany",
                table: "FilterSetting",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1500,
                oldNullable: true);
        }
    }
}

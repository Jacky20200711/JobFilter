using Microsoft.EntityFrameworkCore.Migrations;

namespace JobFilter.Data.Migrations
{
    public partial class extend_limitLenOfIgnoreCompany_to_3000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IgnoreCompany",
                table: "FilterSetting",
                maxLength: 3000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1500)",
                oldMaxLength: 1500,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IgnoreCompany",
                table: "FilterSetting",
                type: "nvarchar(1500)",
                maxLength: 1500,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 3000,
                oldNullable: true);
        }
    }
}

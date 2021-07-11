using Microsoft.EntityFrameworkCore.Migrations;

namespace OSnack.API.Migrations
{
    public partial class _007 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Messages");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustomer",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Communications",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCustomer",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Communications");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Messages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}

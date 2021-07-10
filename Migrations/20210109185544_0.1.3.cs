using Microsoft.EntityFrameworkCore.Migrations;

namespace OSnack.API.Migrations
{
    public partial class _013 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenUrlPath",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Communications");

            migrationBuilder.RenameColumn(
                name: "IsOpen",
                table: "Communications",
                newName: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Communications",
                newName: "IsOpen");

            migrationBuilder.AddColumn<string>(
                name: "TokenUrlPath",
                table: "EmailTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Communications",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

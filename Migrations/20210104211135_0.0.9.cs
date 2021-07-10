using Microsoft.EntityFrameworkCore.Migrations;

namespace OSnack.API.Migrations
{
    public partial class _009 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Newsletters",
                table: "Newsletters");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Newsletters");

            migrationBuilder.DropColumn(
                name: "Show",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Newsletters",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reply",
                table: "Comments",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Newsletters",
                table: "Newsletters",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Newsletters",
                table: "Newsletters");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Newsletters");

            migrationBuilder.DropColumn(
                name: "Reply",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Newsletters",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Newsletters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Show",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Newsletters",
                table: "Newsletters",
                column: "Email");
        }
    }
}

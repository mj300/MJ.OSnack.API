using Microsoft.EntityFrameworkCore.Migrations;

namespace OSnack.API.Migrations
{
    public partial class _002 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerVariablesForEmail");

            migrationBuilder.DropIndex(
                name: "IX_AppLogs_UserId",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "IsDefaultTemplate",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "Locked",
                table: "EmailTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailTemplates",
                type: "nvarchar(256)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "TemplateType",
                table: "EmailTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EmailTemplateServerClass",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<int>(type: "int", nullable: false),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplateServerClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailTemplateServerClass_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_UserId",
                table: "AppLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplateServerClass_EmailTemplateId",
                table: "EmailTemplateServerClass",
                column: "EmailTemplateId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailTemplateServerClass");

            migrationBuilder.DropIndex(
                name: "IX_AppLogs_UserId",
                table: "AppLogs");

            migrationBuilder.DropColumn(
                name: "TemplateType",
                table: "EmailTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EmailTemplates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultTemplate",
                table: "EmailTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "EmailTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ServerVariablesForEmail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailTemplateId = table.Column<int>(type: "int", nullable: true),
                    EnumValue = table.Column<int>(type: "int", nullable: false),
                    ReplacementValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerVariablesForEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerVariablesForEmail_EmailTemplates_EmailTemplateId",
                        column: x => x.EmailTemplateId,
                        principalTable: "EmailTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppLogs_UserId",
                table: "AppLogs",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ServerVariablesForEmail_EmailTemplateId",
                table: "ServerVariablesForEmail",
                column: "EmailTemplateId");
        }
    }
}

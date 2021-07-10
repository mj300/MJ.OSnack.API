using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OSnack.API.Migrations
{
    public partial class _004 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailTemplateServerClass_EmailTemplates_EmailTemplateId",
                table: "EmailTemplateServerClass");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Orders");

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Payments",
                type: "decimal(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingReference",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryOptionId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplateServerClass_EmailTemplates_EmailTemplateId",
                table: "EmailTemplateServerClass",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmailTemplateServerClass_EmailTemplates_EmailTemplateId",
                table: "EmailTemplateServerClass");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "Payments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ShippingReference",
                table: "Orders",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryOptionId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmailTemplateServerClass_EmailTemplates_EmailTemplateId",
                table: "EmailTemplateServerClass",
                column: "EmailTemplateId",
                principalTable: "EmailTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SentinelMES.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoIpToAuditLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                schema: "public",
                table: "systemauditlogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryName",
                schema: "public",
                table: "systemauditlogs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                schema: "public",
                table: "systemauditlogs");

            migrationBuilder.DropColumn(
                name: "CountryName",
                schema: "public",
                table: "systemauditlogs");
        }
    }
}

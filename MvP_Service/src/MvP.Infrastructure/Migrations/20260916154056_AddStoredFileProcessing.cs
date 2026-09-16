using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MvP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredFileProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "StoredFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjectETag",
                table: "StoredFiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "StoredFiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StoredFiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "ObjectETag",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "StoredFiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "StoredFiles");
        }
    }
}

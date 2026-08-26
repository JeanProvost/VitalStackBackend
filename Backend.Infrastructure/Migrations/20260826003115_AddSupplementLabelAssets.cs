using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplementLabelAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LabelAssetsFetchedAtUtc",
                table: "SupplementProducts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LabelPdfUrl",
                table: "SupplementProducts",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "SupplementProducts",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabelAssetsFetchedAtUtc",
                table: "SupplementProducts");

            migrationBuilder.DropColumn(
                name: "LabelPdfUrl",
                table: "SupplementProducts");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "SupplementProducts");
        }
    }
}

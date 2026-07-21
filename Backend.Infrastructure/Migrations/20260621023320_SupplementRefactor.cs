using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupplementRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStackEntries_Supplements_MasterSupplementId",
                table: "UserStackEntries");

            migrationBuilder.DropTable(
                name: "Supplements");

            migrationBuilder.DropIndex(
                name: "IX_UserStackEntries_MasterSupplementId",
                table: "UserStackEntries");

            migrationBuilder.DropColumn(
                name: "MasterSupplementId",
                table: "UserStackEntries");

            migrationBuilder.AddColumn<decimal>(
                name: "ServingMultiplier",
                table: "UserStackEntries",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SupplementProductId",
                table: "UserStackEntries",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SupplementIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CanonicalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aliases = table.Column<List<string>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementIngredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplementProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DsldId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    BrandName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Form = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Metadata = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplementProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductIngredients",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    IngredientId = table.Column<int>(type: "integer", nullable: false),
                    DosageAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DosageUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductIngredients", x => new { x.ProductId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_ProductIngredients_SupplementIngredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "SupplementIngredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductIngredients_SupplementProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "SupplementProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStackEntries_SupplementProductId",
                table: "UserStackEntries",
                column: "SupplementProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductIngredients_IngredientId",
                table: "ProductIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplementIngredients_CanonicalName",
                table: "SupplementIngredients",
                column: "CanonicalName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplementProducts_DsldId",
                table: "SupplementProducts",
                column: "DsldId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStackEntries_SupplementProducts_SupplementProductId",
                table: "UserStackEntries",
                column: "SupplementProductId",
                principalTable: "SupplementProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserStackEntries_SupplementProducts_SupplementProductId",
                table: "UserStackEntries");

            migrationBuilder.DropTable(
                name: "ProductIngredients");

            migrationBuilder.DropTable(
                name: "SupplementIngredients");

            migrationBuilder.DropTable(
                name: "SupplementProducts");

            migrationBuilder.DropIndex(
                name: "IX_UserStackEntries_SupplementProductId",
                table: "UserStackEntries");

            migrationBuilder.DropColumn(
                name: "ServingMultiplier",
                table: "UserStackEntries");

            migrationBuilder.DropColumn(
                name: "SupplementProductId",
                table: "UserStackEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "MasterSupplementId",
                table: "UserStackEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Supplements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    Brand = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DosageAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    DosageUnit = table.Column<string>(type: "text", nullable: false),
                    Form = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RequiresFood = table.Column<bool>(type: "boolean", nullable: true),
                    ScientificContext = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    TimeOfDay = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Supplements", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStackEntries_MasterSupplementId",
                table: "UserStackEntries",
                column: "MasterSupplementId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserStackEntries_Supplements_MasterSupplementId",
                table: "UserStackEntries",
                column: "MasterSupplementId",
                principalTable: "Supplements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

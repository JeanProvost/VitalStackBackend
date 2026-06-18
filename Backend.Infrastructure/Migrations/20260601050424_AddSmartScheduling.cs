using System;
using Backend.Core.Entities.UserStackEntries;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSmartScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserStackEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    MasterSupplementId = table.Column<Guid>(type: "uuid", nullable: true),
                    CustomName = table.Column<string>(type: "text", nullable: true),
                    Cusomization = table.Column<StackCustomization>(type: "jsonb", nullable: false),
                    IntendedTime = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ContextualInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStackEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStackEntries_Supplements_MasterSupplementId",
                        column: x => x.MasterSupplementId,
                        principalTable: "Supplements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntakeLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserStackEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TakenAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IntendedTime = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SupplementName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Dosage = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContextualInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntakeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntakeLogs_UserStackEntries_UserStackEntryId",
                        column: x => x.UserStackEntryId,
                        principalTable: "UserStackEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeLogs_UserId_TakenAtUtc",
                table: "IntakeLogs",
                columns: new[] { "UserId", "TakenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeLogs_UserId_UserStackEntryId_TakenAtUtc",
                table: "IntakeLogs",
                columns: new[] { "UserId", "UserStackEntryId", "TakenAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_IntakeLogs_UserStackEntryId",
                table: "IntakeLogs",
                column: "UserStackEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStackEntries_MasterSupplementId",
                table: "UserStackEntries",
                column: "MasterSupplementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStackEntries_UserId_IsActive_IntendedTime",
                table: "UserStackEntries",
                columns: new[] { "UserId", "IsActive", "IntendedTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntakeLogs");

            migrationBuilder.DropTable(
                name: "UserStackEntries");
        }
    }
}

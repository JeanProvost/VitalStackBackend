using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplementSearchTrigramIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_SupplementProducts_ProductName_trgm"
                ON "SupplementProducts" USING gin ("ProductName" gin_trgm_ops);
                """);
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_SupplementProducts_BrandName_trgm"
                ON "SupplementProducts" USING gin ("BrandName" gin_trgm_ops);
                """);
            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_SupplementIngredients_CanonicalName_trgm"
                ON "SupplementIngredients" USING gin ("CanonicalName" gin_trgm_ops);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_SupplementProducts_ProductName_trgm\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_SupplementProducts_BrandName_trgm\";");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_SupplementIngredients_CanonicalName_trgm\";");
        }
    }
}

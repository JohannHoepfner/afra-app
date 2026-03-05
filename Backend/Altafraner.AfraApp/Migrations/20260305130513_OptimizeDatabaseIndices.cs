using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeDatabaseIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_profunda_einschreibungen_is_fixed_betroffene_person_id",
                table: "profunda_einschreibungen",
                columns: new[] { "is_fixed", "betroffene_person_id" });

            migrationBuilder.CreateIndex(
                name: "ix_personen_rolle",
                table: "personen",
                column: "rolle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_profunda_einschreibungen_is_fixed_betroffene_person_id",
                table: "profunda_einschreibungen");

            migrationBuilder.DropIndex(
                name: "ix_personen_rolle",
                table: "personen");
        }
    }
}

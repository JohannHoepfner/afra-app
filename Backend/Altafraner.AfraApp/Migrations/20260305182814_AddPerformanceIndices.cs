using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Altafraner.AfraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_profundum_einwahl_zeitraeume_einwahl_start_einwahl_stop",
                table: "profundum_einwahl_zeitraeume",
                columns: new[] { "einwahl_start", "einwahl_stop" });

            migrationBuilder.CreateIndex(
                name: "ix_profunda_einschreibungen_is_fixed",
                table: "profunda_einschreibungen",
                column: "is_fixed");

            migrationBuilder.CreateIndex(
                name: "ix_personen_rolle",
                table: "personen",
                column: "rolle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_profundum_einwahl_zeitraeume_einwahl_start_einwahl_stop",
                table: "profundum_einwahl_zeitraeume");

            migrationBuilder.DropIndex(
                name: "ix_profunda_einschreibungen_is_fixed",
                table: "profunda_einschreibungen");

            migrationBuilder.DropIndex(
                name: "ix_personen_rolle",
                table: "personen");
        }
    }
}

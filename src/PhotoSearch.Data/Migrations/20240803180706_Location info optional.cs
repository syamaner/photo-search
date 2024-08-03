using Microsoft.EntityFrameworkCore.Migrations;
using PhotoSearch.Data.GeoJson;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    /// <inheritdoc />
    public partial class Locationinfooptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<FeatureCollection>(
                name: "LocationInformation",
                table: "Photos",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(FeatureCollection),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<FeatureCollection>(
                name: "LocationInformation",
                table: "Photos",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(FeatureCollection),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;
using PhotoSearch.Data.GeoJson;
using PhotoSearch.Data.Models;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedLocationInformation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<FeatureCollection>(
                name: "LocationInformation",
                table: "Photos",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationInformation",
                table: "Photos");
        }
    }
}

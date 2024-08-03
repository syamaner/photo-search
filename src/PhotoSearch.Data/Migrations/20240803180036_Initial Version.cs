using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PhotoSearch.Data.GeoJson;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    RelativePath = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ExactPath = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    PublicUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CaptureDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImportedDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    SizeKb = table.Column<long>(type: "bigint", nullable: false),
                    LocationInformation = table.Column<FeatureCollection>(type: "jsonb", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    PhotoSummaries = table.Column<string>(type: "jsonb", nullable: true),
                    Thumbnails = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.RelativePath);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Photos");
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using PhotoSearch.Data.Models;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    RelativePath = table.Column<string>(type: "text", nullable: false),
                    ExactPath = table.Column<string>(type: "text", nullable: false),
                    PublicUrl = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    CaptureDateUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ImportedDateUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    SizeKb = table.Column<long>(type: "bigint", nullable: false),
                    PhotoSummaries = table.Column<List<PhotoSummary>>(type: "jsonb", nullable: true),
                    Metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    Thumbnails = table.Column<Thumbnail>(type: "jsonb", nullable: true)
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PhotoSearch.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPhotoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Photos",
                newName: "Thumbnails");

            migrationBuilder.RenameColumn(
                name: "ImageSummaries",
                table: "Photos",
                newName: "PhotoSummaries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Thumbnails",
                table: "Photos",
                newName: "Thumbnail");

            migrationBuilder.RenameColumn(
                name: "PhotoSummaries",
                table: "Photos",
                newName: "ImageSummaries");
        }
    }
}

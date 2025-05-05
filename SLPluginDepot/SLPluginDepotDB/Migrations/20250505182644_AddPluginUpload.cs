using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SLPluginDepotDB.Migrations
{
    /// <inheritdoc />
    public partial class AddPluginUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundImageUrl",
                table: "Plugins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GitHubUrl",
                table: "Plugins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Plugins",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundImageUrl",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "GitHubUrl",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Plugins");
        }
    }
}

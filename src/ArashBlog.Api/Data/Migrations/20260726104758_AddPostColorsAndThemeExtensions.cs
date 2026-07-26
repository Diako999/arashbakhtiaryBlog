using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPostColorsAndThemeExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardStyle",
                table: "ThemeConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FontChoice",
                table: "ThemeConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeaderFooterStyle",
                table: "ThemeConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "Posts",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BgColor",
                table: "Posts",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TextColor",
                table: "Posts",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardStyle",
                table: "ThemeConfigs");

            migrationBuilder.DropColumn(
                name: "FontChoice",
                table: "ThemeConfigs");

            migrationBuilder.DropColumn(
                name: "HeaderFooterStyle",
                table: "ThemeConfigs");

            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "BgColor",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "TextColor",
                table: "Posts");
        }
    }
}

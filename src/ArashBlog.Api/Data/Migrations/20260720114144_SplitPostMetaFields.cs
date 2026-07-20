using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitPostMetaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MetaTitle",
                table: "Posts",
                newName: "MetaTitleFa");

            migrationBuilder.RenameColumn(
                name: "MetaDescription",
                table: "Posts",
                newName: "MetaDescriptionFa");

            migrationBuilder.AddColumn<string>(
                name: "MetaDescriptionCkb",
                table: "Posts",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MetaTitleCkb",
                table: "Posts",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MetaDescriptionCkb",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "MetaTitleCkb",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "MetaTitleFa",
                table: "Posts",
                newName: "MetaTitle");

            migrationBuilder.RenameColumn(
                name: "MetaDescriptionFa",
                table: "Posts",
                newName: "MetaDescription");
        }
    }
}

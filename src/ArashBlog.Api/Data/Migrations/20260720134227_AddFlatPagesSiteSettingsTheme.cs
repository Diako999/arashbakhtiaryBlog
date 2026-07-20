using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlatPagesSiteSettingsTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlatPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TitleFa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleCkb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BodyFa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyCkb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaTitleFa = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaTitleCkb = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaDescriptionFa = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MetaDescriptionCkb = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlatPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TelegramUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TwitterUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedinUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WhatsappUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ThemeConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    DefaultMode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlatPages_Slug",
                table: "FlatPages",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlatPages");

            migrationBuilder.DropTable(
                name: "SiteSettings");

            migrationBuilder.DropTable(
                name: "ThemeConfigs");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingPageSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingPageSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeroBadgeFa = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    HeroBadgeCkb = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    HeroSubtitleFa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HeroSubtitleCkb = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    HeroDescriptionFa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    HeroDescriptionCkb = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AboutRoleFa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AboutRoleCkb = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AboutBioFa = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    AboutBioCkb = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false),
                    AboutPhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AboutGithubUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AboutYoutubeUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageSettings");
        }
    }
}

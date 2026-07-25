using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLandingSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingSections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    HeadingFa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeadingCkb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubheadingFa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SubheadingCkb = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BodyFa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyCkb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryCtaTextFa = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PrimaryCtaTextCkb = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    PrimaryCtaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondaryCtaTextFa = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SecondaryCtaTextCkb = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SecondaryCtaUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LandingSections_Type",
                table: "LandingSections",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingSections");
        }
    }
}

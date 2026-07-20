using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ArashBlog.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOfferingsTestimonialsLeads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeadMagnets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleFa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleCkb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    DescriptionFa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionCkb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MetaTitleFa = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaTitleCkb = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaDescriptionFa = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MetaDescriptionCkb = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadMagnets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offerings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TitleFa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleCkb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    SummaryFa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SummaryCkb = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    BodyFa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BodyCkb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    MetaTitleFa = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaTitleCkb = table.Column<string>(type: "nvarchar(70)", maxLength: 70, nullable: false),
                    MetaDescriptionFa = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    MetaDescriptionCkb = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offerings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadMagnetId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsContacted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_LeadMagnets_LeadMagnetId",
                        column: x => x.LeadMagnetId,
                        principalTable: "LeadMagnets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndsAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Testimonials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    AuthorRoleFa = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AuthorRoleCkb = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    QuoteFa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuoteCkb = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OfferingId = table.Column<int>(type: "int", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Testimonials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Testimonials_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferingId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Offerings_OfferingId",
                        column: x => x.OfferingId,
                        principalTable: "Offerings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_OfferingId",
                table: "Enrollments",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_SessionId",
                table: "Enrollments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadMagnets_Slug",
                table: "LeadMagnets",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offerings_Slug",
                table: "Offerings",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OfferingId",
                table: "Sessions",
                column: "OfferingId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_LeadMagnetId",
                table: "Submissions",
                column: "LeadMagnetId");

            migrationBuilder.CreateIndex(
                name: "IX_Testimonials_OfferingId",
                table: "Testimonials",
                column: "OfferingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "Testimonials");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "LeadMagnets");

            migrationBuilder.DropTable(
                name: "Offerings");
        }
    }
}

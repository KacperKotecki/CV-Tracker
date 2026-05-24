using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class SkillsToRequiredSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Skills",
                table: "JobOffers");

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "JobOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "JobOffers");

            migrationBuilder.AddColumn<string>(
                name: "Skills",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);
        }
    }
}

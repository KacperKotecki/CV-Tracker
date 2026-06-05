using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new Level column with default 'Mid' before dropping old Proficiency
            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "UserTechnologies",
                type: "TEXT",
                nullable: false,
                defaultValue: "Mid");

            // Data migration: map integer Proficiency values to SkillLevel names
            migrationBuilder.Sql(@"
                UPDATE UserTechnologies SET Level = CASE
                    WHEN Proficiency = 1 THEN 'Beginner'
                    WHEN Proficiency = 2 THEN 'Junior'
                    WHEN Proficiency = 3 THEN 'Mid'
                    WHEN Proficiency = 4 THEN 'Senior'
                    WHEN Proficiency = 5 THEN 'Expert'
                    ELSE 'Mid'
                END;
            ");

            migrationBuilder.DropColumn(
                name: "Proficiency",
                table: "UserTechnologies");

            migrationBuilder.AddColumn<string>(
                name: "RequiredLevel",
                table: "JobOfferTechnologies",
                type: "TEXT",
                nullable: false,
                defaultValue: "Mid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "UserTechnologies");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "JobOfferTechnologies");

            migrationBuilder.AddColumn<int>(
                name: "Proficiency",
                table: "UserTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);
        }
    }
}

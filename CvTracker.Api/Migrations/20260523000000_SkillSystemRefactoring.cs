using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class SkillSystemRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Technologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technologies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TechnologyAliases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Alias = table.Column<string>(type: "TEXT", nullable: false),
                    TechnologyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologyAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologyAliases_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTechnologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TechnologyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Proficiency = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTechnologies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTechnologies_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JobOfferTechnologies",
                columns: table => new
                {
                    JobOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnologyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOfferTechnologies", x => new { x.JobOfferId, x.TechnologyId });
                    table.ForeignKey(
                        name: "FK_JobOfferTechnologies_JobOffers_JobOfferId",
                        column: x => x.JobOfferId,
                        principalTable: "JobOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobOfferTechnologies_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.DropTable(
                name: "UserSkills");

            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "JobOffers");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyAliases_TechnologyId",
                table: "TechnologyAliases",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTechnologies_TechnologyId",
                table: "UserTechnologies",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferTechnologies_TechnologyId",
                table: "JobOfferTechnologies",
                column: "TechnologyId");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IX_Technologies_Name_CI ON Technologies (Name COLLATE NOCASE);");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IX_TechnologyAliases_Alias_CI ON TechnologyAliases (Alias COLLATE NOCASE);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Technologies_Name_CI;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_TechnologyAliases_Alias_CI;");

            migrationBuilder.DropIndex(
                name: "IX_JobOfferTechnologies_TechnologyId",
                table: "JobOfferTechnologies");

            migrationBuilder.DropIndex(
                name: "IX_UserTechnologies_TechnologyId",
                table: "UserTechnologies");

            migrationBuilder.DropIndex(
                name: "IX_TechnologyAliases_TechnologyId",
                table: "TechnologyAliases");

            migrationBuilder.DropTable(
                name: "JobOfferTechnologies");

            migrationBuilder.DropTable(
                name: "UserTechnologies");

            migrationBuilder.DropTable(
                name: "TechnologyAliases");

            migrationBuilder.DropTable(
                name: "Technologies");

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "JobOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    Proficiency = table.Column<int>(type: "INTEGER", nullable: false),
                    SkillName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSkills", x => x.Id);
                });
        }
    }
}

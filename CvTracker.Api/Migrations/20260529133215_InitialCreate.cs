using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Position = table.Column<string>(type: "TEXT", nullable: false),
                    ContractType = table.Column<string>(type: "TEXT", nullable: false),
                    WorkLoad = table.Column<string>(type: "TEXT", nullable: false),
                    WorkMode = table.Column<string>(type: "TEXT", nullable: false),
                    CompanyName = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    SourceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    SalaryMin = table.Column<decimal>(type: "TEXT", nullable: true),
                    SalaryMax = table.Column<decimal>(type: "TEXT", nullable: true),
                    AppliedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    FollowUpDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RecruiterName = table.Column<string>(type: "TEXT", nullable: true),
                    RecruiterContact = table.Column<string>(type: "TEXT", nullable: true),
                    SentCvVersion = table.Column<string>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOffers", x => x.Id);
                });

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
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", nullable: true),
                    LinkedInUrl = table.Column<string>(type: "TEXT", nullable: true),
                    GitHubUrl = table.Column<string>(type: "TEXT", nullable: true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", nullable: true),
                    AvatarFileName = table.Column<string>(type: "TEXT", nullable: true),
                    ResumeFileName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobOfferNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    EventDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOfferNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOfferNotes_JobOffers_JobOfferId",
                        column: x => x.JobOfferId,
                        principalTable: "JobOffers",
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

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferNotes_JobOfferId",
                table: "JobOfferNotes",
                column: "JobOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferTechnologies_TechnologyId",
                table: "JobOfferTechnologies",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_Technologies_Name_CI",
                table: "Technologies",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyAliases_Alias_CI",
                table: "TechnologyAliases",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologyAliases_TechnologyId",
                table: "TechnologyAliases",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTechnologies_TechnologyId",
                table: "UserTechnologies",
                column: "TechnologyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobOfferNotes");

            migrationBuilder.DropTable(
                name: "JobOfferTechnologies");

            migrationBuilder.DropTable(
                name: "TechnologyAliases");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserTechnologies");

            migrationBuilder.DropTable(
                name: "JobOffers");

            migrationBuilder.DropTable(
                name: "Technologies");
        }
    }
}

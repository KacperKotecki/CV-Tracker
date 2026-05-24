using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSkillsToCanonicalTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Create canonical Skills table ──────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, collation: "NOCASE"),
                    Category = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skills_Name",
                table: "Skills",
                column: "Name",
                unique: true);

            // ── 2. DATA: Populate Skills from existing UserSkills rows ─────────────
            // Must happen BEFORE dropping SkillName/Category columns.
            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO "Skills" ("Name", "Category")
                SELECT DISTINCT "SkillName", "Category"
                FROM "UserSkills"
                WHERE "SkillName" IS NOT NULL AND "SkillName" != '';
                """);

            // ── 3. Add new FK columns to UserSkills ───────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "SkillId",
                table: "UserSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserSkills",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            // ── 4. DATA: Resolve SkillId FK in UserSkills ─────────────────────────
            // Set SkillId by case-insensitive name match; always set UserId = 1.
            migrationBuilder.Sql("""
                UPDATE "UserSkills"
                SET
                    "SkillId" = COALESCE(
                        (SELECT "Id" FROM "Skills"
                         WHERE "Name" = "UserSkills"."SkillName" COLLATE NOCASE
                         LIMIT 1),
                        0
                    ),
                    "UserId" = 1;
                """);

            // ── 5. Drop obsolete string columns from UserSkills ───────────────────
            migrationBuilder.DropColumn(
                name: "Category",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "SkillName",
                table: "UserSkills");

            // ── 6. Add MatchScore column to JobOffers ─────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "MatchScore",
                table: "JobOffers",
                type: "INTEGER",
                nullable: true);

            // ── 7. Create JobOfferSkills join table ───────────────────────────────
            migrationBuilder.CreateTable(
                name: "JobOfferSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobOfferId = table.Column<int>(type: "INTEGER", nullable: false),
                    SkillId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOfferSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobOfferSkills_JobOffers_JobOfferId",
                        column: x => x.JobOfferId,
                        principalTable: "JobOffers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobOfferSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── 8. DATA: Migrate JobOffer.RequiredSkills JSON → Skills + JobOfferSkills ──
            // Uses SQLite json_each() (available in SQLite 3.38+, shipped with .NET 10).
            // Inserts any skill names not yet in Skills, then creates the join rows.
            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO "Skills" ("Name", "Category")
                SELECT DISTINCT j.value, NULL
                FROM "JobOffers", json_each("JobOffers"."RequiredSkills") j
                WHERE j.value IS NOT NULL AND j.value != '';
                """);

            migrationBuilder.Sql("""
                INSERT INTO "JobOfferSkills" ("JobOfferId", "SkillId")
                SELECT jo."Id", s."Id"
                FROM "JobOffers" jo, json_each(jo."RequiredSkills") j
                JOIN "Skills" s ON s."Name" = j.value COLLATE NOCASE
                WHERE j.value IS NOT NULL AND j.value != '';
                """);

            // ── 9. Drop RequiredSkills JSON column from JobOffers ─────────────────
            migrationBuilder.DropColumn(
                name: "RequiredSkills",
                table: "JobOffers");

            // ── 10. Add remaining indexes and FK constraints ──────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_SkillId",
                table: "UserSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSkills_UserId",
                table: "UserSkills",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferSkills_JobOfferId",
                table: "JobOfferSkills",
                column: "JobOfferId");

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferSkills_SkillId",
                table: "JobOfferSkills",
                column: "SkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkills_Skills_SkillId",
                table: "UserSkills",
                column: "SkillId",
                principalTable: "Skills",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSkills_UserProfiles_UserId",
                table: "UserSkills",
                column: "UserId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSkills_Skills_SkillId",
                table: "UserSkills");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSkills_UserProfiles_UserId",
                table: "UserSkills");

            migrationBuilder.DropTable(
                name: "JobOfferSkills");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropIndex(
                name: "IX_UserSkills_SkillId",
                table: "UserSkills");

            migrationBuilder.DropIndex(
                name: "IX_UserSkills_UserId",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "SkillId",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserSkills");

            migrationBuilder.DropColumn(
                name: "MatchScore",
                table: "JobOffers");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "UserSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SkillName",
                table: "UserSkills",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RequiredSkills",
                table: "JobOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

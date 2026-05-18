using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CvTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentFieldsAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "JobOffers");

            migrationBuilder.RenameColumn(
                name: "WhatWeOffer",
                table: "JobOffers",
                newName: "SentCvVersion");

            migrationBuilder.RenameColumn(
                name: "OurRequirements",
                table: "JobOffers",
                newName: "SalaryMin");

            migrationBuilder.RenameColumn(
                name: "Benefits",
                table: "JobOffers",
                newName: "SalaryMax");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AppliedAt",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FollowUpDate",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecruiterContact",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecruiterName",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "JobOffers",
                type: "TEXT",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_JobOfferNotes_JobOfferId",
                table: "JobOfferNotes",
                column: "JobOfferId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobOfferNotes");

            migrationBuilder.DropColumn(
                name: "AppliedAt",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "RecruiterContact",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "RecruiterName",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "JobOffers");

            migrationBuilder.RenameColumn(
                name: "SentCvVersion",
                table: "JobOffers",
                newName: "WhatWeOffer");

            migrationBuilder.RenameColumn(
                name: "SalaryMin",
                table: "JobOffers",
                newName: "OurRequirements");

            migrationBuilder.RenameColumn(
                name: "SalaryMax",
                table: "JobOffers",
                newName: "Benefits");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "JobOffers",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

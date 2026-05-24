using CvTracker.Api.Models;
using CvTracker.Api.Services;
using CvTracker.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CvTracker.Api.Tests.Services;

public class JobOfferServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly JobOfferService _sut;

    public JobOfferServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new JobOfferService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllJobOffers_WhenOffersExist()
    {
        // Arrange
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 1));
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 2, position: "QA Engineer"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenNoOffersExist()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsJobOffer_WhenFound()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsJobOffer()
    {
        // Arrange
        var dto = TestBuilders.BuildJobOfferDto();

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Position.Should().Be(dto.Position);

        var stored = await _context.JobOffers.FindAsync(result.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_SetsFollowUpDate_WhenAppliedAtProvided_AndFollowUpDateOmitted()
    {
        // Arrange
        var appliedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var dto = TestBuilders.BuildJobOfferDto();
        dto.AppliedAt = appliedAt;
        dto.FollowUpDate = null;

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.FollowUpDate.Should().Be(appliedAt.AddDays(14));
    }

    [Fact]
    public async Task CreateAsync_DoesNotOverrideFollowUpDate_WhenExplicitlyProvided()
    {
        // Arrange
        var appliedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var explicitFollowUp = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var dto = TestBuilders.BuildJobOfferDto();
        dto.AppliedAt = appliedAt;
        dto.FollowUpDate = explicitFollowUp;

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.FollowUpDate.Should().Be(explicitFollowUp);
    }

    [Fact]
    public async Task CreateAsync_LeavesFollowUpDateNull_WhenAppliedAtIsNull()
    {
        // Arrange
        var dto = TestBuilders.BuildJobOfferDto();
        dto.AppliedAt = null;
        dto.FollowUpDate = null;

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.FollowUpDate.Should().BeNull();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_UpdatesJobOffer_AndReturnsTrue_WhenFound()
    {
        // Arrange
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 1));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto(position: "Senior Engineer");

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.Should().BeTrue();

        var updated = await _context.JobOffers.FindAsync(1);
        updated!.Position.Should().Be("Senior Engineer");
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var dto = TestBuilders.BuildJobOfferDto();

        // Act
        var result = await _sut.UpdateAsync(999, dto);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_SetsFollowUpDate_WhenAppliedAtProvided_AndFollowUpDateOmitted()
    {
        // Arrange
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 1));
        await _context.SaveChangesAsync();

        var appliedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var dto = TestBuilders.BuildJobOfferDto();
        dto.AppliedAt = appliedAt;
        dto.FollowUpDate = null;

        // Act
        var result = await _sut.UpdateAsync(1, dto);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.JobOffers.FindAsync(1);
        updated!.FollowUpDate.Should().Be(appliedAt.AddDays(14));
    }

    // ── UpdateStatusAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatus_AndReturnsTrue_WhenFound()
    {
        // Arrange
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(
            id: 1,
            status: ApplicationStatus.Draft));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.UpdateStatusAsync(1, ApplicationStatus.Applied);

        // Assert
        result.Should().BeTrue();

        var updated = await _context.JobOffers.FindAsync(1);
        updated!.Status.Should().Be(ApplicationStatus.Applied);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenNotFound()
    {
        // Act
        var result = await _sut.UpdateStatusAsync(999, ApplicationStatus.Applied);

        // Assert
        result.Should().BeFalse();
    }

    // ── GetNotesAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetNotesAsync_ReturnsNotes_OrderedByEventDateDescending()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        var earlier = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var later = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);

        _context.JobOfferNotes.Add(new JobOfferNote { JobOfferId = 1, EventDate = earlier, Content = "First" });
        _context.JobOfferNotes.Add(new JobOfferNote { JobOfferId = 1, EventDate = later, Content = "Second" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetNotesAsync(1);

        // Assert
        result.Should().NotBeNull();
        var list = result!.ToList();
        list.Should().HaveCount(2);
        list[0].EventDate.Should().Be(later);
        list[1].EventDate.Should().Be(earlier);
    }

    [Fact]
    public async Task GetNotesAsync_ReturnsEmpty_WhenNoNotes()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetNotesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNotesAsync_ReturnsNull_WhenOfferNotFound()
    {
        // Act
        var result = await _sut.GetNotesAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ── AddNoteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AddNoteAsync_CreatesAndReturnsNote_WhenOfferExists()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferNoteDto("My note");

        // Act
        var result = await _sut.AddNoteAsync(1, dto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().BeGreaterThan(0);
        result.Content.Should().Be("My note");
        result.JobOfferId.Should().Be(1);

        var stored = await _context.JobOfferNotes.FindAsync(result.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task AddNoteAsync_ReturnsNull_WhenOfferNotFound()
    {
        // Arrange
        var dto = TestBuilders.BuildJobOfferNoteDto();

        // Act
        var result = await _sut.AddNoteAsync(999, dto);

        // Assert
        result.Should().BeNull();
    }

    // ── DeleteNoteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteNoteAsync_ReturnsTrueAndDeletes_WhenFound()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferNoteDto();
        var note = await _sut.AddNoteAsync(1, dto);

        // Act
        var result = await _sut.DeleteNoteAsync(1, note!.Id);

        // Assert
        result.Should().BeTrue();
        var stored = await _context.JobOfferNotes.FindAsync(note.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task DeleteNoteAsync_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.DeleteNoteAsync(1, 999);

        // Assert
        result.Should().BeFalse();
    }

    // ── MatchScore ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ComputeMatchScore_ReturnsNull_WhenOfferHasNoRequiredSkills()
    {
        // Arrange — offer with no required skills
        var dto = TestBuilders.BuildJobOfferDto();

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.MatchScore.Should().BeNull();
    }

    [Fact]
    public async Task ComputeMatchScore_Returns100_WhenAllRequiredSkillsMatch()
    {
        // Arrange — seed two canonical skills and the user's profile skills
        var skill1 = TestBuilders.BuildSkill(1, "React", "Frontend");
        var skill2 = TestBuilders.BuildSkill(2, "TypeScript", "Frontend");
        _context.Skills.AddRange(skill1, skill2);
        _context.UserSkills.AddRange(
            TestBuilders.BuildUserSkill(skillId: 1, proficiency: 3),
            TestBuilders.BuildUserSkill(skillId: 2, proficiency: 4));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto();
        dto.RequiredSkills = [new SkillRefDto(1, "React"), new SkillRefDto(2, "TypeScript")];

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.MatchScore.Should().Be(100);
    }

    [Fact]
    public async Task ComputeMatchScore_Returns50_WhenHalfRequiredSkillsMatch()
    {
        // Arrange — user only has one of two required skills
        var skill1 = TestBuilders.BuildSkill(1, "React", "Frontend");
        var skill2 = TestBuilders.BuildSkill(2, "TypeScript", "Frontend");
        _context.Skills.AddRange(skill1, skill2);
        _context.UserSkills.Add(TestBuilders.BuildUserSkill(skillId: 1, proficiency: 3));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto();
        dto.RequiredSkills = [new SkillRefDto(1, "React"), new SkillRefDto(2, "TypeScript")];

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.MatchScore.Should().Be(50);
    }

    [Fact]
    public async Task ComputeMatchScore_IgnoresSkillGapProficiency_InUserSkills()
    {
        // Arrange — user has the skill but proficiency = 0 (skill gap)
        var skill = TestBuilders.BuildSkill(1, "React", "Frontend");
        _context.Skills.Add(skill);
        _context.UserSkills.Add(TestBuilders.BuildUserSkill(skillId: 1, proficiency: 0));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto();
        dto.RequiredSkills = [new SkillRefDto(1, "React")];

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert — skill gap (proficiency = 0) must not count as a match
        result.MatchScore.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_PersistsMatchScore()
    {
        // Arrange
        var skill = TestBuilders.BuildSkill(1, "React", "Frontend");
        _context.Skills.Add(skill);
        _context.UserSkills.Add(TestBuilders.BuildUserSkill(skillId: 1, proficiency: 3));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto();
        dto.RequiredSkills = [new SkillRefDto(1, "React")];

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert — score is returned and also persisted in the DB
        result.MatchScore.Should().Be(100);
        var stored = await _context.JobOffers.FindAsync(result.Id);
        stored!.MatchScore.Should().Be(100);
    }

    [Fact]
    public async Task RecomputeAllMatchScoresAsync_UpdatesAllOffers()
    {
        // Arrange — two offers requiring the same skill; user has no skills initially
        var skill = TestBuilders.BuildSkill(1, "React", "Frontend");
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        var dto1 = TestBuilders.BuildJobOfferDto(position: "Frontend Dev 1");
        dto1.RequiredSkills = [new SkillRefDto(1, "React")];
        var offer1 = await _sut.CreateAsync(dto1);
        offer1.MatchScore.Should().Be(0);  // no user skills yet

        var dto2 = TestBuilders.BuildJobOfferDto(position: "Frontend Dev 2");
        dto2.RequiredSkills = [new SkillRefDto(1, "React")];
        var offer2 = await _sut.CreateAsync(dto2);
        offer2.MatchScore.Should().Be(0);

        // Add user skill for SkillId = 1
        _context.UserSkills.Add(TestBuilders.BuildUserSkill(skillId: 1, proficiency: 4));
        await _context.SaveChangesAsync();

        // Act
        await _sut.RecomputeAllMatchScoresAsync();

        // Assert — both offers should now have MatchScore = 100
        var updated1 = await _context.JobOffers.FindAsync(offer1.Id);
        var updated2 = await _context.JobOffers.FindAsync(offer2.Id);
        updated1!.MatchScore.Should().Be(100);
        updated2!.MatchScore.Should().Be(100);
    }
}

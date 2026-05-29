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

    // ── Match score ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ComputeMatchScore_NoRequiredSkills_ReturnsNull()
    {
        // Arrange
        var tech = TestBuilders.BuildTechnology(id: 1);
        _context.Technologies.Add(tech);
        _context.UserTechnologies.Add(new UserTechnology { TechnologyId = 1, Proficiency = 3 });
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result!.MatchScore.Should().BeNull();
    }

    [Fact]
    public async Task ComputeMatchScore_AllSkillsMatch_Returns100()
    {
        // Arrange
        var tech = TestBuilders.BuildTechnology(id: 1);
        _context.Technologies.Add(tech);
        await _context.SaveChangesAsync();

        _context.UserTechnologies.Add(new UserTechnology { TechnologyId = tech.Id, Proficiency = 3 });
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(offer.Id);

        // Assert
        result!.MatchScore.Should().Be(100);
    }

    [Fact]
    public async Task ComputeMatchScore_NoSkillsMatch_Returns0()
    {
        // Arrange
        var tech1 = TestBuilders.BuildTechnology(id: 1, name: "C#");
        var tech2 = TestBuilders.BuildTechnology(id: 2, name: "Java");
        _context.Technologies.AddRange(tech1, tech2);
        await _context.SaveChangesAsync();

        // User knows C# but offer requires Java
        _context.UserTechnologies.Add(new UserTechnology { TechnologyId = tech1.Id, Proficiency = 3 });
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech2.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(offer.Id);

        // Assert
        result!.MatchScore.Should().Be(0);
    }

    [Fact]
    public async Task ComputeMatchScore_PartialMatch_ReturnsCorrectPercentage()
    {
        // Arrange
        var tech1 = TestBuilders.BuildTechnology(id: 1, name: "C#");
        var tech2 = TestBuilders.BuildTechnology(id: 2, name: "Java");
        _context.Technologies.AddRange(tech1, tech2);
        await _context.SaveChangesAsync();

        // User knows only C#; offer requires C# and Java
        _context.UserTechnologies.Add(new UserTechnology { TechnologyId = tech1.Id, Proficiency = 3 });
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech1.Id });
        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech2.Id });
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(offer.Id);

        // Assert
        result!.MatchScore.Should().Be(50);
    }

    [Fact]
    public async Task GetAllAsync_PopulatesRequiredSkillIds_FromJoinTable()
    {
        // Arrange
        var tech = TestBuilders.BuildTechnology(id: 1, name: "TypeScript");
        _context.Technologies.Add(tech);
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech.Id });
        await _context.SaveChangesAsync();

        // Act
        var results = await _sut.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().RequiredSkillIds.Should().Contain(tech.Id);
    }

    [Fact]
    public async Task CreateAsync_PersistsJobOfferTechnologyRows()
    {
        // Arrange
        var tech = TestBuilders.BuildTechnology(id: 1, name: "Go");
        _context.Technologies.Add(tech);
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto(requiredSkillIds: [tech.Id]);

        // Act
        var created = await _sut.CreateAsync(dto);

        // Assert
        var joinRows = await _context.JobOfferTechnologies
            .Where(jt => jt.JobOfferId == created.Id)
            .ToListAsync();
        joinRows.Should().HaveCount(1);
        joinRows[0].TechnologyId.Should().Be(tech.Id);
    }

    [Fact]
    public async Task UpdateAsync_ReplacesJobOfferTechnologyRows()
    {
        // Arrange
        var tech1 = TestBuilders.BuildTechnology(id: 1, name: "C#");
        var tech2 = TestBuilders.BuildTechnology(id: 2, name: "Java");
        _context.Technologies.AddRange(tech1, tech2);
        var offer = TestBuilders.BuildJobOffer(id: 1);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        _context.JobOfferTechnologies.Add(new JobOfferTechnology { JobOfferId = offer.Id, TechnologyId = tech1.Id });
        await _context.SaveChangesAsync();

        // Now update with tech2 only
        var dto = TestBuilders.BuildJobOfferDto(requiredSkillIds: [tech2.Id]);

        // Act
        await _sut.UpdateAsync(offer.Id, dto);

        // Assert
        var joinRows = await _context.JobOfferTechnologies
            .Where(jt => jt.JobOfferId == offer.Id)
            .ToListAsync();
        joinRows.Should().HaveCount(1);
        joinRows[0].TechnologyId.Should().Be(tech2.Id);
    }

    [Fact]
    public async Task CreateAsync_PopulatesRequiredSkillIdsAndNames_OnReturnedEntity()
    {
        // Arrange
        var tech = TestBuilders.BuildTechnology(id: 1, name: "Go");
        _context.Technologies.Add(tech);
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto(requiredSkillIds: [tech.Id]);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.RequiredSkillIds.Should().ContainSingle().Which.Should().Be(tech.Id);
        result.RequiredSkillNames.Should().ContainSingle().Which.Should().Be(tech.Name);
    }
}

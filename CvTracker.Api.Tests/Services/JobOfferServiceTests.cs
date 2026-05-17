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
        var company = TestBuilders.BuildCompany();
        _context.Companies.Add(company);
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 1, companyId: company.Id));
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 2, companyId: company.Id, position: "QA Engineer"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(o => o.Company != null);
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
        var company = TestBuilders.BuildCompany();
        _context.Companies.Add(company);
        var offer = TestBuilders.BuildJobOffer(id: 1, companyId: company.Id);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Company.Should().NotBeNull();
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
        var company = TestBuilders.BuildCompany();
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto(companyId: company.Id);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Position.Should().Be(dto.Position);
        result.CompanyId.Should().Be(company.Id);

        var stored = await _context.JobOffers.FindAsync(result.Id);
        stored.Should().NotBeNull();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_UpdatesJobOffer_AndReturnsTrue_WhenFound()
    {
        // Arrange
        var company = TestBuilders.BuildCompany();
        _context.Companies.Add(company);
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(id: 1, companyId: company.Id));
        await _context.SaveChangesAsync();

        var dto = TestBuilders.BuildJobOfferDto(companyId: company.Id, position: "Senior Engineer");

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

    // ── UpdateStatusAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_UpdatesStatus_AndReturnsTrue_WhenFound()
    {
        // Arrange
        var company = TestBuilders.BuildCompany();
        _context.Companies.Add(company);
        _context.JobOffers.Add(TestBuilders.BuildJobOffer(
            id: 1,
            companyId: company.Id,
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
}

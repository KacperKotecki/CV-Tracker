using CvTracker.Api.Services;
using CvTracker.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CvTracker.Api.Tests.Services;

public class CompanyServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly CompanyService _sut;

    public CompanyServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _sut = new CompanyService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllCompanies_WhenCompaniesExist()
    {
        // Arrange
        _context.Companies.Add(TestBuilders.BuildCompany(id: 1, name: "Acme"));
        _context.Companies.Add(TestBuilders.BuildCompany(id: 2, name: "Globex"));
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Select(c => c.CompanyName).Should().Contain(new[] { "Acme", "Globex" });
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyCollection_WhenNoCompaniesExist()
    {
        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    // ── CreateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsCompany()
    {
        // Arrange
        var dto = TestBuilders.BuildCreateCompanyDto(name: "TechCorp", address: "456 Elm St");

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CompanyName.Should().Be("TechCorp");
        result.CompanyAddress.Should().Be("456 Elm St");

        var stored = await _context.Companies.FindAsync(result.Id);
        stored.Should().NotBeNull();
    }
}

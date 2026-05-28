using CvTracker.Api.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CvTracker.Api.Tests.Services;

public class SkillSeedingServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private string? _tempSeedFile;

    public SkillSeedingServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
        if (_tempSeedFile is not null && File.Exists(_tempSeedFile))
            File.Delete(_tempSeedFile);
    }

    private string WriteSeedFile(string json)
    {
        _tempSeedFile = Path.GetTempFileName();
        File.WriteAllText(_tempSeedFile, json);
        return _tempSeedFile;
    }

    private SkillSeedingService BuildSut(string seedFilePath) =>
        new TestableSkillSeedingService(_context, NullLogger<SkillSeedingService>.Instance, seedFilePath);

    // ── Tests ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_EmptyDb_SeedsTechnologiesAndAliases()
    {
        // Arrange
        var json = """
            [
              {
                "canonicalName": "C#",
                "category": "Programming languages",
                "aliases": ["c#", "c sharp", "csharp"]
              },
              {
                "canonicalName": "Python",
                "category": "Programming languages",
                "aliases": ["python", "py"]
              }
            ]
            """;
        var seedFile = WriteSeedFile(json);
        var sut = BuildSut(seedFile);

        // Act
        await sut.SeedAsync();

        // Assert
        var techs = await _context.Technologies.ToListAsync();
        techs.Should().HaveCount(2);
        techs.Should().Contain(t => t.Name == "C#" && t.Category == "Programming languages");
        techs.Should().Contain(t => t.Name == "Python");

        var aliases = await _context.TechnologyAliases.ToListAsync();
        aliases.Should().HaveCount(5);
        aliases.Should().Contain(a => a.Alias == "c#");
        aliases.Should().Contain(a => a.Alias == "py");
    }

    [Fact]
    public async Task SeedAsync_AlreadySeeded_DoesNotDuplicate()
    {
        // Arrange — seed once
        var json = """
            [{"canonicalName": "Go", "category": "Programming languages", "aliases": ["go", "golang"]}]
            """;
        var seedFile = WriteSeedFile(json);
        var sut = BuildSut(seedFile);
        await sut.SeedAsync();

        var countAfterFirst = await _context.Technologies.CountAsync();

        // Act — seed again (idempotent)
        await sut.SeedAsync();

        // Assert
        var countAfterSecond = await _context.Technologies.CountAsync();
        countAfterSecond.Should().Be(countAfterFirst);
    }

    [Fact]
    public async Task SeedAsync_AliasesStoredLowercase()
    {
        // Arrange
        var json = """
            [{"canonicalName": "Java", "category": "Programming languages", "aliases": ["JAVA", "Java EE"]}]
            """;
        var seedFile = WriteSeedFile(json);
        var sut = BuildSut(seedFile);

        // Act
        await sut.SeedAsync();

        // Assert
        var aliases = await _context.TechnologyAliases.Select(a => a.Alias).ToListAsync();
        aliases.Should().AllSatisfy(a => a.Should().Be(a.ToLowerInvariant()));
        aliases.Should().Contain("java");
        aliases.Should().Contain("java ee");
    }

    // ── Helper subclass ────────────────────────────────────────────────────────

    private sealed class TestableSkillSeedingService : SkillSeedingService
    {
        private readonly string _seedFilePath;

        public TestableSkillSeedingService(
            AppDbContext context,
            Microsoft.Extensions.Logging.ILogger<SkillSeedingService> logger,
            string seedFilePath)
            : base(context, logger)
        {
            _seedFilePath = seedFilePath;
        }

        protected override string SeedFilePath => _seedFilePath;
    }
}

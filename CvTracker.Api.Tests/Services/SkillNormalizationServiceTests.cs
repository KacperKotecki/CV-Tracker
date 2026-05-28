using CvTracker.Api.Services;
using CvTracker.Api.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CvTracker.Api.Tests.Services;

public class SkillNormalizationServiceTests
{
    private static IServiceProvider BuildServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseInMemoryDatabase(dbName));
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Resolve_KnownAlias_ReturnsTechnologyId()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tech = TestBuilders.BuildTechnology(id: 1, name: "C#");
        db.Technologies.Add(tech);
        db.TechnologyAliases.Add(new TechnologyAlias { Alias = "c#", TechnologyId = 1 });
        await db.SaveChangesAsync();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);
        await sut.InitializeAsync();

        // Act
        var result = sut.Resolve("c#");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task Resolve_KnownAliasCaseInsensitive_ReturnsTechnologyId()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tech = TestBuilders.BuildTechnology(id: 1, name: "C#");
        db.Technologies.Add(tech);
        db.TechnologyAliases.Add(new TechnologyAlias { Alias = "c sharp", TechnologyId = 1 });
        await db.SaveChangesAsync();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);
        await sut.InitializeAsync();

        // Act
        var result = sut.Resolve("C Sharp");

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task Resolve_UnknownAlias_ReturnsNull()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);
        await sut.InitializeAsync();

        // Act
        var result = sut.Resolve("unknowntechxyz");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Resolve_EmptyString_ReturnsNull()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);
        await sut.InitializeAsync();

        // Act & Assert
        sut.Resolve("").Should().BeNull();
        sut.Resolve("   ").Should().BeNull();
    }

    [Fact]
    public async Task InitializeAsync_LoadsAliasesFromDb()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var tech = TestBuilders.BuildTechnology(id: 1, name: "Python");
        db.Technologies.Add(tech);
        db.TechnologyAliases.Add(new TechnologyAlias { Alias = "python", TechnologyId = 1 });
        db.TechnologyAliases.Add(new TechnologyAlias { Id = 2, Alias = "py", TechnologyId = 1 });
        await db.SaveChangesAsync();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);

        // Act
        await sut.InitializeAsync();

        // Assert
        sut.Resolve("python").Should().Be(1);
        sut.Resolve("py").Should().Be(1);
    }

    [Fact]
    public async Task InitializeAsync_EmptyDb_ReturnsNullForAnyAlias()
    {
        // Arrange
        var provider = BuildServiceProvider(Guid.NewGuid().ToString());
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var sut = new SkillNormalizationService(scopeFactory);

        // Act — should not throw
        await sut.InitializeAsync();

        // Assert
        sut.Resolve("anything").Should().BeNull();
    }
}

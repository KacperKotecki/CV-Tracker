using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CvTracker.Api.Services;

public class SkillSeedingService : ISkillSeedingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SkillSeedingService> _logger;

    public SkillSeedingService(AppDbContext context, ILogger<SkillSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected virtual string SeedFilePath =>
        Path.Combine(AppContext.BaseDirectory, "jobOfferSkills.json");

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _context.Technologies.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("Technologies table already seeded; skipping.");
            return;
        }

        var path = SeedFilePath;
        if (!File.Exists(path))
        {
            _logger.LogWarning("Seed file not found at {Path}; skipping technology seeding.", path);
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        var entries = JsonSerializer.Deserialize<List<SeedEntry>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (entries is null || entries.Count == 0)
        {
            _logger.LogWarning("Seed file is empty or could not be deserialized.");
            return;
        }

        foreach (var entry in entries)
        {
            var technology = new Technology
            {
                Name = entry.CanonicalName,
                Category = entry.Category,
            };
            _context.Technologies.Add(technology);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var alias in entry.Aliases)
            {
                _context.TechnologyAliases.Add(new TechnologyAlias
                {
                    Alias = alias.Trim().ToLowerInvariant(),
                    TechnologyId = technology.Id,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} technologies.", entries.Count);
    }

    private sealed record SeedEntry(string CanonicalName, string Category, List<string> Aliases);
}

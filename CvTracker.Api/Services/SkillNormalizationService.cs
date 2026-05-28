using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Services;

public class SkillNormalizationService : ISkillNormalizationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private volatile Dictionary<string, int> _aliasMap = [];

    public SkillNormalizationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public int? Resolve(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;
        return _aliasMap.TryGetValue(rawText.Trim().ToLowerInvariant(), out var id) ? id : null;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var aliases = await db.TechnologyAliases
            .Select(a => new { a.Alias, a.TechnologyId })
            .ToListAsync(cancellationToken);

        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var alias in aliases)
            map[alias.Alias.ToLowerInvariant()] = alias.TechnologyId;

        _aliasMap = map;
    }
}

using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Services;

/// <summary>
/// Singleton service that resolves raw technology text to Technology IDs.
/// The alias map is loaded once at startup via <see cref="InitializeAsync"/> and replaced
/// atomically on reload (the volatile field guarantees visibility across threads without locks).
/// </summary>
public class SkillNormalizationService : ISkillNormalizationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    // alias (lowercase) → TechnologyId
    private volatile Dictionary<string, int> _aliasMap = [];

    // Pre-compiled per-alias patterns for FindAllInText; rebuilt together with _aliasMap.
    private volatile IReadOnlyList<(Regex Pattern, int TechnologyId)> _aliasPatterns = [];

    public SkillNormalizationService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc/>
    public int? Resolve(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText)) return null;
        return _aliasMap.TryGetValue(rawText.Trim().ToLowerInvariant(), out var id) ? id : null;
    }

    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var aliases = await db.TechnologyAliases
            .Select(a => new { a.Alias, a.TechnologyId })
            .ToListAsync(cancellationToken);

        var map = new Dictionary<string, int>(StringComparer.Ordinal);
        var patterns = new List<(Regex, int)>(aliases.Count);

        foreach (var alias in aliases)
        {
            var lower = alias.Alias.ToLowerInvariant();
            map[lower] = alias.TechnologyId;
            patterns.Add((BuildAliasRegex(alias.Alias), alias.TechnologyId));
        }

        // Atomic replacement — readers always see a consistent pair.
        _aliasMap = map;
        _aliasPatterns = patterns;
    }

    /// <inheritdoc/>
    public IEnumerable<int> FindAllInText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var patterns = _aliasPatterns;
        var found = new HashSet<int>();

        foreach (var (pattern, technologyId) in patterns)
        {
            if (pattern.IsMatch(text))
                found.Add(technologyId);
        }

        return found;
    }

    /// <summary>
    /// Builds a whole-word/phrase regex for a given alias.
    /// Uses <c>\b</c> boundaries for purely alphanumeric aliases and non-alphanumeric
    /// character-class boundaries for aliases containing special characters (e.g., C#, C++, .NET).
    /// </summary>
    private static Regex BuildAliasRegex(string alias)
    {
        var escaped = Regex.Escape(alias);
        bool pureAlphanumericOrSpace = alias.All(c => char.IsLetterOrDigit(c) || c == ' ');

        var pattern = pureAlphanumericOrSpace
            ? $@"\b{escaped}\b"
            : $@"(?<![A-Za-z0-9]){escaped}(?![A-Za-z0-9])";

        return new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}

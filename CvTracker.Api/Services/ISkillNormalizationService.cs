namespace CvTracker.Api.Services;

public interface ISkillNormalizationService
{
    /// <summary>Resolves a single raw alias string to a Technology ID.</summary>
    int? Resolve(string rawText);

    /// <summary>Initialises the in-memory alias map from the database.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans <paramref name="text"/> for all known technology aliases using whole-word
    /// matching and returns the distinct Technology IDs found.
    /// </summary>
    IEnumerable<int> FindAllInText(string text);
}

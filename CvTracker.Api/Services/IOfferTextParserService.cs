namespace CvTracker.Api.Services;

/// <summary>
/// Parses raw job-offer plain text into a <see cref="ScrapedOfferDto"/> using
/// local regex/heuristic matching — no external API calls.
/// </summary>
public interface IOfferTextParserService
{
    /// <summary>
    /// Extracts structured offer fields from <paramref name="text"/>.
    /// Never throws; unrecognised fields are left <c>null</c>.
    /// </summary>
    ScrapedOfferDto Parse(string text);
}

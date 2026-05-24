namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Factory that selects the correct <see cref="IScraper"/> implementation
/// based on the job-offer URI host.
/// </summary>
public interface IScraperFactory
{
    /// <summary>
    /// Returns the most appropriate <see cref="IScraper"/> for the given <paramref name="uri"/>.
    /// Falls back to <see cref="FallbackScraper"/> for unknown hosts.
    /// </summary>
    IScraper GetScraper(Uri uri);
}

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Contract for a portal-specific (or generic fallback) web scraper.
/// Each implementation targets a specific job portal.
/// </summary>
public interface IScraper
{
    /// <summary>Scrapes the job offer at <paramref name="uri"/> and returns structured data.</summary>
    Task<ScrapeResultDto> ScrapeAsync(Uri uri);
}

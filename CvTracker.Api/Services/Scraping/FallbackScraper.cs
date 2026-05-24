using AngleSharp;
using Microsoft.Extensions.Logging;

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Generic fallback scraper for any job portal not handled by a dedicated scraper.
/// Fetches the page HTML and extracts Open Graph meta tags only
/// (<c>og:title</c>, <c>og:description</c>, <c>og:site_name</c>).
/// </summary>
public sealed class FallbackScraper : IScraper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<FallbackScraper> _logger;

    /// <summary>Initialises the scraper with a named <see cref="IHttpClientFactory"/>.</summary>
    public FallbackScraper(IHttpClientFactory httpClientFactory, ILogger<FallbackScraper> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    /// <inheritdoc />
    public async Task<ScrapeResultDto> ScrapeAsync(Uri uri)
    {
        string html;
        try
        {
            var client = _httpClientFactory.CreateClient("ScrapeClient");
            html = await client.GetStringAsync(uri);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FallbackScraper HTML fetch failed for {Uri}", uri);
            return Fail(ex.Message);
        }

        try
        {
            // Parse HTML using AngleSharp; extract OG meta tags only (no JS).
            var context  = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var ogTitle       = document.QuerySelector("meta[property='og:title']")
                                        ?.GetAttribute("content")?.Trim();
            var ogDescription = document.QuerySelector("meta[property='og:description']")
                                        ?.GetAttribute("content")?.Trim();
            var ogSiteName    = document.QuerySelector("meta[property='og:site_name']")
                                        ?.GetAttribute("content")?.Trim();

            // Use OG title as position and OG site_name as company if available.
            return new ScrapeResultDto(
                Position:        ogTitle,
                CompanyName:     ogSiteName,
                Location:        null,
                SalaryMin:       null,
                SalaryMax:       null,
                ContractType:    null,
                WorkMode:        null,
                WorkLoad:        null,
                RequiredSkills:  [],
                OurRequirements: ogDescription,
                WhatWeOffer:     null,
                Benefits:        null,
                ScrapeFailed:    false,
                ErrorMessage:    null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "FallbackScraper DOM parsing failed for {Uri}", uri);
            return Fail(ex.Message);
        }
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static ScrapeResultDto Fail(string message) =>
        new(null, null, null, null, null, null, null, null, [], null, null, null, true, message);
}

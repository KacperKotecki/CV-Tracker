namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Routes job-offer URLs to the correct portal-specific scraper.
/// Resolves all scrapers from the DI container to enable constructor injection
/// (e.g. named <see cref="System.Net.Http.HttpClient"/>s, loggers).
/// </summary>
public sealed class ScraperFactory : IScraperFactory
{
    private readonly JustJoinItScraper  _justJoinIt;
    private readonly NoFluffJobsScraper _noFluffJobs;
    private readonly PracujPlScraper    _pracujPl;
    private readonly FallbackScraper    _fallback;

    /// <summary>Initialises the factory with all available scrapers.</summary>
    public ScraperFactory(
        JustJoinItScraper  justJoinIt,
        NoFluffJobsScraper noFluffJobs,
        PracujPlScraper    pracujPl,
        FallbackScraper    fallback)
    {
        _justJoinIt  = justJoinIt;
        _noFluffJobs = noFluffJobs;
        _pracujPl    = pracujPl;
        _fallback    = fallback;
    }

    /// <inheritdoc />
    public IScraper GetScraper(Uri uri)
    {
        // Strip a leading "www." so both "justjoin.it" and "www.justjoin.it" match.
        var cleanHost = uri.Host.ToLowerInvariant();
        if (cleanHost.StartsWith("www.", StringComparison.Ordinal))
            cleanHost = cleanHost[4..];

        return cleanHost switch
        {
            "justjoin.it"     => _justJoinIt,
            "nofluffjobs.com" => _noFluffJobs,
            "pracuj.pl"       => _pracujPl,
            _                 => _fallback,
        };
    }
}

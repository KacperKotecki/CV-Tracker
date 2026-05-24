using CvTracker.Api.Services.Scraping;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace CvTracker.Api.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ScraperFactory"/> routing logic.
/// Verifies that each known host resolves to the correct scraper type.
/// </summary>
public class ScraperFactoryTests
{
    private readonly ScraperFactory _sut;

    public ScraperFactoryTests()
    {
        // Provide a no-op HttpClientFactory to all scrapers — these tests only
        // verify factory routing, not actual HTTP behaviour.
        var httpFactory = new Mock<IHttpClientFactory>();

        _sut = new ScraperFactory(
            new JustJoinItScraper(httpFactory.Object,  NullLogger<JustJoinItScraper>.Instance),
            new NoFluffJobsScraper(httpFactory.Object, NullLogger<NoFluffJobsScraper>.Instance),
            new PracujPlScraper(httpFactory.Object,    NullLogger<PracujPlScraper>.Instance),
            new FallbackScraper(httpFactory.Object,    NullLogger<FallbackScraper>.Instance)
        );
    }

    [Fact]
    public void GetScraper_JustJoinItHost_ReturnsJustJoinItScraper()
    {
        var uri = new Uri("https://justjoin.it/offers/company-senior-backend-developer");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<JustJoinItScraper>();
    }

    [Fact]
    public void GetScraper_JustJoinItHostWithWww_ReturnsJustJoinItScraper()
    {
        var uri = new Uri("https://www.justjoin.it/offers/company-position");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<JustJoinItScraper>();
    }

    [Fact]
    public void GetScraper_NoFluffJobsHost_ReturnsNoFluffJobsScraper()
    {
        var uri = new Uri("https://nofluffjobs.com/pl/job/company-position-city");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<NoFluffJobsScraper>();
    }

    [Fact]
    public void GetScraper_PracujPlHost_ReturnsPracujPlScraper()
    {
        var uri = new Uri("https://pracuj.pl/praca/senior-developer,oferta,123456");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<PracujPlScraper>();
    }

    [Fact]
    public void GetScraper_UnknownHost_ReturnsFallbackScraper()
    {
        var uri = new Uri("https://linkedin.com/jobs/view/12345");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<FallbackScraper>();
    }

    [Fact]
    public void GetScraper_AnotherUnknownHost_ReturnsFallbackScraper()
    {
        var uri = new Uri("https://example.com/job/42");

        var scraper = _sut.GetScraper(uri);

        scraper.Should().BeOfType<FallbackScraper>();
    }
}

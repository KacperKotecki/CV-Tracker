using CvTracker.Api.Controllers.Models.DTOs;
using CvTracker.Api.Services;
using CvTracker.Api.Services.Scraping;
using Microsoft.AspNetCore.Mvc;

namespace CvTracker.Api.Controllers;

/// <summary>
/// Handles asynchronous job-offer scraping.
/// <c>POST /api/scrape</c> immediately returns 202 Accepted with the new offer ID
/// and delegates actual scraping to a background <see cref="Task"/> using
/// <see cref="IServiceScopeFactory"/> so scoped services (EF Core context) are safe to use.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScrapeController : ControllerBase
{
    private readonly IJobOfferService    _jobOfferService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScrapeController> _logger;

    /// <summary>Initialises the controller with its required dependencies.</summary>
    public ScrapeController(
        IJobOfferService     jobOfferService,
        IServiceScopeFactory scopeFactory,
        ILogger<ScrapeController> logger)
    {
        _jobOfferService = jobOfferService;
        _scopeFactory    = scopeFactory;
        _logger          = logger;
    }

    /// <summary>
    /// Creates a <see cref="JobOffer"/> stub (status <c>ScrapingInProgress</c>) and fires a
    /// background scrape task. Returns 202 Accepted with the new offer ID immediately.
    /// </summary>
    /// <param name="request">Scrape request containing the job-offer URL.</param>
    /// <returns>202 Accepted — <see cref="ScrapeJobResponseDto"/> with the new offer ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ScrapeJobResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScrapeJobResponseDto>> ScrapeOffer([FromBody] ScrapeRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return ValidationProblem(new ValidationProblemDetails
            {
                Detail = "Nieprawidłowy URL. Podaj pełny adres z http:// lub https://.",
            });
        }

        // Persist the stub synchronously so we can return the ID to the client.
        var stub = await _jobOfferService.CreateScrapingStubAsync(request.Url);

        // Fire-and-forget background scrape using IServiceScopeFactory so that
        // scoped services (AppDbContext) can be resolved safely inside Task.Run.
        _ = Task.Run(() => RunBackgroundScrapeAsync(stub.Id, uri));

        return Accepted(new ScrapeJobResponseDto(stub.Id));
    }

    // -----------------------------------------------------------------
    // Private background method
    // -----------------------------------------------------------------

    /// <summary>
    /// Background scrape task. Resolves a DI scope, selects the correct scraper via
    /// <see cref="IScraperFactory"/>, and applies the result with
    /// <see cref="IJobOfferService.ApplyScrapedResultAsync"/>.
    /// All exceptions are caught and logged — unhandled exceptions in Task.Run
    /// are silently swallowed in .NET 6+.
    /// </summary>
    private async Task RunBackgroundScrapeAsync(int offerId, Uri uri)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var scraperFactory   = scope.ServiceProvider.GetRequiredService<IScraperFactory>();
        var jobOfferService  = scope.ServiceProvider.GetRequiredService<IJobOfferService>();
        var logger           = scope.ServiceProvider.GetRequiredService<ILogger<ScrapeController>>();

        ScrapeResultDto result;
        try
        {
            var scraper = scraperFactory.GetScraper(uri);
            result = await scraper.ScrapeAsync(uri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while scraping offer {OfferId} at {Uri}", offerId, uri);
            result = new ScrapeResultDto(
                null, null, null, null, null, null, null, null, [],
                null, null, null, true, ex.Message);
        }

        try
        {
            // Always apply — even on failure — to transition status away from ScrapingInProgress.
            await jobOfferService.ApplyScrapedResultAsync(offerId, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply scrape result for offer {OfferId}", offerId);
        }
    }
}


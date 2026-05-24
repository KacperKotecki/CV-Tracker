using AngleSharp;
using AngleSharp.Html.Dom;
using CvTracker.Api.Models;
using Microsoft.Extensions.Logging;

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Scrapes job offers from Pracuj.pl by fetching HTML and using AngleSharp
/// CSS selectors to extract structured data from the DOM.
/// CSS selectors may silently return null if Pracuj.pl changes their DOM —
/// <see cref="ScrapeResultDto.ScrapeFailed"/> is set to <see langword="true"/>
/// only if the HTTP fetch itself fails.
/// </summary>
public sealed class PracujPlScraper : IScraper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PracujPlScraper> _logger;

    /// <summary>Initialises the scraper with a named <see cref="IHttpClientFactory"/>.</summary>
    public PracujPlScraper(IHttpClientFactory httpClientFactory, ILogger<PracujPlScraper> logger)
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
            _logger.LogWarning(ex, "Pracuj.pl HTML fetch failed for {Uri}", uri);
            return Fail(ex.Message);
        }

        try
        {
            // Parse with AngleSharp (no JavaScript execution — DOM only).
            var context  = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Pracuj.pl renders position title in <h1> with data-test attribute.
            var position    = document.QuerySelector("[data-test='text-positionName']")?.TextContent?.Trim()
                           ?? document.QuerySelector("h1")?.TextContent?.Trim();

            var companyName = document.QuerySelector("[data-test='text-employerName']")?.TextContent?.Trim();
            var location    = document.QuerySelector("[data-test='text-workplaceAddress']")?.TextContent?.Trim();
            // F4: Try selectors in order until one returns a non-null value.
            // TODO: verify selector against live Pracuj.pl DOM
            var salaryRaw =
                document.QuerySelector("[data-test='text-earningAmount']")?.TextContent?.Trim()
             ?? document.QuerySelector("[data-test='earningsSectionSalary']")?.TextContent?.Trim()
             ?? document.QuerySelector(".salary-range")?.TextContent?.Trim()
             ?? document.QuerySelector("[data-test='job-offer-salary']")?.TextContent?.Trim();

            var contractRaw = document.QuerySelector("[data-test='text-typeOfContract']")?.TextContent?.Trim();
            var workModeRaw = document.QuerySelector("[data-test='text-workSchedule']")?.TextContent?.Trim();

            var (salaryMin, salaryMax, contractType) = SalaryParser.Parse(salaryRaw);
            if (contractType is null && contractRaw is not null)
            {
                contractType = MapContractType(contractRaw);
            }

            return new ScrapeResultDto(
                Position:       position,
                CompanyName:    companyName,
                Location:       location,
                SalaryMin:      salaryMin,
                SalaryMax:      salaryMax,
                ContractType:   contractType,
                WorkMode:       MapWorkMode(workModeRaw),
                WorkLoad:       null,
                RequiredSkills: ExtractSkills(document),
                OurRequirements: null,
                WhatWeOffer:    null,
                Benefits:       null,
                ScrapeFailed:   false,
                ErrorMessage:   null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pracuj.pl DOM parsing failed for {Uri}", uri);
            return Fail(ex.Message);
        }
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static ScrapeResultDto Fail(string message) =>
        new(null, null, null, null, null, null, null, null, [], null, null, null, true, message);

    /// <summary>
    /// Extracts required skills from the Pracuj.pl DOM.
    /// Tries selectors in order of likelihood; returns an empty list if none match.
    /// </summary>
    private static List<string> ExtractSkills(AngleSharp.Dom.IDocument document)
    {
        // Try most-specific selector first, then progressively broader fallbacks.
        string[] selectors =
        [
            "[data-test='chip-expected']",
            "[data-test='section-requirements'] li",
            ".chips-list li",
        ];

        foreach (var selector in selectors)
        {
            var nodes = document.QuerySelectorAll(selector);
            if (nodes.Length == 0)
                continue;

            var skills = nodes
                .Select(n => n.TextContent?.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Cast<string>()
                .ToList();

            if (skills.Count > 0)
                return skills;
        }

        return [];
    }

    private static ContractType? MapContractType(string raw)
    {
        var lower = raw.ToLowerInvariant();
        if (lower.Contains("b2b"))                 return ContractType.B2B;
        if (lower.Contains("o pracę"))             return ContractType.UoP;
        if (lower.Contains("zlecenie"))            return ContractType.MandateContract;
        if (lower.Contains("o dzieło"))            return ContractType.SpecificWorkContract;
        if (lower.Contains("staż"))                return ContractType.Internship;
        if (lower.Contains("praktyk"))             return ContractType.Apprenticeship;
        return null;
    }

    private static WorkMode? MapWorkMode(string? raw)
    {
        if (raw is null) return null;
        var lower = raw.ToLowerInvariant();
        if (lower.Contains("zdalna") || lower.Contains("remote")) return WorkMode.Remote;
        if (lower.Contains("hybrydow"))                           return WorkMode.Hybrid;
        if (lower.Contains("stacjonar"))                          return WorkMode.OnSite;
        return null;
    }
}

using AngleSharp;
using System.Text.Json;
using CvTracker.Api.Models;
using Microsoft.Extensions.Logging;

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Scrapes job offers from NoFluffJobs.com by fetching the public HTML page and
/// extracting the embedded <c>&lt;script type="application/ld+json"&gt;</c>
/// (schema.org <c>JobPosting</c>) block — no authentication required.
/// </summary>
public sealed class NoFluffJobsScraper : IScraper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NoFluffJobsScraper> _logger;

    /// <summary>Initialises the scraper with a named <see cref="IHttpClientFactory"/>.</summary>
    public NoFluffJobsScraper(IHttpClientFactory httpClientFactory, ILogger<NoFluffJobsScraper> logger)
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
            _logger.LogWarning(ex, "NoFluffJobs HTML fetch failed for {Uri}", uri);
            return Fail(ex.Message);
        }

        try
        {
            var context  = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            // Find the first JSON-LD block whose @type is "JobPosting".
            var jsonLdRaw = document
                .QuerySelectorAll("script[type='application/ld+json']")
                .Select(s => s.TextContent)
                .FirstOrDefault(text =>
                {
                    try
                    {
                        using var probe = JsonDocument.Parse(text);
                        return probe.RootElement.TryGetProperty("@type", out var t)
                               && t.GetString() == "JobPosting";
                    }
                    catch { return false; }
                });

            if (jsonLdRaw is null)
            {
                _logger.LogWarning("No JobPosting JSON-LD block found for {Uri}", uri);
                return Fail("No JobPosting JSON-LD found on NoFluffJobs page.");
            }

            using var doc = JsonDocument.Parse(jsonLdRaw);
            var root = doc.RootElement;

            var position    = GetString(root, "title");
            var companyName = root.TryGetProperty("hiringOrganization", out var org)
                              ? GetString(org, "name")
                              : null;
            var location    = root.TryGetProperty("jobLocation", out var jobLoc)
                              && jobLoc.TryGetProperty("address", out var addr)
                              ? GetString(addr, "addressLocality")
                              : null;
            var description = GetString(root, "description");

            var (salaryMin, salaryMax, contractType) = ExtractSalary(root, description);
            var requiredSkills = ExtractSkills(root);
            var workMode       = ExtractWorkMode(root);

            return new ScrapeResultDto(
                Position:        position,
                CompanyName:     companyName,
                Location:        location,
                SalaryMin:       salaryMin,
                SalaryMax:       salaryMax,
                ContractType:    contractType,
                WorkMode:        workMode,
                WorkLoad:        null,
                RequiredSkills:  requiredSkills,
                OurRequirements: null,
                WhatWeOffer:     null,
                Benefits:        null,
                ScrapeFailed:    false,
                ErrorMessage:    null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "NoFluffJobs JSON-LD parsing failed for {Uri}", uri);
            return Fail(ex.Message);
        }
    }

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    private static ScrapeResultDto Fail(string message) =>
        new(null, null, null, null, null, null, null, null, [], null, null, null, true, message);

    private static string? GetString(JsonElement el, string property) =>
        el.TryGetProperty(property, out var prop) && prop.ValueKind == JsonValueKind.String
        ? prop.GetString()
        : null;

    /// <summary>
    /// Reads salary from the schema.org <c>baseSalary</c> property
    /// (<c>MonetaryAmount</c> → <c>QuantitativeValue</c>).
    /// Falls back to <see cref="SalaryParser"/> on the description text.
    /// </summary>
    private static (decimal? min, decimal? max, ContractType? contractType) ExtractSalary(
        JsonElement root, string? description)
    {
        if (root.TryGetProperty("baseSalary", out var baseSalary))
        {
            decimal? minVal = null, maxVal = null;

            if (baseSalary.TryGetProperty("value", out var value))
            {
                if (value.ValueKind == JsonValueKind.Object)
                {
                    if (value.TryGetProperty("minValue", out var minProp) && minProp.TryGetDecimal(out var minDec))
                        minVal = minDec;
                    if (value.TryGetProperty("maxValue", out var maxProp) && maxProp.TryGetDecimal(out var maxDec))
                        maxVal = maxDec;
                }
                else if (value.TryGetDecimal(out var single))
                {
                    minVal = maxVal = single;
                }
            }

            if (minVal.HasValue || maxVal.HasValue)
                return (minVal, maxVal, null);
        }

        // Fall back to salary hints in description text.
        if (description is not null)
        {
            var (parsedMin, parsedMax, parsedCt) = SalaryParser.Parse(description);
            if (parsedMin.HasValue || parsedMax.HasValue)
                return (parsedMin, parsedMax, parsedCt);
        }

        return (null, null, null);
    }

    private static List<string> ExtractSkills(JsonElement root)
    {
        if (!root.TryGetProperty("skills", out var skills))
            return [];

        return skills.ValueKind switch
        {
            JsonValueKind.Array => skills.EnumerateArray()
                                         .Select(s => s.ValueKind == JsonValueKind.String ? s.GetString() : null)
                                         .Where(s => !string.IsNullOrWhiteSpace(s))
                                         .Cast<string>()
                                         .ToList(),
            JsonValueKind.String when !string.IsNullOrWhiteSpace(skills.GetString())
                                 => [skills.GetString()!],
            _                    => [],
        };
    }

    private static WorkMode? ExtractWorkMode(JsonElement root)
    {
        // schema.org uses "TELECOMMUTE" for remote jobs.
        if (root.TryGetProperty("jobLocationType", out var jlt)
            && jlt.ValueKind == JsonValueKind.String
            && jlt.GetString()?.Equals("TELECOMMUTE", StringComparison.OrdinalIgnoreCase) == true)
        {
            return WorkMode.Remote;
        }
        return null;
    }
}

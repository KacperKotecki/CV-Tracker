using System.ComponentModel.DataAnnotations;

/// <remarks>
/// This DTO was used by the old synchronous LLM-based scrape endpoint (200 OK).
/// It is now dead code — the endpoint returns <see cref="ScrapeJobResponseDto"/> (202 Accepted).
/// Kept here to avoid compilation errors in any code that may still reference it.
/// </remarks>
[Obsolete("ScrapedOfferDto is no longer returned by POST /api/scrape. Use ScrapeJobResponseDto instead.")]
public class ScrapedOfferDto
{
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public string? ContractType { get; set; }
    public string? WorkLoad { get; set; }
    public string? WorkMode { get; set; }
    public List<string> RequiredSkills { get; set; } = [];
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }
    public string? CompanyName { get; set; }
    public string? Location { get; set; }
}

public record ScrapeRequest([Required] string Url);

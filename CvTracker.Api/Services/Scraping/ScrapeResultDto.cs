using CvTracker.Api.Models;

namespace CvTracker.Api.Services.Scraping;

/// <summary>
/// Internal scraping-layer model — structured output produced by any scraper.
/// Not exposed as an HTTP API contract.
/// </summary>
public record ScrapeResultDto(
    string?       Position,
    string?       CompanyName,
    string?       Location,
    decimal?      SalaryMin,
    decimal?      SalaryMax,
    ContractType? ContractType,
    WorkMode?     WorkMode,
    WorkLoad?     WorkLoad,
    List<string>  RequiredSkills,
    string?       OurRequirements,
    string?       WhatWeOffer,
    string?       Benefits,
    bool          ScrapeFailed,
    string?       ErrorMessage
);

/// <summary>
/// DTO returned by POST /api/parse. Contains the parsed fields extracted from raw offer text.
/// SalaryMin and SalaryMax represent the monthly salary range in PLN (brutto).
/// </summary>
public class ScrapedOfferDto
{
    public string? Position { get; set; }

    /// <summary>Minimum monthly salary in PLN (brutto). Null if not found in text.</summary>
    public decimal? SalaryMin { get; set; }

    /// <summary>Maximum monthly salary in PLN (brutto). Null if not found in text.</summary>
    public decimal? SalaryMax { get; set; }

    public string? ContractType { get; set; }
    public string? WorkLoad { get; set; }
    public string? WorkMode { get; set; }
    public List<int> RequiredSkillIds { get; set; } = [];
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? CompanyName { get; set; }
    public string? Location { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class ScrapedOfferDto
{
    public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public string? ContractType { get; set; }
    public string? WorkLoad { get; set; }
    public string? WorkMode { get; set; }
    [JsonIgnore] public string? Skills { get; set; }
    public List<int> RequiredSkillIds { get; set; } = [];
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }
    public string? CompanyName { get; set; }
    public string? Location { get; set; }
}

public record ScrapeRequest([Required] string Url);

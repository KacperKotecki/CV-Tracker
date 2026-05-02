using CvTracker.Api.Models;

public class JobOffer
{
    public int Id { get; set; }
    public required string Position { get; set; }
    public decimal Salary { get; set; }

    public ContractType ContractType { get; set; }  // UoP / B2B / ...
    public WorkLoad WorkLoad { get; set; }           // FullTime / PartTime
    public WorkMode WorkMode { get; set; }           // Remote / OnSite / Hybrid
    public required int CompanyId { get; set; }
    public Company? Company { get; set; }

    public string? Skills { get; set; }
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }
}


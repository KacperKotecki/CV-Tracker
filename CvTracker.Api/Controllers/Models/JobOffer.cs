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

    public ICollection<SkillItem>? Skills { get; set; }
    

    public ICollection<string>? OurRequirements { get; set; }

    public ICollection<string>? WhatWeOffer { get; set; }

    public ICollection<string>? Benefits { get; set; }
}


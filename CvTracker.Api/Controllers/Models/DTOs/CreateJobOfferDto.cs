using CvTracker.Api.Models;

public class CreateJobOfferDto
{
    public required string Position { get; set; }
    public decimal Salary { get; set; }
    public ContractType ContractType { get; set; }
    public WorkLoad WorkLoad { get; set; }
    public WorkMode WorkMode { get; set; }
    public int CompanyId { get; set; }
    public string? Skills { get; set; }
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }
}
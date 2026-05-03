using CvTracker.Api.Models;
using System.ComponentModel.DataAnnotations;
public class JobOfferDto
{
    [Required]
    public required string Position { get; set; }
    [Required]
    public decimal Salary { get; set; }
    public ContractType ContractType { get; set; }
    public WorkLoad WorkLoad { get; set; }
    public WorkMode WorkMode { get; set; }

    public string? CompanyName { get; set; }
    public string? Location { get; set; }
    public string? Skills { get; set; }
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }

    public ApplicationStatus Status { get; set; }
}
using CvTracker.Api.Models;
using System.ComponentModel.DataAnnotations;
public class JobOfferDto
{
    [Required]
    public required string Position { get; set; }
    [Required]
    [Range(0, 100000, ErrorMessage = "Salary must be a non-negative number.")]
    public decimal Salary { get; set; }
    public ContractType ContractType { get; set; }
    public WorkLoad WorkLoad { get; set; }
    public WorkMode WorkMode { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public string? Skills { get; set; }
    public string? OurRequirements { get; set; }
    public string? WhatWeOffer { get; set; }
    public string? Benefits { get; set; }

    public Status Status { get; set; }
}
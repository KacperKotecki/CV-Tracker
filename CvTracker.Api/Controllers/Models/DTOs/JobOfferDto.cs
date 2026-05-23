using CvTracker.Api.Models;
using System.ComponentModel.DataAnnotations;
public class JobOfferDto
{
    [Required]
    public required string Position { get; set; }
    public ContractType ContractType { get; set; }
    public WorkLoad WorkLoad { get; set; }
    public WorkMode WorkMode { get; set; }

    public string? CompanyName { get; set; }
    public string? Location { get; set; }
    public List<string> RequiredSkills { get; set; } = [];

    [Url]
    public string? SourceUrl { get; set; }

    public ApplicationStatus Status { get; set; }

    public decimal? SalaryMin { get; set; }
    public decimal? SalaryMax { get; set; }
    public DateTimeOffset? AppliedAt { get; set; }
    public DateTimeOffset? FollowUpDate { get; set; }
    public string? RecruiterName { get; set; }
    public string? RecruiterContact { get; set; }
    public string? SentCvVersion { get; set; }
    public string? RejectionReason { get; set; }
}
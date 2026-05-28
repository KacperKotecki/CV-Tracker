using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CvTracker.Api.Models;

public class JobOffer
{
    public int Id { get; set; }
    public required string Position { get; set; }

    public ContractType ContractType { get; set; }  // UoP / B2B / ...
    public WorkLoad WorkLoad { get; set; }           // FullTime / PartTime
    public WorkMode WorkMode { get; set; }           // Remote / OnSite / Hybrid

    public string? CompanyName { get; set; }
    public string? Location { get; set; }

    [NotMapped] public int? MatchScore { get; set; }

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

    public ICollection<JobOfferNote> Notes { get; set; } = [];
    public ICollection<JobOfferTechnology> RequiredTechnologies { get; set; } = [];

    [NotMapped] public List<int> RequiredSkillIds { get; set; } = [];
    [NotMapped] public List<string> RequiredSkillNames { get; set; } = [];
}
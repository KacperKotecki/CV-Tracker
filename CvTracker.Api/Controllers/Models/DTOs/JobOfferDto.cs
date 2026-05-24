using CvTracker.Api.Models;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// DTO for creating or updating a job offer.
/// <c>RequiredSkills</c> uses <see cref="SkillRefDto"/> with <c>Id = 0</c> for find-or-create by name.
/// </summary>
public class JobOfferDto
{
    [Required]
    public required string Position { get; set; }
    public ContractType ContractType { get; set; }
    public WorkLoad WorkLoad { get; set; }
    public WorkMode WorkMode { get; set; }

    public string? CompanyName { get; set; }
    public string? Location { get; set; }

    /// <summary>Required skills. Use <c>Id = 0</c> to trigger find-or-create by name on the server.</summary>
    public List<SkillRefDto> RequiredSkills { get; set; } = [];

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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CvTracker.Api.Models;

/// <summary>
/// EF Core entity representing a job offer / application record.
/// Required skills are stored as <see cref="JobOfferSkill"/> join rows and projected
/// back as <see cref="RequiredSkills"/> for JSON serialization.
/// </summary>
public class JobOffer
{
    public int Id { get; set; }
    public required string Position { get; set; }

    public ContractType ContractType { get; set; }  // UoP / B2B / ...
    public WorkLoad WorkLoad { get; set; }           // FullTime / PartTime
    public WorkMode WorkMode { get; set; }           // Remote / OnSite / Hybrid

    public string? CompanyName { get; set; }
    public string? Location { get; set; }

    /// <summary>
    /// MatchScore as a persisted column, recomputed on write (create/update/scrape/skills-update).
    /// Null when the offer has no required skills.
    /// </summary>
    public int? MatchScore { get; set; }

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

    /// <summary>
    /// EF Core navigation — excluded from JSON serialization.
    /// Use <see cref="RequiredSkills"/> for the API response shape.
    /// </summary>
    [JsonIgnore]
    public ICollection<JobOfferSkill> JobOfferSkills { get; set; } = [];

    /// <summary>
    /// Computed property projecting <see cref="JobOfferSkills"/> into the API contract shape.
    /// Populated when the entity is loaded with ThenInclude(jos => jos.Skill).
    /// </summary>
    [NotMapped]
    public IReadOnlyList<SkillRefDto> RequiredSkills =>
        JobOfferSkills.Select(jos => new SkillRefDto(jos.SkillId, jos.Skill?.Name ?? string.Empty))
                      .ToList();
}
using CvTracker.Api.Models;

namespace CvTracker.Api.Tests.Helpers;

public static class TestBuilders
{
    public static JobOffer BuildJobOffer(
        int id = 1,
        string position = "Software Engineer",
        ApplicationStatus status = ApplicationStatus.Draft)
    {
        return new JobOffer
        {
            Id = id,
            Position = position,
            ContractType = ContractType.B2B,
            WorkLoad = WorkLoad.FullTime,
            WorkMode = WorkMode.Remote,
            Status = status
        };
    }

    public static JobOfferDto BuildJobOfferDto(
        string position = "Software Engineer",
        ApplicationStatus status = ApplicationStatus.Draft)
    {
        return new JobOfferDto
        {
            Position = position,
            ContractType = ContractType.B2B,
            WorkLoad = WorkLoad.FullTime,
            WorkMode = WorkMode.Remote,
            Status = status
        };
    }

    public static JobOfferNoteDto BuildJobOfferNoteDto(
        string content = "Test note",
        DateTimeOffset? eventDate = null)
    {
        return new JobOfferNoteDto
        {
            Content = content,
            EventDate = eventDate ?? DateTimeOffset.UtcNow
        };
    }

    /// <summary>Creates a canonical Skill entity for use in unit tests.</summary>
    public static Skill BuildSkill(int id, string name, string? category = null) =>
        new() { Id = id, Name = name, Category = category };

    /// <summary>Creates a UserSkill entity linked to a Skill for use in unit tests.</summary>
    public static UserSkill BuildUserSkill(int skillId, int proficiency = 3) =>
        new() { SkillId = skillId, UserId = 1, Proficiency = proficiency };
}


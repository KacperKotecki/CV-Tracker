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
        ApplicationStatus status = ApplicationStatus.Draft,
        List<int>? requiredSkillIds = null)
    {
        return new JobOfferDto
        {
            Position = position,
            ContractType = ContractType.B2B,
            WorkLoad = WorkLoad.FullTime,
            WorkMode = WorkMode.Remote,
            Status = status,
            RequiredSkillIds = requiredSkillIds ?? [],
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

    public static Technology BuildTechnology(
        int id = 1,
        string name = "C#",
        string category = "Programming languages")
    {
        return new Technology
        {
            Id = id,
            Name = name,
            Category = category,
        };
    }

    public static TechnologyAlias BuildTechnologyAlias(
        int id = 1,
        string alias = "c#",
        int technologyId = 1)
    {
        return new TechnologyAlias
        {
            Id = id,
            Alias = alias,
            TechnologyId = technologyId,
        };
    }
}

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
}

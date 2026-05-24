namespace CvTracker.Api.Models;

public enum ApplicationStatus
{
    /// <summary>Transient status — offer is being scraped in the background.</summary>
    ScrapingInProgress,
    Draft,
    Applied,
    HRScreening,
    TechnicalInterview,
    LiveCodingOrAssignment,
    AwaitingFeedback,

    Rejected,
    Accepted,

    Ghosted,
}
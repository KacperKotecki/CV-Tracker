namespace CvTracker.Api.Models;
public enum ApplicationStatus
{
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
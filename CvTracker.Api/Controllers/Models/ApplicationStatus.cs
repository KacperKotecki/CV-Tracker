namespace CvTracker.Api.Models;
public enum Status
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
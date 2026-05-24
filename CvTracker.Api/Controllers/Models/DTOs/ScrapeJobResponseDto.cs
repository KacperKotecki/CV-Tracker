namespace CvTracker.Api.Controllers.Models.DTOs;

/// <summary>
/// Response DTO returned by <c>POST /api/scrape</c>.
/// Contains the ID of the newly created <see cref="JobOffer"/> stub.
/// </summary>
public record ScrapeJobResponseDto(int Id);

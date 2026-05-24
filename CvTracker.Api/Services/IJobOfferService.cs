using CvTracker.Api.Models;
using CvTracker.Api.Services.Scraping;

namespace CvTracker.Api.Services;

public interface IJobOfferService
{
    Task<ICollection<JobOffer>> GetAllAsync();
    Task<JobOffer?> GetByIdAsync(int id);
    Task<JobOffer> CreateAsync(JobOfferDto dto);
    Task<bool> UpdateAsync(int id, JobOfferDto dto);
    Task<bool> UpdateStatusAsync(int id, ApplicationStatus status);
    Task<IEnumerable<JobOfferNote>?> GetNotesAsync(int offerId);
    Task<JobOfferNote?> AddNoteAsync(int offerId, JobOfferNoteDto dto);
    Task<bool> DeleteNoteAsync(int offerId, int noteId);

    /// <summary>
    /// Creates a placeholder <see cref="JobOffer"/> with status <see cref="ApplicationStatus.ScrapingInProgress"/>
    /// and stores the source URL. Returns the new offer so the caller can obtain its ID.
    /// </summary>
    Task<JobOffer> CreateScrapingStubAsync(string url);

    /// <summary>
    /// Applies the structured scrape result to an existing offer and transitions
    /// its status to <see cref="ApplicationStatus.Draft"/>.
    /// Always called — even when <see cref="ScrapeResultDto.ScrapeFailed"/> is true —
    /// to prevent records from being permanently stuck in <see cref="ApplicationStatus.ScrapingInProgress"/>.
    /// </summary>
    Task ApplyScrapedResultAsync(int id, ScrapeResultDto result);

    /// <summary>
    /// Recomputes <see cref="JobOffer.MatchScore"/> for every offer in the database
    /// using the current user skills (UserId = 1). Called after the user updates their skills.
    /// </summary>
    Task RecomputeAllMatchScoresAsync();
}


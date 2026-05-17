using CvTracker.Api.Models;

namespace CvTracker.Api.Services;

public interface IJobOfferService
{
    Task<ICollection<JobOffer>> GetAllAsync();
    Task<JobOffer?> GetByIdAsync(int id);
    Task<JobOffer> CreateAsync(JobOfferDto dto);
    Task<bool> UpdateAsync(int id, JobOfferDto dto);
    Task<bool> UpdateStatusAsync(int id, ApplicationStatus status);
}

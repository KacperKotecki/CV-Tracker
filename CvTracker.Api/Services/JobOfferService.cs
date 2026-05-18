using CvTracker.Api.Models;
using CvTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Services;

public class JobOfferService : IJobOfferService
{
    private readonly AppDbContext _context;

    public JobOfferService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<JobOffer>> GetAllAsync()
    {
        return await _context.JobOffers.ToListAsync();
    }

    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        return await _context.JobOffers.FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<JobOffer> CreateAsync(JobOfferDto dto)
    {
        var followUpDate = dto.FollowUpDate;
        if (followUpDate == null && dto.AppliedAt != null)
            followUpDate = dto.AppliedAt.Value.AddDays(14);

        var jobOffer = new JobOffer
        {
            Position = dto.Position,
            ContractType = dto.ContractType,
            WorkLoad = dto.WorkLoad,
            WorkMode = dto.WorkMode,
            CompanyName = dto.CompanyName,
            Location = dto.Location,
            Skills = dto.Skills,
            SourceUrl = dto.SourceUrl,
            Status = dto.Status,
            SalaryMin = dto.SalaryMin,
            SalaryMax = dto.SalaryMax,
            AppliedAt = dto.AppliedAt,
            FollowUpDate = followUpDate,
            RecruiterName = dto.RecruiterName,
            RecruiterContact = dto.RecruiterContact,
            SentCvVersion = dto.SentCvVersion,
            RejectionReason = dto.RejectionReason
        };

        _context.JobOffers.Add(jobOffer);
        await _context.SaveChangesAsync();
        return jobOffer;
    }

    public async Task<bool> UpdateAsync(int id, JobOfferDto dto)
    {
        var jobOffer = await GetByIdAsync(id);
        if (jobOffer == null) return false;

        var followUpDate = dto.FollowUpDate;
        if (followUpDate == null && dto.AppliedAt != null)
            followUpDate = dto.AppliedAt.Value.AddDays(14);

        jobOffer.Position = dto.Position;
        jobOffer.ContractType = dto.ContractType;
        jobOffer.WorkLoad = dto.WorkLoad;
        jobOffer.WorkMode = dto.WorkMode;
        jobOffer.CompanyName = dto.CompanyName;
        jobOffer.Location = dto.Location;
        jobOffer.Skills = dto.Skills;
        jobOffer.SourceUrl = dto.SourceUrl;
        jobOffer.Status = dto.Status;
        jobOffer.SalaryMin = dto.SalaryMin;
        jobOffer.SalaryMax = dto.SalaryMax;
        jobOffer.AppliedAt = dto.AppliedAt;
        jobOffer.FollowUpDate = followUpDate;
        jobOffer.RecruiterName = dto.RecruiterName;
        jobOffer.RecruiterContact = dto.RecruiterContact;
        jobOffer.SentCvVersion = dto.SentCvVersion;
        jobOffer.RejectionReason = dto.RejectionReason;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, ApplicationStatus status)
    {
        var jobOffer = await GetByIdAsync(id);
        if (jobOffer == null) return false;

        jobOffer.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<JobOfferNote>?> GetNotesAsync(int offerId)
    {
        var offerExists = await _context.JobOffers.AnyAsync(j => j.Id == offerId);
        if (!offerExists) return null;

        return await _context.JobOfferNotes
            .Where(n => n.JobOfferId == offerId)
            .OrderByDescending(n => n.EventDate)
            .ToListAsync();
    }

    public async Task<JobOfferNote?> AddNoteAsync(int offerId, JobOfferNoteDto dto)
    {
        var offerExists = await _context.JobOffers.AnyAsync(j => j.Id == offerId);
        if (!offerExists) return null;

        var note = new JobOfferNote
        {
            JobOfferId = offerId,
            EventDate = dto.EventDate,
            Content = dto.Content
        };

        _context.JobOfferNotes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    public async Task<bool> DeleteNoteAsync(int offerId, int noteId)
    {
        var note = await _context.JobOfferNotes
            .FirstOrDefaultAsync(n => n.Id == noteId && n.JobOfferId == offerId);
        if (note == null) return false;

        _context.JobOfferNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }
}

using CvTracker.Api.Models;
using CvTracker.Api.Services;
using CvTracker.Api.Services.Scraping;
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
        var offers = await _context.JobOffers
            .Include(j => j.Notes)
            .ToListAsync();
        foreach (var o in offers)
            o.MatchScore = await ComputeMatchScoreAsync(o.RequiredSkills);
        return offers;
    }

    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.Notes)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (jobOffer != null)
            jobOffer.MatchScore = await ComputeMatchScoreAsync(jobOffer.RequiredSkills);
        return jobOffer;
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
            RequiredSkills = dto.RequiredSkills,
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
        jobOffer.RequiredSkills = dto.RequiredSkills;
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
        if (note is null) return false;

        _context.JobOfferNotes.Remove(note);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<JobOffer> CreateScrapingStubAsync(string url)
    {
        // Create a minimal placeholder so the background scrape task has an ID to write to.
        var stub = new JobOffer
        {
            Position  = "…",  // placeholder — replaced by ApplyScrapedResultAsync
            Status    = ApplicationStatus.ScrapingInProgress,
            SourceUrl = url,
            AppliedAt = DateTimeOffset.UtcNow,
        };

        _context.JobOffers.Add(stub);
        await _context.SaveChangesAsync();
        return stub;
    }

    /// <inheritdoc />
    public async Task ApplyScrapedResultAsync(int id, ScrapeResultDto result)
    {
        var offer = await _context.JobOffers.FindAsync(id);
        if (offer is null) return;

        // Always transition to Draft — even on failure — to avoid stuck records.
        offer.Status = ApplicationStatus.Draft;

        if (!result.ScrapeFailed)
        {
            // Only overwrite fields that have a value; keep the placeholder position if scraper returned null.
            if (result.Position is not null)     offer.Position      = result.Position;
            if (result.CompanyName is not null)  offer.CompanyName   = result.CompanyName;
            if (result.Location is not null)     offer.Location      = result.Location;
            if (result.SalaryMin is not null)    offer.SalaryMin     = result.SalaryMin;
            if (result.SalaryMax is not null)    offer.SalaryMax     = result.SalaryMax;
            if (result.ContractType is not null) offer.ContractType  = result.ContractType.Value;
            if (result.WorkMode is not null)     offer.WorkMode      = result.WorkMode.Value;
            if (result.WorkLoad is not null)     offer.WorkLoad      = result.WorkLoad.Value;
            if (result.RequiredSkills.Count > 0)
            {
                var known = await GetKnownSkillsAsync();
                offer.RequiredSkills = result.RequiredSkills
                    .Where(s => known.Contains(s.ToLower()))
                    .ToList();
            }
        }

        // Use a sensible fallback position when the scrape produced nothing.
        if (offer.Position == "…")
        {
            offer.Position = offer.SourceUrl ?? "Unknown";
        }

        await _context.SaveChangesAsync();
    }

    private async Task<HashSet<string>> GetKnownSkillsAsync() =>
        (await _context.UserSkills
            .Select(s => s.SkillName.ToLower())
            .ToListAsync())
        .ToHashSet();

    private async Task<int?> ComputeMatchScoreAsync(List<string> requiredSkills)
    {
        if (requiredSkills.Count == 0) return null;
        var profileSkills = await GetKnownSkillsAsync();
        var matched = requiredSkills.Count(r => profileSkills.Contains(r.ToLower()));
        return (int)Math.Round((double)matched / requiredSkills.Count * 100);
    }
}

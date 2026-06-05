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
        var userSkills = await _context.UserTechnologies.ToListAsync();

        var offers = await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.RequiredTechnologies)
                .ThenInclude(t => t.Technology)
            .ToListAsync();

        foreach (var offer in offers)
        {
            offer.RequiredSkillIds = offer.RequiredTechnologies
                .Select(t => t.TechnologyId).ToList();
            offer.RequiredSkillNames = offer.RequiredTechnologies
                .Select(t => t.Technology.Name).ToList();
            offer.RequiredSkillLevels = offer.RequiredTechnologies
                .ToDictionary(t => t.TechnologyId, t => t.RequiredLevel);
            offer.MatchScore = ComputeMatchScore(offer, userSkills);
        }

        return offers;
    }

    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        var userSkills = await _context.UserTechnologies.ToListAsync();

        var jobOffer = await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.RequiredTechnologies)
                .ThenInclude(t => t.Technology)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (jobOffer != null)
        {
            jobOffer.RequiredSkillIds = jobOffer.RequiredTechnologies
                .Select(t => t.TechnologyId).ToList();
            jobOffer.RequiredSkillNames = jobOffer.RequiredTechnologies
                .Select(t => t.Technology.Name).ToList();
            jobOffer.RequiredSkillLevels = jobOffer.RequiredTechnologies
                .ToDictionary(t => t.TechnologyId, t => t.RequiredLevel);
            jobOffer.MatchScore = ComputeMatchScore(jobOffer, userSkills);
        }

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

        foreach (var skill in dto.RequiredSkills)
        {
            _context.JobOfferTechnologies.Add(new JobOfferTechnology
            {
                JobOfferId = jobOffer.Id,
                TechnologyId = skill.TechnologyId,
                RequiredLevel = skill.RequiredLevel,
            });
        }

        if (dto.RequiredSkills.Count > 0)
            await _context.SaveChangesAsync();

        // Populate [NotMapped] fields on the returned entity so the 201 response
        // includes RequiredSkillIds, RequiredSkillNames and RequiredSkillLevels.
        if (dto.RequiredSkills.Count > 0)
        {
            var techIds = dto.RequiredSkills.Select(s => s.TechnologyId).ToList();
            var technologies = await _context.Technologies
                .Where(t => techIds.Contains(t.Id))
                .ToListAsync();
            jobOffer.RequiredSkillIds = technologies.Select(t => t.Id).ToList();
            jobOffer.RequiredSkillNames = technologies.Select(t => t.Name).ToList();
            jobOffer.RequiredSkillLevels = dto.RequiredSkills
                .ToDictionary(s => s.TechnologyId, s => s.RequiredLevel);
        }

        return jobOffer;
    }

    public async Task<bool> UpdateAsync(int id, JobOfferDto dto)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.RequiredTechnologies)
            .FirstOrDefaultAsync(j => j.Id == id);

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

        _context.JobOfferTechnologies.RemoveRange(jobOffer.RequiredTechnologies);
        foreach (var skill in dto.RequiredSkills)
        {
            _context.JobOfferTechnologies.Add(new JobOfferTechnology
            {
                JobOfferId = jobOffer.Id,
                TechnologyId = skill.TechnologyId,
                RequiredLevel = skill.RequiredLevel,
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStatusAsync(int id, ApplicationStatus status)
    {
        var jobOffer = await _context.JobOffers.FirstOrDefaultAsync(j => j.Id == id);
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

    private static int ComputeMatchScore(JobOffer offer, IEnumerable<UserTechnology> userSkills)
    {
        var required = offer.RequiredTechnologies?.ToList();
        if (required == null || required.Count == 0) return 0;

        var userMap = userSkills.ToDictionary(s => s.TechnologyId, s => s.Level);
        double totalContribution = 0;

        foreach (var req in required)
        {
            if (!userMap.TryGetValue(req.TechnologyId, out var userLevel))
            {
                // Skill absent from profile — zero contribution
                continue;
            }

            double contribution;
            if (req.RequiredLevel == SkillLevel.Theory)
            {
                // Special case: Theory requirement is always met
                contribution = 1.0;
            }
            else
            {
                // Partial credit: min(user, required) / required
                contribution = Math.Min((double)userLevel, (double)req.RequiredLevel) / (double)req.RequiredLevel;
            }

            totalContribution += contribution;
        }

        return (int)Math.Round(totalContribution / required.Count * 100);
    }
}

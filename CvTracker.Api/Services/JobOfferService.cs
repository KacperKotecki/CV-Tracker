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

    /// <inheritdoc />
    public async Task<ICollection<JobOffer>> GetAllAsync()
    {
        // Load offers with all navigation properties required to populate RequiredSkills.
        return await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.JobOfferSkills)
                .ThenInclude(jos => jos.Skill)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<JobOffer?> GetByIdAsync(int id)
    {
        return await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.JobOfferSkills)
                .ThenInclude(jos => jos.Skill)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    /// <inheritdoc />
    public async Task<JobOffer> CreateAsync(JobOfferDto dto)
    {
        var followUpDate = dto.FollowUpDate;
        if (followUpDate is null && dto.AppliedAt is not null)
            followUpDate = dto.AppliedAt.Value.AddDays(14);

        var jobOffer = new JobOffer
        {
            Position        = dto.Position,
            ContractType    = dto.ContractType,
            WorkLoad        = dto.WorkLoad,
            WorkMode        = dto.WorkMode,
            CompanyName     = dto.CompanyName,
            Location        = dto.Location,
            SourceUrl       = dto.SourceUrl,
            Status          = dto.Status,
            SalaryMin       = dto.SalaryMin,
            SalaryMax       = dto.SalaryMax,
            AppliedAt       = dto.AppliedAt,
            FollowUpDate    = followUpDate,
            RecruiterName   = dto.RecruiterName,
            RecruiterContact = dto.RecruiterContact,
            SentCvVersion   = dto.SentCvVersion,
            RejectionReason = dto.RejectionReason,
        };

        // Resolve each requested skill to a canonical Skill row (find-or-create).
        foreach (var skillRef in dto.RequiredSkills)
        {
            var skill = await FindOrCreateSkillAsync(skillRef.Id, skillRef.Name, null);
            jobOffer.JobOfferSkills.Add(new JobOfferSkill { SkillId = skill.Id, Skill = skill });
        }

        // Compute and persist MatchScore before the first save.
        var userSkills = await _context.UserSkills.Where(s => s.UserId == 1).ToListAsync();
        jobOffer.MatchScore = ComputeMatchScore(jobOffer.JobOfferSkills, userSkills);

        _context.JobOffers.Add(jobOffer);
        await _context.SaveChangesAsync();

        // Reload with full navigation so RequiredSkills computed property has correct names.
        return await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.JobOfferSkills)
                .ThenInclude(jos => jos.Skill)
            .FirstAsync(j => j.Id == jobOffer.Id);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateAsync(int id, JobOfferDto dto)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.Notes)
            .Include(j => j.JobOfferSkills)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (jobOffer is null) return false;

        var followUpDate = dto.FollowUpDate;
        if (followUpDate is null && dto.AppliedAt is not null)
            followUpDate = dto.AppliedAt.Value.AddDays(14);

        jobOffer.Position        = dto.Position;
        jobOffer.ContractType    = dto.ContractType;
        jobOffer.WorkLoad        = dto.WorkLoad;
        jobOffer.WorkMode        = dto.WorkMode;
        jobOffer.CompanyName     = dto.CompanyName;
        jobOffer.Location        = dto.Location;
        jobOffer.SourceUrl       = dto.SourceUrl;
        jobOffer.Status          = dto.Status;
        jobOffer.SalaryMin       = dto.SalaryMin;
        jobOffer.SalaryMax       = dto.SalaryMax;
        jobOffer.AppliedAt       = dto.AppliedAt;
        jobOffer.FollowUpDate    = followUpDate;
        jobOffer.RecruiterName   = dto.RecruiterName;
        jobOffer.RecruiterContact = dto.RecruiterContact;
        jobOffer.SentCvVersion   = dto.SentCvVersion;
        jobOffer.RejectionReason = dto.RejectionReason;

        // Replace required skills: remove existing rows, then add updated ones.
        _context.JobOfferSkills.RemoveRange(jobOffer.JobOfferSkills);
        jobOffer.JobOfferSkills = [];

        foreach (var skillRef in dto.RequiredSkills)
        {
            var skill = await FindOrCreateSkillAsync(skillRef.Id, skillRef.Name, null);
            jobOffer.JobOfferSkills.Add(new JobOfferSkill { SkillId = skill.Id, Skill = skill });
        }

        // Recompute MatchScore for this offer only.
        var userSkills = await _context.UserSkills.Where(s => s.UserId == 1).ToListAsync();
        jobOffer.MatchScore = ComputeMatchScore(jobOffer.JobOfferSkills, userSkills);

        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateStatusAsync(int id, ApplicationStatus status)
    {
        var jobOffer = await GetByIdAsync(id);
        if (jobOffer is null) return false;

        jobOffer.Status = status;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<JobOfferNote>?> GetNotesAsync(int offerId)
    {
        var offerExists = await _context.JobOffers.AnyAsync(j => j.Id == offerId);
        if (!offerExists) return null;

        return await _context.JobOfferNotes
            .Where(n => n.JobOfferId == offerId)
            .OrderByDescending(n => n.EventDate)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<JobOfferNote?> AddNoteAsync(int offerId, JobOfferNoteDto dto)
    {
        var offerExists = await _context.JobOffers.AnyAsync(j => j.Id == offerId);
        if (!offerExists) return null;

        var note = new JobOfferNote
        {
            JobOfferId = offerId,
            EventDate  = dto.EventDate,
            Content    = dto.Content
        };

        _context.JobOfferNotes.Add(note);
        await _context.SaveChangesAsync();
        return note;
    }

    /// <inheritdoc />
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
        var offer = await _context.JobOffers
            .Include(j => j.JobOfferSkills)
            .FirstOrDefaultAsync(j => j.Id == id);

        if (offer is null) return;

        // Always transition to Draft — even on failure — to avoid stuck records.
        offer.Status = ApplicationStatus.Draft;

        if (!result.ScrapeFailed)
        {
            if (result.Position is not null)     offer.Position       = result.Position;
            if (result.CompanyName is not null)  offer.CompanyName    = result.CompanyName;
            if (result.Location is not null)     offer.Location       = result.Location;
            if (result.SalaryMin is not null)    offer.SalaryMin      = result.SalaryMin;
            if (result.SalaryMax is not null)    offer.SalaryMax      = result.SalaryMax;
            if (result.ContractType is not null) offer.ContractType   = result.ContractType.Value;
            if (result.WorkMode is not null)     offer.WorkMode       = result.WorkMode.Value;
            if (result.WorkLoad is not null)     offer.WorkLoad       = result.WorkLoad.Value;

            if (result.RequiredSkills.Count > 0)
            {
                // Store ALL scraped skills (no filtering by user profile).
                // MatchScore is computed below from the full intersection.
                _context.JobOfferSkills.RemoveRange(offer.JobOfferSkills);
                offer.JobOfferSkills = [];

                foreach (var skillName in result.RequiredSkills)
                {
                    if (string.IsNullOrWhiteSpace(skillName)) continue;
                    var skill = await FindOrCreateSkillAsync(0, skillName, null);
                    offer.JobOfferSkills.Add(new JobOfferSkill { SkillId = skill.Id, Skill = skill });
                }
            }
        }

        // Use a sensible fallback position when the scrape produced nothing.
        if (offer.Position == "…")
            offer.Position = offer.SourceUrl ?? "Unknown";

        // Recompute MatchScore with the freshly resolved skills.
        var userSkills = await _context.UserSkills.Where(s => s.UserId == 1).ToListAsync();
        offer.MatchScore = ComputeMatchScore(offer.JobOfferSkills, userSkills);

        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RecomputeAllMatchScoresAsync()
    {
        // Load user skills once — avoids N+1 query pattern.
        var userSkills = await _context.UserSkills
            .Where(s => s.UserId == 1)
            .ToListAsync();

        var offers = await _context.JobOffers
            .Include(j => j.JobOfferSkills)
            .ToListAsync();

        foreach (var offer in offers)
            offer.MatchScore = ComputeMatchScore(offer.JobOfferSkills, userSkills);

        await _context.SaveChangesAsync();
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the percentage of required skills the user possesses (proficiency ≥ 1).
    /// Returns <c>null</c> when the offer has no required skills.
    /// </summary>
    private static int? ComputeMatchScore(
        ICollection<JobOfferSkill> requiredSkills,
        ICollection<UserSkill> userSkills)
    {
        if (requiredSkills.Count == 0) return null;

        // Skills with Proficiency = 0 are "skill gaps" — not counted as a match.
        var userSkillIds = userSkills
            .Where(s => s.Proficiency > 0)
            .Select(s => s.SkillId)
            .ToHashSet();

        var matched = requiredSkills.Count(r => userSkillIds.Contains(r.SkillId));
        return (int)Math.Round((double)matched / requiredSkills.Count * 100);
    }

    /// <summary>
    /// Finds an existing <see cref="Skill"/> by ID or name (case-insensitive),
    /// or creates a new one if neither lookup succeeds.
    /// </summary>
    private async Task<Skill> FindOrCreateSkillAsync(int skillId, string name, string? category)
    {
        // Prefer lookup by ID when the caller already knows the canonical ID.
        if (skillId > 0)
        {
            var byId = await _context.Skills.FindAsync(skillId);
            if (byId is not null) return byId;
        }

        // Case-insensitive name lookup — relies on NOCASE collation at DB level as well.
        var trimmedName = name.Trim();
        var existing = await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == trimmedName.ToLower());

        if (existing is not null) return existing;

        // Create a new canonical skill row.
        var skill = new Skill { Name = trimmedName, Category = category };
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return skill;
    }
}


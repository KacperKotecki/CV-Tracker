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
        var jobOffer = new JobOffer
        {
            Position = dto.Position,
            Salary = dto.Salary,
            ContractType = dto.ContractType,
            WorkLoad = dto.WorkLoad,
            WorkMode = dto.WorkMode,
            CompanyName = dto.CompanyName,
            Location = dto.Location,
            Skills = dto.Skills,
            OurRequirements = dto.OurRequirements,
            WhatWeOffer = dto.WhatWeOffer,
            Benefits = dto.Benefits,
            SourceUrl = dto.SourceUrl,
            Status = dto.Status
        };

        _context.JobOffers.Add(jobOffer);
        await _context.SaveChangesAsync();
        return jobOffer;
    }

    public async Task<bool> UpdateAsync(int id, JobOfferDto dto)
    {
        var jobOffer = await GetByIdAsync(id);
        if (jobOffer == null) return false;

        jobOffer.Position = dto.Position;
        jobOffer.Salary = dto.Salary;
        jobOffer.ContractType = dto.ContractType;
        jobOffer.WorkLoad = dto.WorkLoad;
        jobOffer.WorkMode = dto.WorkMode;
        jobOffer.CompanyName = dto.CompanyName;
        jobOffer.Location = dto.Location;
        jobOffer.Skills = dto.Skills;
        jobOffer.OurRequirements = dto.OurRequirements;
        jobOffer.WhatWeOffer = dto.WhatWeOffer;
        jobOffer.Benefits = dto.Benefits;
        jobOffer.SourceUrl = dto.SourceUrl;
        jobOffer.Status = dto.Status;

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
}

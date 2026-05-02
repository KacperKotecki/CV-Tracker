using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CvTracker.Api.Models;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public JobApplicationsController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<ActionResult<ICollection<JobOffer>>> GetAll()
    {
        var jobOffers = await _context.JobOffers
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .ToListAsync();
        return Ok(jobOffers);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<JobOffer>> GetById(int id)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.Company)
            .Include(j => j.Skills)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (jobOffer == null)
        {
            return NotFound();
        }
        return Ok(jobOffer);
    }
    [HttpPost]
    public async Task<ActionResult<JobOffer>> Create([FromBody] CreateJobOfferDto jobOffer)
    {
        var jobOfferCreated = new JobOffer
        {
            Position = jobOffer.Position,
            Salary = jobOffer.Salary,
            ContractType = jobOffer.ContractType,
            WorkLoad = jobOffer.WorkLoad,
            WorkMode = jobOffer.WorkMode,
            CompanyId = jobOffer.CompanyId,
            Skills = string.IsNullOrWhiteSpace(jobOffer.Skills)
                ? null
                : new List<SkillItem> { new SkillItem { Name = jobOffer.Skills } },
            OurRequirements = string.IsNullOrWhiteSpace(jobOffer.OurRequirements)
                ? null
                : new List<string> { jobOffer.OurRequirements },
            WhatWeOffer = string.IsNullOrWhiteSpace(jobOffer.WhatWeOffer)
                ? null
                : new List<string> { jobOffer.WhatWeOffer },
            Benefits = string.IsNullOrWhiteSpace(jobOffer.Benefits)
                ? null
                : new List<string> { jobOffer.Benefits }
        };
        _context.JobOffers.Add(jobOfferCreated);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = jobOfferCreated.Id }, jobOfferCreated);

    }
}
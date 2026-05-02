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
            .ToListAsync();
        return Ok(jobOffers);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<JobOffer>> GetById(int id)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.Company)
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
            Skills = jobOffer.Skills,
            OurRequirements = jobOffer.OurRequirements,
            WhatWeOffer = jobOffer.WhatWeOffer,
            Benefits = jobOffer.Benefits
        };
        _context.JobOffers.Add(jobOfferCreated);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = jobOfferCreated.Id }, jobOfferCreated);

    }
}
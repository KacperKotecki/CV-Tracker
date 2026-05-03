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
        var jobOffer = await GetJobOfferByIdAsync(id);
        if (jobOffer == null)
        {
            return NotFound();
        }
        return Ok(jobOffer);
    }
    [HttpPost]
    public async Task<ActionResult<JobOffer>> Create([FromBody] JobOfferDto jobOffer)
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
            Benefits = jobOffer.Benefits,
            Status = jobOffer.Status
        };
        _context.JobOffers.Add(jobOfferCreated);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = jobOfferCreated.Id }, jobOfferCreated);

    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Edit(int id, [FromBody] JobOfferDto jobOffer)
    {
        var jobOfferToEdit = await GetJobOfferByIdAsync(id);
        if (jobOfferToEdit == null)
        {
            return NotFound();
        }

        jobOfferToEdit.Position = jobOffer.Position;
        jobOfferToEdit.Salary = jobOffer.Salary;
        jobOfferToEdit.ContractType = jobOffer.ContractType;
        jobOfferToEdit.WorkLoad = jobOffer.WorkLoad;
        jobOfferToEdit.WorkMode = jobOffer.WorkMode;
        jobOfferToEdit.CompanyId = jobOffer.CompanyId;
        jobOfferToEdit.Skills = jobOffer.Skills;
        jobOfferToEdit.OurRequirements = jobOffer.OurRequirements;
        jobOfferToEdit.WhatWeOffer = jobOffer.WhatWeOffer;
        jobOfferToEdit.Benefits = jobOffer.Benefits;
        jobOfferToEdit.Status = jobOffer.Status;

        await _context.SaveChangesAsync();
        return NoContent();

    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] ApplicationStatus status)
    {
        var jobOffer = await GetJobOfferByIdAsync(id);
        if (jobOffer == null)
        {
            return NotFound();
        }

        jobOffer.Status = status;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [NonAction]
    private async Task<JobOffer?> GetJobOfferByIdAsync(int id)
    {
        var jobOffer = await _context.JobOffers
            .Include(j => j.Company)
            .FirstOrDefaultAsync(j => j.Id == id);

        return jobOffer;
    }
}
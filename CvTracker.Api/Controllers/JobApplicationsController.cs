using Microsoft.AspNetCore.Mvc;
using CvTracker.Api.Models;
using CvTracker.Api.Services;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly IJobOfferService _service;

    public JobApplicationsController(IJobOfferService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<JobOffer>>> GetAll()
    {
        var jobOffers = await _service.GetAllAsync();
        return Ok(jobOffers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobOffer>> GetById(int id)
    {
        var jobOffer = await _service.GetByIdAsync(id);
        if (jobOffer == null)
        {
            return NotFound();
        }
        return Ok(jobOffer);
    }

    [HttpPost]
    public async Task<ActionResult<JobOffer>> Create([FromBody] JobOfferDto jobOffer)
    {
        var created = await _service.CreateAsync(jobOffer);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Edit(int id, [FromBody] JobOfferDto jobOffer)
    {
        var updated = await _service.UpdateAsync(id, jobOffer);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult> UpdateStatus(int id, [FromBody] ApplicationStatus status)
    {
        var updated = await _service.UpdateStatusAsync(id, status);
        if (!updated)
        {
            return NotFound();
        }
        return NoContent();
    }
}
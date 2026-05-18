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

    [HttpGet("{id}/notes")]
    public async Task<ActionResult<IEnumerable<JobOfferNote>>> GetNotes(int id)
    {
        var notes = await _service.GetNotesAsync(id);
        if (notes == null) return NotFound();
        return Ok(notes);
    }

    [HttpPost("{id}/notes")]
    public async Task<ActionResult<JobOfferNote>> AddNote(int id, [FromBody] JobOfferNoteDto dto)
    {
        var note = await _service.AddNoteAsync(id, dto);
        if (note == null) return NotFound();
        return CreatedAtAction(nameof(GetNotes), new { id }, note);
    }

    [HttpDelete("{id}/notes/{noteId}")]
    public async Task<ActionResult> DeleteNote(int id, int noteId)
    {
        var deleted = await _service.DeleteNoteAsync(id, noteId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
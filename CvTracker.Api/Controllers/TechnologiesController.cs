using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/technologies")]
public class TechnologiesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<TechnologyCategoryDto>>> GetAll()
    {
        var technologies = await db.Technologies
            .OrderBy(t => t.Id)
            .ToListAsync();

        var result = technologies
            .GroupBy(t => t.Category)
            .Select(g => new TechnologyCategoryDto(
                g.Key,
                g.OrderBy(t => t.Name)
                    .Select(t => new TechnologyDto(t.Id, t.Name, t.Category))
                    .ToList()
            ))
            .ToList();

        return Ok(result);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CompaniesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<Company>>> GetAll()
    {
        var companies = await _context.Companies.ToListAsync();
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> Create([FromBody] CreateCompanyDto dto)
    {
        var company = new Company
        {
            CompanyName = dto.CompanyName,
            CompanyAddress = dto.CompanyAddress
        };
        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = company.Id }, company);
    }
}

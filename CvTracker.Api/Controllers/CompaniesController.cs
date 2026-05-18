using Microsoft.AspNetCore.Mvc;
using CvTracker.Api.Services;

namespace CvTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _service;

    public CompaniesController(ICompanyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<Company>>> GetAll()
    {
        var companies = await _service.GetAllAsync();
        return Ok(companies);
    }

    [HttpPost]
    public async Task<ActionResult<Company>> Create([FromBody] CreateCompanyDto dto)
    {
        var company = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = company.Id }, company);
    }
}

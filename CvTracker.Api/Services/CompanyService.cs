using CvTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CvTracker.Api.Services;

public class CompanyService : ICompanyService
{
    private readonly AppDbContext _context;

    public CompanyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ICollection<Company>> GetAllAsync()
    {
        return await _context.Companies.ToListAsync();
    }

    public async Task<Company> CreateAsync(CreateCompanyDto dto)
    {
        var company = new Company
        {
            CompanyName = dto.CompanyName,
            CompanyAddress = dto.CompanyAddress
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();
        return company;
    }
}

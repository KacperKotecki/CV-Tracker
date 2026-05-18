namespace CvTracker.Api.Services;

public interface ICompanyService
{
    Task<ICollection<Company>> GetAllAsync();
    Task<Company> CreateAsync(CreateCompanyDto dto);
}

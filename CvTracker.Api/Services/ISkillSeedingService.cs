namespace CvTracker.Api.Services;

public interface ISkillSeedingService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}

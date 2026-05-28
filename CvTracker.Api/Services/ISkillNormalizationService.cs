namespace CvTracker.Api.Services;

public interface ISkillNormalizationService
{
    int? Resolve(string rawText);
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

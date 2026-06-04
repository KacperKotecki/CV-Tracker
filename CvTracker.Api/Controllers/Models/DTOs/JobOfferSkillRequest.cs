using CvTracker.Api.Models;

public record JobOfferSkillRequest
{
    public int TechnologyId { get; init; }
    public SkillLevel RequiredLevel { get; init; } = SkillLevel.Mid;
}

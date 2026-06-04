using CvTracker.Api.Models;

public record UpdateUserTechnologiesRequest
{
    public List<UserTechnologyItemRequest> Technologies { get; set; } = [];
}

public record UserTechnologyItemRequest
{
    public int TechnologyId { get; set; }
    public SkillLevel Level { get; set; } = SkillLevel.Mid;
}

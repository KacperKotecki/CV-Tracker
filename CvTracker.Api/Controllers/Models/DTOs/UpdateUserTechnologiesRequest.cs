using System.ComponentModel.DataAnnotations;

public record UpdateUserTechnologiesRequest
{
    public List<UserTechnologyItemRequest> Technologies { get; set; } = [];
}

public record UserTechnologyItemRequest
{
    public int TechnologyId { get; set; }
    [Range(1, 5)] public int Proficiency { get; set; }
}

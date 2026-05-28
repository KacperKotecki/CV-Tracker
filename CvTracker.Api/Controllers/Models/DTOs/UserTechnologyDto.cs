public record UserTechnologyDto
{
    public int Id { get; init; }
    public int TechnologyId { get; init; }
    public required string TechnologyName { get; init; }
    public required string Category { get; init; }
    public int Proficiency { get; init; }
}

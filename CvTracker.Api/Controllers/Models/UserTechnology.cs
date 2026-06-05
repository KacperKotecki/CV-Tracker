using System.Text.Json.Serialization;
using CvTracker.Api.Models;

public class UserTechnology
{
    public int Id { get; set; }
    public int TechnologyId { get; set; }
    public SkillLevel Level { get; set; } = SkillLevel.Mid;
    [JsonIgnore] public Technology Technology { get; set; } = null!;
}

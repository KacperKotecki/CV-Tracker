using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class UserTechnology
{
    public int Id { get; set; }
    public int TechnologyId { get; set; }
    [Range(1, 5)] public int Proficiency { get; set; }
    [JsonIgnore] public Technology Technology { get; set; } = null!;
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class TechnologyAlias
{
    public int Id { get; set; }
    [Required] public required string Alias { get; set; }
    public int TechnologyId { get; set; }
    [JsonIgnore] public Technology Technology { get; set; } = null!;
}

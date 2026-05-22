using System.ComponentModel.DataAnnotations;

public class UpdateUserSkillsRequest
{
    public List<SkillItemRequest> Skills { get; set; } = [];
}

public class SkillItemRequest
{
    [Required] public required string Category { get; set; }
    [Required] public required string SkillName { get; set; }
    [Range(1, 5)] public int Proficiency { get; set; }
}

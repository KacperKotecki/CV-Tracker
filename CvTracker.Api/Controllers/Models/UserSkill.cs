using System.ComponentModel.DataAnnotations;

public class UserSkill
{
    public int Id { get; set; }
    [Required] public required string Category { get; set; }
    [Required] public required string SkillName { get; set; }
    [Range(1, 5)] public int Proficiency { get; set; }
}

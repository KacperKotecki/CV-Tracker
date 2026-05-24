/// <summary>
/// DTO representing a user skill, including the canonical skill reference.
/// </summary>
public class UserSkillDto
{
    public int Id { get; set; }

    /// <summary>FK to the canonical Skill entity.</summary>
    public int SkillId { get; set; }

    public required string Category { get; set; }
    public required string SkillName { get; set; }

    /// <summary>Proficiency level: 0 = skill gap, 1–5 = competency level.</summary>
    public int Proficiency { get; set; }
}

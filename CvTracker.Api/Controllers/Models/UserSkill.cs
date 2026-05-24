using System.ComponentModel.DataAnnotations;

/// <summary>
/// Represents a skill that the user possesses, linked to the canonical <see cref="Skill"/> table.
/// Proficiency 0 means "skill gap" (known gap, not counted as a match); 1–5 indicate competency level.
/// </summary>
public class UserSkill
{
    public int Id { get; set; }

    /// <summary>FK to the canonical Skill entity.</summary>
    public int SkillId { get; set; }

    /// <summary>FK to the owning UserProfile (always 1 for the single-user app).</summary>
    public int UserId { get; set; }

    /// <summary>Navigation property to the canonical Skill entity.</summary>
    public Skill Skill { get; set; } = null!;

    /// <summary>Proficiency level: 0 = skill gap, 1–5 = competency level.</summary>
    [Range(0, 5)]
    public int Proficiency { get; set; }
}

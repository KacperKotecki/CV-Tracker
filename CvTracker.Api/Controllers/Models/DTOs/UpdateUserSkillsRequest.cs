using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request body for <c>PUT /api/profile/skills</c>.
/// Each item references a canonical Skill — use <c>SkillId = 0</c> for find-or-create by name.
/// </summary>
public class UpdateUserSkillsRequest
{
    public List<SkillItemRequest> Skills { get; set; } = [];
}

/// <summary>
/// A single skill entry in <see cref="UpdateUserSkillsRequest"/>.
/// <c>SkillId = 0</c> triggers find-or-create by <c>SkillName</c> + <c>Category</c>.
/// </summary>
public class SkillItemRequest
{
    /// <summary>Canonical Skill ID; 0 means find-or-create by SkillName.</summary>
    public int SkillId { get; set; }

    [Required]
    public required string Category { get; set; }

    [Required]
    public required string SkillName { get; set; }

    /// <summary>Proficiency level: 0 = skill gap, 1–5 = competency level.</summary>
    [Range(0, 5)]
    public int Proficiency { get; set; }
}

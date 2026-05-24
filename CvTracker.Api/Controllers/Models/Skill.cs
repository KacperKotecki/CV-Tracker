using System.ComponentModel.DataAnnotations;

/// <summary>
/// Canonical skill entity — single source of truth for skill names.
/// Both <see cref="UserSkill"/> and <see cref="JobOfferSkill"/> reference this by FK,
/// enabling reliable set-intersection for MatchScore computation.
/// </summary>
public class Skill
{
    public int Id { get; set; }

    /// <summary>Skill name, case-insensitive unique (NOCASE collation in SQLite).</summary>
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    /// <summary>Optional category label (e.g. "Frontend", "Backend").</summary>
    public string? Category { get; set; }
}

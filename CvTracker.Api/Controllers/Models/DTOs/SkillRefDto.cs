/// <summary>
/// Lightweight DTO for a skill reference used in job offer required-skills lists.
/// <c>Id = 0</c> in a request triggers find-or-create by <c>Name</c> on the server.
/// </summary>
/// <param name="Id">Canonical Skill ID; 0 means "find or create by name".</param>
/// <param name="Name">Human-readable skill name.</param>
public record SkillRefDto(int Id, string Name);

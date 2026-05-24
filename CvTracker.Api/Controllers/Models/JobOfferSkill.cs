/// <summary>
/// Join entity connecting a <see cref="JobOffer"/> to a required <see cref="Skill"/>.
/// Replaces the old JSON <c>RequiredSkills</c> column on JobOffer.
/// </summary>
public class JobOfferSkill
{
    public int Id { get; set; }

    /// <summary>FK to the owning JobOffer (CASCADE delete).</summary>
    public int JobOfferId { get; set; }

    /// <summary>FK to the required Skill (CASCADE delete).</summary>
    public int SkillId { get; set; }

    /// <summary>Navigation property to the canonical Skill entity.</summary>
    public Skill Skill { get; set; } = null!;
}

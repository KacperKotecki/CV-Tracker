using System.ComponentModel.DataAnnotations;

public class Technology
{
    public int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Category { get; set; }

    public ICollection<TechnologyAlias> Aliases { get; set; } = [];
    public ICollection<UserTechnology> UserTechnologies { get; set; } = [];
    public ICollection<JobOfferTechnology> JobOfferTechnologies { get; set; } = [];
}

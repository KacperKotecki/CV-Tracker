using System.Text.Json.Serialization;
using CvTracker.Api.Models;

public class JobOfferTechnology
{
    public int JobOfferId { get; set; }
    public int TechnologyId { get; set; }
    public SkillLevel RequiredLevel { get; set; } = SkillLevel.Mid;
    [JsonIgnore] public JobOffer JobOffer { get; set; } = null!;
    [JsonIgnore] public Technology Technology { get; set; } = null!;
}

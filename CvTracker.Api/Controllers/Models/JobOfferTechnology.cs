using System.Text.Json.Serialization;

public class JobOfferTechnology
{
    public int JobOfferId { get; set; }
    public int TechnologyId { get; set; }
    [JsonIgnore] public JobOffer JobOffer { get; set; } = null!;
    [JsonIgnore] public Technology Technology { get; set; } = null!;
}

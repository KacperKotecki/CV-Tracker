public class JobOfferNote
{
    public int Id { get; set; }
    public int JobOfferId { get; set; }
    public required DateTimeOffset EventDate { get; set; }
    public required string Content { get; set; }
}

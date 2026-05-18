using System.ComponentModel.DataAnnotations;

public class JobOfferNoteDto
{
    [Required]
    public DateTimeOffset EventDate { get; set; }

    [Required]
    public required string Content { get; set; }
}

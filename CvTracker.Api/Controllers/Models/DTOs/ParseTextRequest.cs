using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request body for POST /api/parse. Contains raw plain text from a job offer page.
/// Text must be between 50 and 20 000 characters.
/// </summary>
public record ParseTextRequest(
    [Required][MaxLength(20000)] string Text);

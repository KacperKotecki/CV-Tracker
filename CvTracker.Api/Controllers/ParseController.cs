using CvTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Parses raw pasted job-offer text into structured fields.
/// Replaces the removed <c>ScrapeController</c> (which fetched URLs + called OpenRouter).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ParseController : ControllerBase
{
    private readonly IOfferTextParserService _parser;
    private readonly ILogger<ParseController> _logger;

    public ParseController(IOfferTextParserService parser, ILogger<ParseController> logger)
    {
        _parser = parser;
        _logger = logger;
    }

    /// <summary>
    /// Parses the supplied plain text and returns a partially-filled <see cref="ScrapedOfferDto"/>.
    /// </summary>
    /// <param name="request">Request body containing the raw offer text (50–20 000 chars).</param>
    /// <returns>
    /// 200 OK with <see cref="ScrapedOfferDto"/>; 400 Bad Request when text is too short.
    /// </returns>
    [HttpPost]
    public ActionResult<ScrapedOfferDto> ParseOffer([FromBody] ParseTextRequest request)
    {
        var text = request.Text;

        if (string.IsNullOrWhiteSpace(text) || text.Length < 50)
        {
            _logger.LogInformation("Parse request rejected: text too short ({Length} chars)", text?.Length ?? 0);
            return BadRequest("Tekst jest za krótki — wymagane minimum 50 znaków.");
        }

        // Server-side truncation as defence-in-depth (attribute MaxLength(20000) already rejects
        // larger payloads at model-binding time, but we truncate here for safety).
        if (text.Length > 20_000)
            text = text[..20_000];

        _logger.LogDebug("Parsing offer text ({Length} chars)", text.Length);
        var result = _parser.Parse(text);

        return Ok(result);
    }
}

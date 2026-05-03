using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class ScrapeController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ScrapeController> _logger;

    public ScrapeController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ScrapeController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ScrapedOfferDto>> ScrapeOffer([FromBody] ScrapeRequest request)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("Nieprawidłowy URL. Podaj pełny adres z http:// lub https://.");
        }

        string pageText;
        try
        {
            var scrapeClient = _httpClientFactory.CreateClient("ScrapeClient");
            var html = await scrapeClient.GetStringAsync(uri);
            pageText = StripHtml(html);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch URL: {Url}", request.Url);
            return BadRequest("Nie udało się pobrać strony pod podanym URL.");
        }

        if (pageText.Length < 300)
        {
            _logger.LogWarning("Scraped text too short ({Length} chars) for URL: {Url}", pageText.Length, request.Url);
            return UnprocessableEntity(
                "Nie udało się pobrać treści oferty. Strona może wymagać JavaScript lub jest zabezpieczona przed automatycznym pobieraniem.");
        }

        if (pageText.Length > 12000)
            pageText = pageText[..12000];

        var apiKey = _configuration["OpenRouter:ApiKey"];
        var model = _configuration["OpenRouter:Model"] ?? "mistralai/mistral-7b-instruct:free";
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenRouter:ApiKey is not configured.");
            return StatusCode(500, "Brak konfiguracji klucza API OpenRouter.");
        }

        if (string.IsNullOrWhiteSpace(model))        {
            _logger.LogError("OpenRouter:Model is not configured.");
            return StatusCode(500, "Brak konfiguracji modelu OpenRouter.");
        }

        ScrapedOfferDto? result;
        string? rawAiResponse;
        string prompt;
        try
        {
            (result, rawAiResponse, prompt) = await CallOpenRouter(apiKey, model, pageText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenRouter API call failed.");
            return StatusCode(500, "Błąd podczas analizy oferty przez AI.");
        }

        await WriteDebugLogAsync(request.Url, pageText, prompt, rawAiResponse);

        if (result is null)
            return StatusCode(500, "AI nie zwróciło poprawnych danych.");

        return Ok(result);
    }

    private static string StripHtml(string html)
    {
        html = Regex.Replace(html,
            @"<(script|style|head|nav|footer|header|aside|form)[^>]*>[\s\S]*?</(script|style|head|nav|footer|header|aside|form)>",
            " ", RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<[^>]+>", " ");
        html = WebUtility.HtmlDecode(html);
        html = Regex.Replace(html, @"\s+", " ").Trim();
        return html;
    }

    private async Task<(ScrapedOfferDto? result, string? rawAiResponse, string prompt)> CallOpenRouter(string apiKey, string model, string pageText)
    {
        // Static instructions go to "system" — models follow system role more reliably.
        // No interpolation needed here so raw string literal is safe with JSON braces.
        const string systemPrompt = """
            Jesteś ekspertem do ekstrakcji danych z ogłoszeń o pracę.

            BEZWZGLĘDNE ZASADY:
            1. Odpowiedź to WYŁĄCZNIE obiekt JSON — żadnego markdown, żadnych komentarzy, żadnego tekstu przed ani po.
            2. Każde nieznane lub brakujące pole ustaw na null. Nigdy nie używaj "", 0 ani "brak".
            3. Jeśli tekst jest pusty, jest hashem, kodem błędu lub NIE jest ogłoszeniem o pracę — zwróć JSON ze WSZYSTKIMI polami równymi null.
            4. Zachowaj język oryginału: polska oferta → pola po polsku; angielska → po angielsku.

            DOZWOLONE WARTOŚCI ENUM (tylko te lub null, wielkość liter musi się zgadzać):
            contractType : "UoP" | "B2B" | "MandateContract" | "SpecificWorkContract" | "Internship" | "Apprenticeship"
            workMode     : "OnSite" | "Remote" | "Hybrid"
            workLoad     : "FullTime" | "PartTime"

            POLE salary — TYLKO liczba całkowita PLN brutto miesięcznie:
            - Widełki brutto → środek: "8 000–12 000 zł brutto" → (8000+12000)/2 = 10000
            - Wartość netto B2B → pomnóż ×1.23: "10 000 zł netto" → 10000×1.23 = 12300
            - Widełki netto B2B → środek, potem ×1.23: "18 000–22 000 netto B2B" → 20000×1.23 = 24600
            - Brak informacji → null

            PRZYKŁAD WEJŚCIA:
            "Senior Java Developer – Kraków. Wynagrodzenie: 18 000–22 000 PLN netto B2B. Praca hybrydowa (2 dni z biura). Wymagania: Java 17+, Spring Boot, min. 5 lat doświadczenia. Oferujemy: prywatna opieka medyczna LuxMed, karta Multisport."

            PRZYKŁAD WYJŚCIA (dokładnie ten format, nic więcej):
            {"position":"Senior Java Developer","salary":24600,"contractType":"B2B","workMode":"Hybrid","workLoad":"FullTime","skills":"Java 17+, Spring Boot","ourRequirements":"min. 5 lat doświadczenia, Java 17+, Spring Boot","whatWeOffer":"prywatna opieka medyczna LuxMed, karta Multisport","benefits":"prywatna opieka medyczna LuxMed, karta Multisport","companyName":null,"location":"Kraków"}

            SCHEMAT — wszystkie pola są wymagane:
            {"position":null,"salary":null,"contractType":null,"workMode":null,"workLoad":null,"skills":null,"ourRequirements":null,"whatWeOffer":null,"benefits":null,"companyName":null,"location":null}
            """;

        var userContent = $"Przeanalizuj poniższy tekst i zwróć JSON:\n\n{pageText}";
        var fullPromptForLog = $"[SYSTEM]\n{systemPrompt}\n\n[USER]\n{userContent}";

        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userContent  }
            }
        };

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            "https://openrouter.ai/api/v1/chat/completions");

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var client = _httpClientFactory.CreateClient("OpenRouterClient");
        var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var messageContent = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrEmpty(messageContent)) return (null, messageContent, fullPromptForLog);

        var json = ExtractJson(messageContent);
        var result = JsonSerializer.Deserialize<ScrapedOfferDto>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return (result, messageContent, fullPromptForLog);
    }

    private static async Task WriteDebugLogAsync(string url, string pageText, string prompt, string? aiResponse)
    {
        var separator = new string('=', 80);
        var entry = new StringBuilder();
        entry.AppendLine(separator);
        entry.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]  URL: {url}");
        entry.AppendLine(separator);
        entry.AppendLine();
        entry.AppendLine("--- WYCIĄGNIĘTY TEKST ZE STRONY ---");
        entry.AppendLine(pageText);
        entry.AppendLine();
        entry.AppendLine("--- PROMPT WYSŁANY DO MODELU ---");
        entry.AppendLine(prompt);
        entry.AppendLine();
        entry.AppendLine("--- ODPOWIEDŹ MODELU ---");
        entry.AppendLine(aiResponse ?? "(brak odpowiedzi)");
        entry.AppendLine();

        var logPath = Path.Combine(AppContext.BaseDirectory, "scrape-debug.log");
        await System.IO.File.AppendAllTextAsync(logPath, entry.ToString(), Encoding.UTF8);
    }

    private static string ExtractJson(string content)
    {
        var match = Regex.Match(content, @"```(?:json)?\s*([\s\S]*?)\s*```");
        if (match.Success) return match.Groups[1].Value;

        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start >= 0 && end > start) return content[start..(end + 1)];

        return content.Trim();
    }
}

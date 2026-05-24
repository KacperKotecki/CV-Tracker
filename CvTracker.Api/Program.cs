using Microsoft.EntityFrameworkCore;
using CvTracker.Api.Models;
using CvTracker.Api.Services;
using CvTracker.Api.Services.Scraping;

var builder = WebApplication.CreateBuilder(args);

// ── Named HttpClients ──────────────────────────────────────────────────────────

// Generic browser-like client for fetching raw HTML (PracujPl, FallbackScraper).
builder.Services.AddHttpClient("ScrapeClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");
    client.DefaultRequestHeaders.Accept.ParseAdd(
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(
        "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7");
    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
    client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = System.Net.DecompressionMethods.All,
    AllowAutoRedirect = true,
    MaxAutomaticRedirections = 5
});

// ── MVC / JSON ─────────────────────────────────────────────────────────────────

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(allowIntegerValues: false)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── Data ───────────────────────────────────────────────────────────────────────

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Application Services ───────────────────────────────────────────────────────

builder.Services.AddScoped<IJobOfferService, JobOfferService>();

// Scraping: register all concrete scrapers so ScraperFactory can receive them via DI.
builder.Services.AddScoped<JustJoinItScraper>();
builder.Services.AddScoped<NoFluffJobsScraper>();
builder.Services.AddScoped<PracujPlScraper>();
builder.Services.AddScoped<FallbackScraper>();
builder.Services.AddScoped<IScraperFactory, ScraperFactory>();

// ── Build ──────────────────────────────────────────────────────────────────────

var app = builder.Build();

// ── Startup sweep: reset stuck ScrapingInProgress records ─────────────────────
// If the process was killed while a background scrape was running, records may
// remain in ScrapingInProgress forever. Reset them to Draft on every startup.
using (var startupScope = app.Services.CreateScope())
{
    var db = startupScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.JobOffers
        .Where(j => j.Status == ApplicationStatus.ScrapingInProgress)
        .ExecuteUpdateAsync(s => s.SetProperty(j => j.Status, ApplicationStatus.Draft));
}

// ── Middleware pipeline ────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseStaticFiles();
app.UseAuthorization();

// Ensure upload directories exist.
var wwwroot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(wwwroot, "uploads", "avatars"));
Directory.CreateDirectory(Path.Combine(wwwroot, "uploads", "resumes"));

app.MapControllers();

app.Run();
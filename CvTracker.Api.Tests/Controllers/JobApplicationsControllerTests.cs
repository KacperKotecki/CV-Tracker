using CvTracker.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace CvTracker.Api.Tests.Controllers;

/// <summary>
/// Stub seeder that simply ensures the schema exists on the in-memory SQLite DB.
/// The real seeder reads from a JSON file that is not present during tests.
/// </summary>
file sealed class NoOpSkillSeedingService : ISkillSeedingService
{
    private readonly AppDbContext _context;

    public NoOpSkillSeedingService(AppDbContext context) => _context = context;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
        => await _context.Database.EnsureCreatedAsync(cancellationToken);
}

/// <summary>
/// Custom factory that swaps the production SQLite file connection with a shared in-memory
/// SQLite connection. Using the same provider (SQLite) avoids the "two database providers
/// registered" error that would occur when mixing SQLite + InMemory providers.
/// </summary>
public sealed class JobApplicationsWebApplicationFactory : WebApplicationFactory<Program>
{
    // Keep the connection open for the full lifetime of the factory so the
    // in-memory database persists across requests and seeding scopes.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public JobApplicationsWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the production DbContextOptions (pointing to the SQLite file).
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Re-register with the shared in-memory SQLite connection.
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));

            // Replace the seeder so it creates the schema instead of reading a seed file.
            var seederDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ISkillSeedingService));
            if (seederDescriptor != null)
                services.Remove(seederDescriptor);

            services.AddScoped<ISkillSeedingService, NoOpSkillSeedingService>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}

public class JobApplicationsControllerTests : IClassFixture<JobApplicationsWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    private readonly JobApplicationsWebApplicationFactory _factory;

    public JobApplicationsControllerTests(JobApplicationsWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAll_WithSeededOffers_Returns200AndMatchingCount()
    {
        // Arrange – seed 3 job offers; one linked to a real Technology via the join table.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Guard: skip seeding if data already exists (factory is shared across tests).
            if (!await db.JobOffers.AnyAsync())
            {
                var technology = new Technology { Name = "C#", Category = "Language" };
                db.Technologies.Add(technology);
                await db.SaveChangesAsync();

                db.JobOffers.AddRange(
                    new JobOffer
                    {
                        Position = "Backend Developer",
                        RequiredTechnologies =
                        [
                            new JobOfferTechnology { TechnologyId = technology.Id },
                        ],
                    },
                    new JobOffer { Position = "Frontend Developer" },
                    new JobOffer { Position = "Full Stack Developer" });

                await db.SaveChangesAsync();
            }
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/jobapplications");

        // Assert 1 – 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Assert 2 – body deserializes to List<JobOfferDto> without JsonException
        var json = await response.Content.ReadAsStringAsync();
        var offers = JsonSerializer.Deserialize<List<JobOfferDto>>(json, JsonOptions);
        Assert.NotNull(offers);

        // Assert 3 – returned count matches seeded count
        Assert.Equal(3, offers.Count);
    }

    [Fact]
    public async Task CreateOffer_WithRequiredSkillIds_SkillIdsPersistedAndReturnedOnGet()
    {
        // Arrange — use an isolated factory so this test does not share the DB with GetAll.
        await using var isolatedFactory = new JobApplicationsWebApplicationFactory();

        int techId;
        using (var scope = isolatedFactory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var tech = new Technology { Name = "TypeScript", Category = "Language" };
            db.Technologies.Add(tech);
            await db.SaveChangesAsync();
            techId = tech.Id;
        }

        var client = isolatedFactory.CreateClient();

        // Act — POST a new offer that references the seeded skill ID.
        // This verifies the full data flow:
        //   OfferForm requiredSkills → POST body → JobOfferDto.RequiredSkills
        //   → JobOfferService.CreateAsync → JobOfferTechnology rows inserted.
        var payload = new
        {
            position = "Frontend Developer",
            contractType = "B2B",
            workMode = "Remote",
            workLoad = "FullTime",
            status = "Applied",
            requiredSkills = new[] { new { technologyId = techId, requiredLevel = "Mid" } },
        };
        var postResponse = await client.PostAsJsonAsync("/api/jobapplications", payload);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var createdJson = await postResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JobOfferResponse>(createdJson, JsonOptions);
        Assert.NotNull(created);

        // Assert — GET by ID returns the offer with the skill ID present in RequiredSkillIds.
        var getResponse = await client.GetAsync($"/api/jobapplications/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getJson = await getResponse.Content.ReadAsStringAsync();
        var retrieved = JsonSerializer.Deserialize<JobOfferResponse>(getJson, JsonOptions);
        Assert.NotNull(retrieved);
        Assert.Contains(techId, retrieved.RequiredSkillIds);
    }
}

/// <summary>
/// Minimal shape for deserializing responses from <c>GET /api/jobapplications/{id}</c>
/// and <c>POST /api/jobapplications</c> in controller tests.
/// </summary>
file sealed class JobOfferResponse
{
    public int Id { get; set; }
    public List<int> RequiredSkillIds { get; set; } = [];
}

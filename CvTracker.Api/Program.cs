using Microsoft.EntityFrameworkCore;
using CvTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IJobOfferService, JobOfferService>();
builder.Services.AddScoped<ISkillSeedingService, SkillSeedingService>();
builder.Services.AddScoped<IOfferTextParserService, OfferTextParserService>();
builder.Services.AddSingleton<ISkillNormalizationService, SkillNormalizationService>();

var app = builder.Build();

// Apply pending migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<ISkillSeedingService>();
    await seeder.SeedAsync();
}

var normalizationService = app.Services.GetRequiredService<ISkillNormalizationService>();
await normalizationService.InitializeAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseStaticFiles();
app.UseAuthorization();

// Ensure upload directories exist
var wwwroot = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
Directory.CreateDirectory(Path.Combine(wwwroot, "uploads", "avatars"));
Directory.CreateDirectory(Path.Combine(wwwroot, "uploads", "resumes"));

app.MapControllers();

app.Run();

// Expose Program to the integration-test project
public partial class Program { }
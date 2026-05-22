using System.Text.Json;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
       modelBuilder.Entity<JobOffer>()
            .Property(x => x.ContractType)
            .HasConversion<string>();
       modelBuilder.Entity<JobOffer>()
            .Property(x => x.WorkLoad)
            .HasConversion<string>();
       modelBuilder.Entity<JobOffer>()
            .Property(x => x.WorkMode)
            .HasConversion<string>();
       modelBuilder.Entity<JobOffer>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<JobOfferNote>()
            .HasOne<JobOffer>()
            .WithMany(j => j.Notes)
            .HasForeignKey(n => n.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<JobOfferNote> JobOfferNotes { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
}
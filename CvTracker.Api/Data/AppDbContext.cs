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

        modelBuilder.Entity<JobOfferTechnology>()
            .HasKey(t => new { t.JobOfferId, t.TechnologyId });

        modelBuilder.Entity<JobOfferTechnology>()
            .HasOne(t => t.JobOffer)
            .WithMany(o => o.RequiredTechnologies)
            .HasForeignKey(t => t.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<JobOfferTechnology>()
            .HasOne(t => t.Technology)
            .WithMany(t => t.JobOfferTechnologies)
            .HasForeignKey(t => t.TechnologyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TechnologyAlias>()
            .HasOne(a => a.Technology)
            .WithMany(t => t.Aliases)
            .HasForeignKey(a => a.TechnologyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Technology>()
            .HasIndex(t => t.Name)
            .IsUnique()
            .HasDatabaseName("IX_Technologies_Name_CI");

        modelBuilder.Entity<TechnologyAlias>()
            .HasIndex(a => a.Alias)
            .IsUnique()
            .HasDatabaseName("IX_TechnologyAliases_Alias_CI");
    }

    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<JobOfferNote> JobOfferNotes { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Technology> Technologies { get; set; }
    public DbSet<TechnologyAlias> TechnologyAliases { get; set; }
    public DbSet<UserTechnology> UserTechnologies { get; set; }
    public DbSet<JobOfferTechnology> JobOfferTechnologies { get; set; }
}
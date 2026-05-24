using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── JobOffer enum conversions ───────────────────────────────────────────
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

        // ── JobOfferNote → JobOffer (CASCADE delete) ────────────────────────────
        modelBuilder.Entity<JobOfferNote>()
            .HasOne<JobOffer>()
            .WithMany(j => j.Notes)
            .HasForeignKey(n => n.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Skill: unique case-insensitive index on Name ────────────────────────
        modelBuilder.Entity<Skill>()
            .HasIndex(s => s.Name)
            .IsUnique();
        modelBuilder.Entity<Skill>()
            .Property(s => s.Name)
            .UseCollation("NOCASE");

        // ── JobOfferSkill → JobOffer (CASCADE delete) ───────────────────────────
        modelBuilder.Entity<JobOfferSkill>()
            .HasOne<JobOffer>()
            .WithMany(j => j.JobOfferSkills)
            .HasForeignKey(jos => jos.JobOfferId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── JobOfferSkill → Skill (CASCADE delete) ──────────────────────────────
        modelBuilder.Entity<JobOfferSkill>()
            .HasOne(jos => jos.Skill)
            .WithMany()
            .HasForeignKey(jos => jos.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── UserSkill → Skill (CASCADE delete) ─────────────────────────────────
        modelBuilder.Entity<UserSkill>()
            .HasOne(us => us.Skill)
            .WithMany()
            .HasForeignKey(us => us.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── UserSkill → UserProfile (CASCADE delete) ────────────────────────────
        modelBuilder.Entity<UserSkill>()
            .HasOne<UserProfile>()
            .WithMany()
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<JobOffer> JobOffers { get; set; }
    public DbSet<JobOfferNote> JobOfferNotes { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }

    /// <summary>Canonical skill catalog shared by UserSkill and JobOfferSkill.</summary>
    public DbSet<Skill> Skills { get; set; }

    /// <summary>Join table connecting job offers to their required skills.</summary>
    public DbSet<JobOfferSkill> JobOfferSkills { get; set; }
}

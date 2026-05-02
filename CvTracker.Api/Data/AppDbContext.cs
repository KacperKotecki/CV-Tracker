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

        
    }
    public DbSet<Company> Companies { get; set; }
    public DbSet<JobOffer> JobOffers { get; set; }
}
using Microsoft.EntityFrameworkCore;
using PollingStationAPI.Data.Models;

namespace PollingStationAPI.Data.Contexts;

public class CosmosDbContext : DbContext
{
    public CosmosDbContext(DbContextOptions options) : base(options) { }

    public DbSet<PollingStation> PollingStation { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PollingStation>()
        .HasNoDiscriminator()
        .HasPartitionKey(x => x.Id)
        .HasKey(x => x.Id);
    }
}
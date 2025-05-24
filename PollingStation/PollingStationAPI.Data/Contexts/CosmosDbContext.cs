using Microsoft.EntityFrameworkCore;
using PollingStationAPI.Data.Models;
using System.Security.Cryptography;

namespace PollingStationAPI.Data.Contexts;

public class CosmosDbContext : DbContext
{
    public CosmosDbContext(DbContextOptions options) : base(options) { }

    public DbSet<PollingStation> PollingStation { get; set; }
    public DbSet<CommitteeMember> CommitteeMember { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PollingStation>()
        .HasNoDiscriminator()
        .HasPartitionKey(x => x.Id)
        .HasKey(x => x.Id);

        modelBuilder.Entity<CommitteeMember>()
        .HasNoDiscriminator()
        .HasPartitionKey(x => x.Id)
        .HasKey(x => x.Id);
    }
}
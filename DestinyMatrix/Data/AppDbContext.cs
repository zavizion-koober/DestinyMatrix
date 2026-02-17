using Microsoft.EntityFrameworkCore;
using DestinyMatrix.Features.DestinyMatrix;

namespace DestinyMatrix.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<ArcanaEntity> Arcanas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArcanaEntity>().OwnsOne(a => a.Energy);
    }
}

public class ArcanaEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Archetype { get; set; } = string.Empty;
    public EnergyState Energy { get; set; } = new(string.Empty, string.Empty);
    public string ZonesJson { get; set; } = string.Empty; 
}
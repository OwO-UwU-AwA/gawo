using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using gawo.Models;

namespace gawo;

public class GawoDbContext : DbContext
{
    public DbSet<User>? Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        var connectionString = configuration.GetConnectionString("GawoDbContext");
        optionsBuilder.UseSqlite(connectionString);
    }
}
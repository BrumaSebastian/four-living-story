using Microsoft.EntityFrameworkCore;

namespace FourLivingStory.ApiService.Infrastructure.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Each module registers its entity configurations via IEntityTypeConfiguration<T>.
        // ApplyConfigurationsFromAssembly discovers all of them automatically.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTechnicalEquipmentBorrowingSystem.Data;

// Used by `dotnet ef` commands, which bypass Program.cs entirely.
// Reads .env (real DB) so `dotnet ef database update` targets the actual database;
// falls back to an offline placeholder for `migrations add` when no .env is present.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        if (File.Exists(".env")) DotNetEnv.Env.Load();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=design_time;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new AppDbContext(options);
    }
}

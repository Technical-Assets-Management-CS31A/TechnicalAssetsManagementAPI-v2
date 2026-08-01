using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BackendTechnicalEquipmentBorrowingSystem.Data;

// Lets `dotnet ef migrations add` scaffold offline, with no real DB or Program.cs config.
// The runtime connection string comes from configuration in Program.cs, not from here.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time;Username=postgres;Password=postgres")
            .Options;
        return new AppDbContext(options);
    }
}

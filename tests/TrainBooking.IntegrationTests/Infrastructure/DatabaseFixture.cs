using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using TrainBooking.Infrastructure.Persistence;

namespace TrainBooking.IntegrationTests.Infrastructure;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithPassword("Password_12345!")
        .Build();

    public string ConnectionString => _container.GetConnectionString() + ";Database=TrainBooking;TrustServerCertificate=true";

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await ApplyMigrationsAsync();
    }

    public async Task DisposeAsync() =>
        await _container.DisposeAsync();

    private async Task ApplyMigrationsAsync()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(ConnectionString, b => b.MigrationsAssembly("TrainBooking.Migrations"))
        .Options;

        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.MigrateAsync();
    }

    public AppDbContext CreateDbContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(ConnectionString, b => b.MigrationsAssembly("TrainBooking.Migrations"))
        .Options;

        return new AppDbContext(options);
    }
}

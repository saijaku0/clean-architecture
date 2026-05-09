using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrainBooking.Infrastructure;
using TrainBooking.Infrastructure.Persistence;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("TrainBooking.Migrations")));

using IHost host = builder.Build();

using IServiceScope scope = host.Services.CreateScope();
ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

try
{
    logger.LogInformation("Applying migrations...");
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Migrations applied successfully.");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to apply migrations.");
    return 1;
}

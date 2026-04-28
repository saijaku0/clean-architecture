using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrainBooking.Infrastructure;
using TrainBooking.Infrastructure.Persistence;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

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

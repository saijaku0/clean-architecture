using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TrainBooking.Infrastructure.Persistence;
using TrainBooking.Seed;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found. " +
        "Set it via user secrets or environment variable.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<DataFactory>();
builder.Services.AddScoped<SeedRunner>();

using IHost host = builder.Build();

SeedAction action = ParseAction(args);

using IServiceScope scope = host.Services.CreateScope();
ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
SeedRunner runner = scope.ServiceProvider.GetRequiredService<SeedRunner>();

try
{
    await runner.RunAsync(action);
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "Seed operation failed.");
    return 1;
}

static SeedAction ParseAction(string[] args)
{
    string? actionArg = null;

    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--action" && i + 1 < args.Length)
        {
            actionArg = args[i + 1];
            break;
        }
    }

    actionArg ??= args.FirstOrDefault();

    if (string.IsNullOrWhiteSpace(actionArg))
        return SeedAction.Full;

    if (Enum.TryParse<SeedAction>(actionArg, ignoreCase: true, out SeedAction parsed))
        return parsed;

    throw new ArgumentException(
        $"Unknown action: '{actionArg}'. Valid values: {string.Join(", ", Enum.GetNames<SeedAction>())}.");
}

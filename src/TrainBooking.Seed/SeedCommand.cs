using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace TrainBooking.Seed;

/// <summary>
/// Command to seed the database with initial data for the Train Booking application.
/// </summary>
internal sealed class SeedCommand : Command
{
    public SeedCommand(IServiceProvider serviceProvider)
        : base("seed", "Database seeding management")
    {
        var actionOption = new Option<SeedAction>("--action")
        {
            Description = "The seeding action to perform",
            DefaultValueFactory = (result) => SeedAction.Seed
        };

        Options.Add(actionOption);

        SetAction(async (result, ct) =>
        {
            SeedAction action = result.GetValue(actionOption);
            SeedRunner? runner = ActivatorUtilities.CreateInstance<SeedRunner>(serviceProvider);
            await runner.RunAsync(action, ct);
        });
    }
}

/// <summary>
/// Actions that can be performed by the SeedCommand.
/// </summary>
public enum SeedAction
{
    Seed,
    Full,
    Reset
}

using Microsoft.Extensions.DependencyInjection;

namespace TrainBooking.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(
        this ServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly));

        return services;
    }
}

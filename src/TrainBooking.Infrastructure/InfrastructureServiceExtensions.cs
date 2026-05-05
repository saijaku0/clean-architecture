using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Infrastructure.Persistence;
using TrainBooking.Infrastructure.Persistence.Interceptors;
using TrainBooking.Infrastructure.Persistence.Repositories;

namespace TrainBooking.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly("TrainBooking.Migrations"));
            options.AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<ITripSeatRepository, TripSeatRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        return services;
    }

}

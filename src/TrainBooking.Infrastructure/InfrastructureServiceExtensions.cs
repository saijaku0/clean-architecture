using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TrainBooking.Application.Abstractions.Repositories;
using TrainBooking.Infrastructure.Persistence;
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
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly("TrainBooking.Migrations")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITrainRepository, TrainRepository>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<ITripSeatRepository, TripSeatRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        return services;
    }

}

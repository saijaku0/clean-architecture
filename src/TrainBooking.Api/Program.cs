using Microsoft.AspNetCore.Authentication.JwtBearer;
using TrainBooking.Api.BackgroundServices;
using TrainBooking.Api.Middleware;
using TrainBooking.Api.Service;
using TrainBooking.Application;
using TrainBooking.Application.Abstractions.Identity;
using TrainBooking.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddHostedService<ExpireReservationsService>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.ConfigureOptions<Auth0JwtBearerOptionsConfigurator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Auth0UserProvisioningMiddleware>();
app.MapControllers();

app.Run();

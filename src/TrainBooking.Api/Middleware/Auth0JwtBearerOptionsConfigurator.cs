using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace TrainBooking.Api.Middleware;

internal sealed class Auth0JwtBearerOptionsConfigurator(IConfiguration configuration)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        options.Authority = $"https://{configuration["Auth0:Domain"]}/";
        options.Audience = configuration["Auth0:Audience"];

        options.MapInboundClaims = false;

        // Optional for local development
        if (bool.TryParse(configuration["Auth0:RequireHttpsMetadata"], out bool requireHttps))
            options.RequireHttpsMetadata = requireHttps;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            TryAllIssuerSigningKeys = true
        };
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
            Configure(options);
    }
}

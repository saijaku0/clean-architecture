# This stage is used when launching from Visual Studio in fast mode (default for the debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used for building the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TrainBooking.Api/TrainBooking.Api.csproj", "TrainBooking.Api/"]
RUN dotnet restore "./TrainBooking.Api/TrainBooking.Api.csproj"
COPY . .
WORKDIR "/src/TrainBooking.Api"
RUN dotnet build "./TrainBooking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used for publishing the service project, which will be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TrainBooking.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in the production environment or when launching from VS in normal mode (default when debug configuration is not used)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TrainBooking.Api.dll"]

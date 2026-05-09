FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY ["src/", "/src/"]
WORKDIR /src/TrainBooking.Api
RUN dotnet restore "TrainBooking.Api.csproj" && \
    dotnet publish "TrainBooking.Api.csproj" --no-restore -c Release -o /app/publish

# This stage is used in the production environment or when launching from VS in normal mode (default when debug configuration is not used)
FROM base AS final
COPY --from=build /app/publish .
USER 1000
ENTRYPOINT ["dotnet", "TrainBooking.Api.dll"]

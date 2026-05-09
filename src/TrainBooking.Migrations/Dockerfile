FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
COPY ["src/", "/src/"]
WORKDIR /src/TrainBooking.Migrations
RUN dotnet restore "TrainBooking.Migrations.csproj" && \
    dotnet publish "TrainBooking.Migrations.csproj" --no-restore -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrainBooking.Migrations.dll"]
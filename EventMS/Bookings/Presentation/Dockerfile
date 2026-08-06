
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Auth/Contracts/Contracts.csproj", "Auth/Contracts/"]
COPY ["Shared/KafkaSettings/KafkaSettingsShared.csproj", "Shared/KafkaSettings/"]
COPY ["Bookings/Domain/Domain.csproj", "Bookings/Domain/"]
COPY ["Bookings/Application/Application.csproj", "Bookings/Application/"]
COPY ["Bookings/Infrastructure/Infrastructure.csproj", "Bookings/Infrastructure/"]
COPY ["Bookings/Presentation/BookingsService.csproj", "Bookings/Presentation/"]

RUN dotnet restore "Bookings/Presentation/BookingsService.csproj"

COPY . .

WORKDIR "/src/Bookings/Presentation"
RUN dotnet build "BookingsService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "BookingsService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingsService.dll"]


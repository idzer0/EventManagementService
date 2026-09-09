
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Auth/Contracts/EventMS.Auth.Contracts.csproj", "Auth/Contracts/"]
COPY ["Shared/KafkaSettings/KafkaSettingsShared.csproj", "Shared/KafkaSettings/"]
COPY ["Bookings/Domain/EventMS.Bookings.Domain.csproj", "Bookings/Domain/"]
COPY ["Bookings/Application/EventMS.Bookings.Application.csproj", "Bookings/Application/"]
COPY ["Bookings/Infrastructure/EventMS.Bookings.Infrastructure.csproj", "Bookings/Infrastructure/"]
COPY ["Bookings/Presentation/EventMS.Bookings.BookingsService.csproj", "Bookings/Presentation/"]

RUN dotnet restore "Bookings/Presentation/EventMS.Bookings.BookingsService.csproj"

COPY . .

WORKDIR "/src/Bookings/Presentation"
RUN dotnet build "EventMS.Bookings.BookingsService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "EventMS.Bookings.BookingsService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем iproute2, чтобы получить команду ss
RUN apt-get update && apt-get install -y iproute2 && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "EventMS.Bookings.BookingsService.dll"]


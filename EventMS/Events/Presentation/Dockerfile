
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Auth/Contracts/EventMS.Auth.Contracts.csproj", "Auth/Contracts/"]
COPY ["Shared/KafkaSettings/KafkaSettingsShared.csproj", "Shared/KafkaSettings/"]
COPY ["Events/Domain/EventMS.Events.Domain.csproj", "Events/Domain/"]
COPY ["Events/Application/EventMS.Events.Application.csproj", "Events/Application/"]
COPY ["Events/Infrastructure/EventMS.Events.Infrastructure.csproj", "Events/Infrastructure/"]
COPY ["Events/Presentation/EventMS.Events.EventsService.csproj", "Events/Presentation/"]

RUN dotnet restore "Events/Presentation/EventMS.Events.EventsService.csproj"

COPY . .

WORKDIR "/src/Events/Presentation"
RUN dotnet build "EventMS.Events.EventsService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "EventMS.Events.EventsService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем iproute2, чтобы получить команду ss
RUN apt-get update && apt-get install -y iproute2 && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "EventMS.Events.EventsService.dll"]
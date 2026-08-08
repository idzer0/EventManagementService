
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Auth/Contracts/Contracts.csproj", "Auth/Contracts/"]
COPY ["Shared/KafkaSettings/KafkaSettingsShared.csproj", "Shared/KafkaSettings/"]
COPY ["Events/Domain/Domain.csproj", "Events/Domain/"]
COPY ["Events/Application/Application.csproj", "Events/Application/"]
COPY ["Events/Infrastructure/Infrastructure.csproj", "Events/Infrastructure/"]
COPY ["Events/Presentation/EventsService.csproj", "Events/Presentation/"]

RUN dotnet restore "Events/Presentation/EventsService.csproj"

COPY . .

WORKDIR "/src/Events/Presentation"
RUN dotnet build "EventsService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "EventsService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventsService.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Auth/Contracts/EventMS.Auth.Contracts.csproj", "Auth/Contracts/"]
COPY ["Auth/Domain/EventMS.Auth.Domain.csproj", "Auth/Domain/"]
COPY ["Auth/Application/EventMS.Auth.Application.csproj", "Auth/Application/"]
COPY ["Auth/Infrastructure/EventMS.Auth.Infrastructure.csproj", "Auth/Infrastructure/"]
COPY ["Auth/Presentation/EventMS.Auth.AuthService.csproj", "Auth/Presentation/"]

RUN dotnet restore "Auth/Presentation/EventMS.Auth.AuthService.csproj"

COPY . .
WORKDIR "/src/Auth/Presentation"
RUN dotnet build "EventMS.Auth.AuthService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "EventMS.Auth.AuthService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Устанавливаем iproute2, чтобы получить команду ss
RUN apt-get update && apt-get install -y iproute2 && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "EventMS.Auth.AuthService.dll"]

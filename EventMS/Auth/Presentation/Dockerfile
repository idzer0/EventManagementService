
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Auth/Contracts/Contracts.csproj", "Auth/Contracts/"]
COPY ["Auth/Domain/Domain.csproj", "Auth/Domain/"]
COPY ["Auth/Application/Application.csproj", "Auth/Application/"]
COPY ["Auth/Infrastructure/Infrastructure.csproj", "Auth/Infrastructure/"]
COPY ["Auth/Presentation/AuthService.csproj", "Auth/Presentation/"]

RUN dotnet restore "Auth/Presentation/AuthService.csproj"

COPY . .
WORKDIR "/src/Auth/Presentation"
RUN dotnet build "AuthService.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Debug -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]

# Event Management API

REST API для управления мероприятиями.  
Реализованы CRUD-операции, хранение в памяти, валидация, Swagger.

## Требования
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

Использовался SDK 10.0.105, включает:
    .NET Runtime 10.0.5
    ASP.NET Core Runtime 10.0.5
    .NET Desktop Runtime 10.0.5

## Запуск
1. Клонировать репозиторий:
   ```bash
   git clone https://github.com/idzer0/EventManagementService.git
   cd EventManagementService

2. Востановить зависимости:
    dotnet restore

3. Собрать проект:
    dotnet build

4. Запустить приложение:
    dotnet run --project ./EventManagementService/EventManagementService.csproj

5. Открыть Swagger UI:
http://localhost:5244/swagger (порт может отличаться; точный адрес выводится в консоли после запуска).

## Тестирование

Проект использует **xUnit**, **Moq** и **FluentAssertions** для юнит-тестирования.

### Запуск тестов

```bash
dotnet test
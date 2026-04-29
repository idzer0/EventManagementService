# Event Management API

REST API для управления мероприятиями.  
Реализованы CRUD-операции, хранение в памяти, валидация, Swagger.

## 📋 О проекте

REST API сервис, позволяющий:
- Создавать и управлять мероприятиями (событиями)
- Бронировать места на мероприятия
- Получать статус обработки бронирования
- Асинхронно обрабатывать заявки с помощью фонового сервиса

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

#### Статусы бронирования (BookingStatus)
Статус	Описание
Pending = 1	    Бронь создана, ожидает обработки
Confirmed = 2	Бронь подтверждена системой
Rejected = 3	Бронь отклонена (например, нет свободных мест)

#### API Эндпоинты
Управление мероприятиями (Events)
Метод	Эндпоинт	Описание
POST	/events	Создать новое мероприятие
GET	/events	Получить список всех мероприятий
GET	/events/{id}	Получить мероприятие по ID
PUT	/events/{id}	Обновить мероприятие
DELETE	/events/{id}	Удалить мероприятие

Управление бронированиями (Bookings)
Метод	Эндпоинт	Описание	Ответ
POST	/events/{id}/book	Создать бронь на мероприятие	202 Accepted + Location header
GET	/bookings/{id}	Получить статус бронирования	200 OK или 404 Not Found


#### Детали реализации
Асинхронное создание бронирования
POST /events/{eventId}/book

Создаёт новое бронирование и мгновенно возвращает ответ, обработка выполняется в фоне.

Пример запроса:

bash
curl -X POST http://localhost:5244/events/3fa85f64-5717-4562-b3fc-2c963f66afa6/book
Пример успешного ответа (202 Accepted):

http
HTTP/1.1 202 Accepted
Location: /bookings/7c9e6679-7425-40de-944b-e07fc1f90ae7
Content-Type: application/json

{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-11-20T14:30:00Z",
  "processedAt": null
}
Коды ответов:

202 Accepted — бронь создана, обработка начата

404 Not Found — мероприятие с указанным Id не найдено

Получение статуса бронирования
GET /bookings/{bookingId}

Возвращает текущее состояние бронирования.

Пример запроса:

bash
curl -X GET https://api.example.com/bookings/7c9e6679-7425-40de-944b-e07fc1f90ae7
Пример ответа (200 OK) — бронь в обработке:

json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-11-20T14:30:00Z",
  "processedAt": null
}
Пример ответа (200 OK) — бронь обработана:

json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Confirmed",
  "createdAt": "2024-11-20T14:30:00Z",
  "processedAt": "2024-11-20T14:30:05Z"
}
Коды ответов:

200 OK — бронь найдена

404 Not Found — бронь с указанным Id не существует


#### Фоновая обработка заявок
Сервис использует BookingBackgroundProcessing для асинхронной обработки бронирований:

Алгоритм работы:
Фоновый сервис запускается при старте приложения

Каждые 5 секунд сервис проверяет хранилище на наличие броней со статусом Pending

Для каждой найденной брони:

Выполняется искусственная задержка 2 секунды (имитация вызова внешней системы)

Бронь переводится в статус Confirmed (в текущей версии)

Заполняется поле ProcessedAt текущим временем

Обновлённая бронь сохраняется в хранилище

При ошибках выполняется логирование и сервис продолжает работу
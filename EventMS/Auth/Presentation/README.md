# AuthService

REST API для управления пользователями.  
Реализованы CRUD-операции, валидация, Swagger.

## 📋 О проекте

REST API сервис, позволяющий:
- Создавать нового пользователя
- Осуществлять вход и получение JWT-токена

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

2. Восстановить зависимости:
    dotnet restore

3. Собрать проект:
    dotnet build

4. Запустить приложение:
  в Production
    dotnet run --project ./EventManagementService/EventMS/Auth/Presentation.csproj
  в окружении Development
    ASPNETCORE_ENVIRONMENT=Development dotnet run --project ./EventManagementService/EventMS/Auth/Presentation

5. Открыть Swagger UI:
http://localhost:5000/swagger (порт может отличаться; точный адрес выводится в консоли после запуска).

Для авторизации в сваггер необходимо по кнопке Authorize ввести токен, полученный в методе login, в предложенное поле в формате:
"Bearer <токен>" (без кавычек).

## Структура решения

Решение содержит проекты:
Domain
  Содержит:  
  - доменные сущности и перечисления;
  - доменные исключения.

Application
  Содержит:  
  - интерфейсы сервисов и их реализации (use cases);
  - интерфейсы портов — абстракции для доступа к данным (репозитории) и внешним сервисам;
  - DTO (объекты передачи данных между слоями);
  - фоновые сервисы.

Infrastructure
  Содержит:  
  - реализации интерфейсов репозиториев с использованием DbContext;
  - сам DbContext, конфигурации маппинга сущностей, миграции.

Presentation
  Содержит контроллеры и обработчик глобальных исключений с маппингом доменных исключений в HTTP-статусы. 

## Тестирование

Проект использует **xUnit**, **Moq** и **FluentAssertions** для юнит-тестирования.

Для интеграционного тестирования используется InMemory-провайдер.

### Запуск тестов

```bash
dotnet test

Для выполнения тестов проекта AuthServiceTestsDb требуется docker.
В linux системах может возникать проблема с запуском контейнера из-за недостатка прав.
Для решения этой проблемы следует выполнить следующие команды:

```bash
sudo chgrp "$(id -gn)" /var/run/docker.sock
sudo chmod g+rw /var/run/docker.sock

#### API Эндпоинты
Управление мероприятиями (Events)
Метод	Эндпоинт	Описание
POST	/events	Создать новое мероприятие
GET	/events	Получить список всех мероприятий
GET	/events/{id}	Получить мероприятие по ID
PUT	/events/{id}	Обновить мероприятие
DELETE	/events/{id}	Удалить мероприятие


#### Детали реализации
Регистрация пользователя
POST /auth/register

Создаёт нового пользователя. Возвращает Ok (http code 200)

Пример запроса:

bash
curl -X POST http://localhost:5000/auth/register

200 Accepted — бронь создана, обработка начата
400 Bad request - ошибки валидации имени пользователя и пароля


#### Параметры JWT токена 

Хранятся в отдельном конфигурационном файле, путь к которому указан в параметре PathToJwtSecret основного конфига.


#### База данных

В проекте используется СУБД Postgresql. Перед запуском сервиса необходимо развернуть экземпляр СУБД том же хосте или на хосте, доступном по сети. 

Строка подключения настраивается в файле appsettings.json, в параметре ConnectionStrings.


#### Работа с изменениями схемы данных

Создание изменений схемы данных: 
  dotnet ef migrations add <имя_миграции> --project Infrastructure --startup-project Presentation
  
  или через .sql файлы:
    ./create-migration.sh /path/to/project [имя_миграции]

Применение изменений к базе данных:
  dotnet ef database update --project Infrastructure --startup-project Presentation

  или через .sql файлы:
  ./apply-migration-sql.sh /path/to/project [имя_миграции]

## Наблюдаемость (Observability)
Проект использует стек OpenTelemetry + Prometheus + Jaeger + Grafana для сбора, хранения и визуализации телеметрии: трассировок, метрик и (опционально) логов.

### Как это работает
Микросервис инструментирован с помощью OpenTelemetry SDK для .NET:

Трассировки собираются через инструментации ASP.NET Core, HttpClient, Entity Framework Core и экспортируются в Jaeger по протоколу OTLP.

Метрики (рантайм, ASP.NET Core) экспортируются в Prometheus через эндпоинт /metrics, который Prometheus периодически опрашивает (scrape).

Grafana подключается к Prometheus как источнику данных для построения дашбордов.

### Конфигурация сервисов
В каждом сервисе добавлен метод расширения AddOpenTelemetryService, регистрирующий OpenTelemetry

Каждому сервису в docker-compose.yml задаются стандартные переменные OpenTelemetry:
OTEL_SERVICE_NAME и OTEL_SERVICE_VERSION используются для идентификации сервиса в Jaeger/Prometheus.
OTEL_EXPORTER_OTLP_ENDPOINT указывает SDK, куда отправлять трассировки (в данном случае на Jaeger, который слушает OTLP gRPC на порту 4317).

### Контейнеризация
Сборка контейнера выполняется командой:
docker-compose build authservice

Для изменений сервиса необходимо пересобрать контейнер.

Запуск инфраструктуры вместе с сервисами выполняется из папки с файлом docker-compose.yml командой
docker-compose up -d

остановка инфраструктуры вместе с сервисами выполняется командой
docker-compose down (для удалени volume используется ключ -v)

### Инфраструктура Observability
В docker-compose.yml подняты 4 сервиса:

Seq (порт 8080) собирает логи.

Prometheus (порт 9090) собирает метрики с эндпоинтов /metrics каждого сервиса. Файл prometheus.yml должен быть настроен на обнаружение целей (scrape targets).

Jaeger (порт 16686 – UI, 4317 – OTLP) принимает трассировки и предоставляет удобный интерфейс для их анализа.

Grafana (порт 3000) используется для создания дашбордов на основе данных из Prometheus. Логин по умолчанию admin, пароль задаётся переменной GF_SECURITY_ADMIN_PASSWORD (в примере admin).

Доступ к интерфейсам
После запуска docker-compose up:

Jaeger UI: http://localhost:16686

Prometheus: http://localhost:9090

Seq : http://localhost:8080

Grafana: http://localhost:3000 (логин admin, пароль admin)

Для Prometheus настройки описаны в prometheus.yml
#!/bin/bash

# Скрипт для применения миграции из SQL файла (PostgreSQL)
# Использование: ./apply-migration-sql.sh /path/to/project [имя_миграции]

# Проверка наличия аргумента
if [ $# -eq 0 ]; then
    echo "Ошибка: Укажите путь к проекту"
    echo "Использование: $0 /путь/к/проекту [имя_миграции]"
    exit 1
fi

PROJECT_PATH=$1
MIGRATION_NAME=$2

# Проверка существования пути
if [ ! -d "$PROJECT_PATH" ]; then
    echo "Ошибка: Путь '$PROJECT_PATH' не существует"
    exit 1
fi

# Переход в директорию проекта
cd "$PROJECT_PATH" || exit 1

# Проверка наличия .csproj файла
if ! ls *.csproj 1> /dev/null 2>&1; then
    echo "Ошибка: В директории '$PROJECT_PATH' не найден .csproj файл"
    exit 1
fi

# Проверка наличия appsettings.json
if [ ! -f "appsettings.json" ]; then
    echo "Ошибка: Файл appsettings.json не найден в проекте"
    exit 1
fi

# Чтение строки подключения из appsettings.json
CONNECTION_STRING=$(grep -A1 '"DefaultConnection"' appsettings.json | grep -o '"[^"]*Host[^"]*"' | tail -1 | tr -d '"')

if [ -z "$CONNECTION_STRING" ]; then
    # Альтернативный способ чтения через sed
    CONNECTION_STRING=$(sed -n '/"DefaultConnection"/{n;s/.*"\(.*\)".*/\1/p;}' appsettings.json)
fi

if [ -z "$CONNECTION_STRING" ]; then
    echo "Ошибка: Не найдена строка подключения 'DefaultConnection' в appsettings.json"
    exit 1
fi

echo "Строка подключения прочитана из appsettings.json"

# Парсинг параметров подключения из строки
parse_connection_string() {
    local conn_string=$1
    
    # Извлечение Host
    DB_HOST=$(echo "$conn_string" | grep -oP 'Host=\K[^;]+' || echo "")
    
    # Извлечение Port
    DB_PORT=$(echo "$conn_string" | grep -oP 'Port=\K[^;]+' || echo "5432")
    
    # Извлечение Database
    DB_NAME=$(echo "$conn_string" | grep -oP 'Database=\K[^;]+' || echo "")
    
    # Извлечение Username
    DB_USER=$(echo "$conn_string" | grep -oP 'Username=\K[^;]+' || echo "")
    
    # Извлечение Password
    DB_PASSWORD=$(echo "$conn_string" | grep -oP 'Password=\K[^;]+' || echo "")
    
    # Проверка альтернативных имен параметров
    if [ -z "$DB_HOST" ]; then
        DB_HOST=$(echo "$conn_string" | grep -oP 'Server=\K[^;]+' || echo "localhost")
    fi
    
    if [ -z "$DB_USER" ]; then
        DB_USER=$(echo "$conn_string" | grep -oP 'User ID=\K[^;]+' || echo "postgres")
    fi
    
    if [ -z "$DB_PASSWORD" ]; then
        DB_PASSWORD=$(echo "$conn_string" | grep -oP 'Password=\K[^;]+' || echo "")
    fi
}

# Парсим строку подключения
parse_connection_string "$CONNECTION_STRING"

# Проверка обязательных параметров
if [ -z "$DB_NAME" ]; then
    echo "Ошибка: Не удалось извлечь имя базы данных из строки подключения"
    exit 1
fi

# Установка значений по умолчанию для отсутствующих параметров
DB_HOST=${DB_HOST:-localhost}
DB_PORT=${DB_PORT:-5432}
DB_USER=${DB_USER:-postgres}

echo "Параметры подключения:"
echo "  Хост: $DB_HOST"
echo "  Порт: $DB_PORT"
echo "  База данных: $DB_NAME"
echo "  Пользователь: $DB_USER"
echo "  Пароль: [скрыт]"

# Определение SQL файла
if [ -z "$MIGRATION_NAME" ]; then
    # Ищем последний SQL файл
    if [ -d "Migrations/SQL" ]; then
        SQL_FILE=$(ls -t Migrations/SQL/*.sql 2>/dev/null | grep -v "_Rollback" | grep -v "FullMigration" | head -1)
        if [ -z "$SQL_FILE" ]; then
            echo "Ошибка: Не найдено SQL файлов в директории Migrations/SQL"
            exit 1
        fi
    else
        echo "Ошибка: Директория Migrations/SQL не существует"
        exit 1
    fi
else
    SQL_FILE="Migrations/SQL/${MIGRATION_NAME}.sql"
    if [ ! -f "$SQL_FILE" ]; then
        # Пробуем найти файл без учета регистра
        SQL_FILE=$(find Migrations/SQL -iname "${MIGRATION_NAME}.sql" 2>/dev/null | head -1)
        if [ -z "$SQL_FILE" ]; then
            echo "Ошибка: Файл $SQL_FILE не найден"
            echo "Доступные SQL файлы:"
            ls -1 Migrations/SQL/*.sql 2>/dev/null | xargs -n 1 basename
            exit 1
        fi
    fi
fi

echo ""
echo "Применение SQL файла: $SQL_FILE"
echo ""

# Проверка наличия psql
if ! command -v psql &> /dev/null; then
    echo "Ошибка: psql не установлен"
    echo "Установите postgresql-client: sudo apt-get install postgresql-client"
    exit 1
fi

# Проверка подключения к базе данных
echo "Проверка подключения к базе данных..."
if [ -z "$DB_PASSWORD" ]; then
    PGPASSWORD="" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1" &> /dev/null
else
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "SELECT 1" &> /dev/null
fi

if [ $? -ne 0 ]; then
    echo "❌ Ошибка: Не удалось подключиться к базе данных"
    echo "Проверьте параметры подключения в appsettings.json"
    exit 1
fi

echo "✅ Подключение успешно"

# Применение SQL
echo ""
echo "Применение миграции..."

if [ -z "$DB_PASSWORD" ]; then
    PGPASSWORD="" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$SQL_FILE"
else
    PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -f "$SQL_FILE"
fi

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ Миграция успешно применена"
    
    # Обновление истории миграций в таблице __EFMigrationsHistory
    echo ""
    echo "Обновление истории миграций..."
    
    # Получаем имя миграции из имени файла
    MIGRATION_ID=$(basename "$SQL_FILE" .sql)
    
    # Получаем версию EF Core
    EF_VERSION=$(dotnet ef --version 2>/dev/null | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
    if [ -z "$EF_VERSION" ]; then
        EF_VERSION="8.0.0" # Значение по умолчанию
    fi
    
    # Добавляем запись в историю миграций
    INSERT_SQL="INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('$MIGRATION_ID', '$EF_VERSION') ON CONFLICT (\"MigrationId\") DO NOTHING;"
    
    if [ -z "$DB_PASSWORD" ]; then
        PGPASSWORD="" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "$INSERT_SQL" 2>/dev/null
    else
        PGPASSWORD="$DB_PASSWORD" psql -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" -c "$INSERT_SQL" 2>/dev/null
    fi
    
    if [ $? -eq 0 ]; then
        echo "✅ История миграций обновлена"
    else
        echo "⚠️ Не удалось обновить историю миграций (таблица может отсутствовать)"
    fi
else
    echo ""
    echo "❌ Ошибка при применении миграции"
    exit 1
fi

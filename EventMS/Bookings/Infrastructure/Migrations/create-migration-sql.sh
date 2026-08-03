#!/bin/bash

# Скрипт для создания миграции EF Core и генерации SQL скрипта
# Использование: ./create-migration-sql.sh /path/to/project [имя_миграции]

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

# Если имя миграции не указано, генерируем автоматически
if [ -z "$MIGRATION_NAME" ]; then
    MIGRATION_NAME="Migration_$(date +%Y%m%d_%H%M%S)"
fi

echo "Создание миграции в проекте: $PROJECT_PATH"
echo "Имя миграции: $MIGRATION_NAME"

# Создание миграции
dotnet ef migrations add "$MIGRATION_NAME"

# Проверка результата создания миграции
if [ $? -ne 0 ]; then
    echo "❌ Ошибка при создании миграции"
    exit 1
fi

echo "✅ Миграция '$MIGRATION_NAME' успешно создана"

# Генерация SQL скрипта
echo "Генерация SQL скрипта для миграции..."

# Создаем директорию для SQL скриптов если её нет
mkdir -p Migrations/SQL

# Имя SQL файла
SQL_FILE="Migrations/SQL/${MIGRATION_NAME}.sql"

# Генерация SQL скрипта (только для этой миграции)
dotnet ef migrations script \
    "$MIGRATION_NAME" \
    --output "$SQL_FILE" \
    --idempotent

# Проверка результата генерации SQL
if [ $? -eq 0 ]; then
    echo "✅ SQL скрипт создан: $SQL_FILE"
    
    # Показываем размер файла
    FILE_SIZE=$(du -h "$SQL_FILE" | cut -f1)
    echo "Размер файла: $FILE_SIZE"
else
    echo "❌ Ошибка при генерации SQL скрипта"
    exit 1
fi

# Дополнительно создаем скрипт для отката (Down)
echo "Генерация SQL скрипта для отката..."

ROLLBACK_FILE="Migrations/SQL/${MIGRATION_NAME}_Rollback.sql"

# Генерация скрипта отката
dotnet ef migrations script \
    "$(dotnet ef migrations list | grep -v "\[ \]" | head -1 | awk '{print $1}')" \
    "$MIGRATION_NAME" \
    --output "$ROLLBACK_FILE" \
    --idempotent 2>/dev/null

if [ $? -eq 0 ] && [ -f "$ROLLBACK_FILE" ]; then
    echo "✅ SQL скрипт отката создан: $ROLLBACK_FILE"
else
    echo "⚠️ Не удалось создать скрипт отката (возможно это первая миграция)"
    rm -f "$ROLLBACK_FILE" 2>/dev/null
fi

echo ""
echo "📁 Файлы миграции:"
echo "  - Миграция C#: Migrations/*${MIGRATION_NAME}*.cs"
echo "  - SQL скрипт: $SQL_FILE"
[ -f "$ROLLBACK_FILE" ] && echo "  - SQL откат: $ROLLBACK_FILE"
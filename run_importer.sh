#!/bin/bash

# ============================================
# Запуск Importer (консольное приложение)
# Логи: /var/log/nbsite/importer_YYYYMMDD.log
# ============================================

# Путь к опубликованному приложению
APP_DIR="/root/NBSite/Importer/bin/Debug/net10.0"
APP_DLL="Importer.dll"

# Директория для логов
LOG_DIR="/var/log/nbsite"
LOG_FILE="$LOG_DIR/importer_$(date +\%Y\%m\%d).log"

# Путь к dotnet 10 (симлинк)
DOTNET="/usr/local/bin/dotnet10"

# Создаём директорию для логов, если её нет
mkdir -p "$LOG_DIR"

# Переходим в папку с приложением
cd "$APP_DIR" || {
    echo "[ERROR] Не удалось перейти в директорию $APP_DIR" >> "$LOG_FILE"
    exit 1
}

# Запускаем приложение, добавляем временную метку к каждой строке вывода
"$DOTNET" "$APP_DLL" 2>&1 | while IFS= read -r line; do
    echo "$(date '+%Y-%m-%d %H:%M:%S') $line"
done >> "$LOG_FILE"

# Сохраняем код возврата dotnet
exit ${PIPESTATUS[0]}
#!/bin/bash

# ============================================
# Запуск Exporter (консольное приложение)
# Логи: /var/log/nbsite/exporter_YYYYMMDD.log
# ============================================

APP_DIR="/root/NBSite/Exporter/bin/Debug/net10.0"
APP_DLL="Exporter.dll"
LOG_DIR="/var/log/nbsite"
LOG_FILE="$LOG_DIR/exporter_$(date +\%Y\%m\%d).log"
DOTNET="/usr/local/bin/dotnet10"

mkdir -p "$LOG_DIR"

cd "$APP_DIR" || {
    echo "[ERROR] Не удалось перейти в директорию $APP_DIR" >> "$LOG_FILE"
    exit 1
}

"$DOTNET" "$APP_DLL" 2>&1 | while IFS= read -r line; do
    echo "$(date '+%Y-%m-%d %H:%M:%S') $line"
done >> "$LOG_FILE"

exit ${PIPESTATUS[0]}
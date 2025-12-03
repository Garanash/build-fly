#!/bin/bash

# Скрипт для автоматической сборки игры Unity

echo "=== Сборка игры Unity Drone Assembly & Flight Simulator ==="

# Путь к Unity (обычно в Applications на macOS)
UNITY_PATH="/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"

# Находим последнюю версию Unity
LATEST_UNITY=$(ls -td /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity 2>/dev/null | head -1)

if [ -z "$LATEST_UNITY" ]; then
    echo "❌ Unity не найден. Установите Unity Hub и Unity Editor."
    echo "Скачайте с: https://unity.com/download"
    exit 1
fi

echo "✅ Найден Unity: $LATEST_UNITY"

# Путь к проекту
PROJECT_PATH="$(cd "$(dirname "$0")" && pwd)"
BUILD_PATH="$PROJECT_PATH/Build"

echo "📁 Проект: $PROJECT_PATH"
echo "📦 Папка сборки: $BUILD_PATH"

# Создаем папку для сборки
mkdir -p "$BUILD_PATH"

# Запускаем сборку
echo "🔨 Начинаю сборку..."

"$LATEST_UNITY" \
    -quit \
    -batchmode \
    -projectPath "$PROJECT_PATH" \
    -buildTarget StandaloneOSX \
    -buildPath "$BUILD_PATH/DroneGame.app" \
    -executeMethod BuildScript.BuildGame \
    -logFile "$BUILD_PATH/build.log"

if [ $? -eq 0 ]; then
    echo "✅ Сборка завершена успешно!"
    echo "📦 Игра находится в: $BUILD_PATH/DroneGame.app"
    echo "🚀 Для запуска: open $BUILD_PATH/DroneGame.app"
else
    echo "❌ Ошибка при сборке. Проверьте лог: $BUILD_PATH/build.log"
    exit 1
fi


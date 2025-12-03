#!/bin/bash

# Скрипт для запуска игры как для конечного пользователя

echo "🚁 Запуск игры Unity Drone Assembly & Flight Simulator"
echo "=================================================="

# Проверяем наличие собранной версии
BUILD_PATH="./Build/DroneGame.app"

if [ -d "$BUILD_PATH" ]; then
    echo "✅ Найдена собранная версия игры!"
    echo "🚀 Запускаю игру..."
    open "$BUILD_PATH"
    echo "✅ Игра запущена!"
    exit 0
fi

# Если нет собранной версии, ищем Unity для сборки
echo "📦 Собранная версия не найдена. Ищу Unity для сборки..."

# Стандартные пути к Unity на macOS
UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity.app/Contents/MacOS/Unity"
)

UNITY_FOUND=""

for path_pattern in "${UNITY_PATHS[@]}"; do
    for unity_path in $path_pattern; do
        if [ -f "$unity_path" ]; then
            UNITY_FOUND="$unity_path"
            break 2
        fi
    done
done

if [ -z "$UNITY_FOUND" ]; then
    echo "❌ Unity не найден!"
    echo ""
    echo "Для запуска игры вам нужно:"
    echo "1. Установить Unity Hub: https://unity.com/download"
    echo "2. Установить Unity Editor 2020.3 или новее"
    echo "3. Открыть проект в Unity"
    echo "4. Создать сцену DroneAssembly и добавить GameBootstrap"
    echo "5. Нажать Play ▶️"
    echo ""
    echo "Или следуйте инструкциям в файле LAUNCH_GAME.md"
    exit 1
fi

echo "✅ Найден Unity: $UNITY_FOUND"
echo "📦 Начинаю сборку игры..."

# Создаем папку для сборки
mkdir -p "./Build"

# Запускаем сборку через Unity
"$UNITY_FOUND" \
    -quit \
    -batchmode \
    -projectPath "$(pwd)" \
    -buildTarget StandaloneOSX \
    -buildPath "./Build/DroneGame.app" \
    -executeMethod BuildScript.BuildGame \
    -logFile "./Build/build.log" \
    2>&1 | tee "./Build/build_output.log"

if [ $? -eq 0 ] && [ -d "./Build/DroneGame.app" ]; then
    echo ""
    echo "✅ Сборка завершена успешно!"
    echo "🚀 Запускаю игру..."
    open "./Build/DroneGame.app"
    echo "✅ Игра запущена!"
else
    echo ""
    echo "❌ Ошибка при сборке. Проверьте логи:"
    echo "   - ./Build/build.log"
    echo "   - ./Build/build_output.log"
    echo ""
    echo "Альтернативный способ запуска:"
    echo "1. Откройте проект в Unity Editor"
    echo "2. Создайте сцену DroneAssembly"
    echo "3. Добавьте GameBootstrap компонент"
    echo "4. Нажмите Play ▶️"
    exit 1
fi


#!/bin/bash

# Скрипт для автоматической сборки и запуска игры
# Работает как для разработчиков, так и для конечных пользователей

set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$PROJECT_DIR/Build"
APP_NAME="DroneGame"
APP_PATH="$BUILD_DIR/${APP_NAME}.app"

echo "🚁 Unity Drone Assembly & Flight Simulator"
echo "=========================================="
echo ""

# Проверяем наличие готовой сборки
if [ -d "$APP_PATH" ]; then
    echo "✅ Найдена готовая сборка игры!"
    echo "🚀 Запускаю игру..."
    open "$APP_PATH"
    echo "✅ Игра запущена!"
    exit 0
fi

echo "📦 Готовая сборка не найдена."
echo ""

# Ищем Unity
echo "🔍 Ищу Unity для сборки..."

UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor"/*/Unity.app/Contents/MacOS/Unity
    "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity.app/Contents/MacOS/Unity"
    "$HOME/Applications/Unity/Hub/Editor"/*/Unity.app/Contents/MacOS/Unity
)

UNITY_FOUND=""

for path_pattern in "${UNITY_PATHS[@]}"; do
    for unity_path in $path_pattern; do
        if [ -f "$unity_path" ] && [ -x "$unity_path" ]; then
            UNITY_FOUND="$unity_path"
            break 2
        fi
    done
done

if [ -z "$UNITY_FOUND" ]; then
    echo "❌ Unity не найден на этом компьютере."
    echo ""
    echo "📋 Для сборки игры вам нужно:"
    echo ""
    echo "1. Установить Unity Hub:"
    echo "   https://unity.com/download"
    echo ""
    echo "2. Установить Unity Editor 2020.3 LTS или новее"
    echo ""
    echo "3. Запустить этот скрипт снова:"
    echo "   ./build_and_run.sh"
    echo ""
    echo "📖 Или следуйте инструкциям в INSTALL_AND_RUN.md"
    echo ""
    echo "💡 После первой сборки игра будет работать без Unity!"
    exit 1
fi

echo "✅ Найден Unity: $UNITY_FOUND"
echo ""

# Проверяем наличие сцен
SCENES_DIR="$PROJECT_DIR/Assets/Scenes"
DRONE_ASSEMBLY_SCENE="$SCENES_DIR/DroneAssembly.unity"
FLIGHT_SIMULATOR_SCENE="$SCENES_DIR/FlightSimulator.unity"

if [ ! -f "$DRONE_ASSEMBLY_SCENE" ] || [ ! -f "$FLIGHT_SIMULATOR_SCENE" ]; then
    echo "⚠️  Сцены не найдены. Создаю их автоматически..."
    
    # Создаем папку для сцен
    mkdir -p "$SCENES_DIR"
    
    # Создаем временный скрипт для создания сцен
    cat > "$PROJECT_DIR/create_scenes.cs" << 'EOF'
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CreateScenes : Editor
{
    [MenuItem("Tools/Create Game Scenes")]
    public static void CreateGameScenes()
    {
        if (!System.IO.Directory.Exists("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        }
        
        // Создаем сцену сборки
        var assemblyScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        EditorSceneManager.SaveScene(assemblyScene, "Assets/Scenes/DroneAssembly.unity");
        
        // Создаем сцену симулятора
        var flightScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        EditorSceneManager.SaveScene(flightScene, "Assets/Scenes/FlightSimulator.unity");
        
        AssetDatabase.Refresh();
        Debug.Log("Сцены созданы!");
    }
}
EOF
    
    echo "📝 Запускаю Unity для создания сцен..."
    
    "$UNITY_FOUND" \
        -quit \
        -batchmode \
        -projectPath "$PROJECT_DIR" \
        -executeMethod CreateScenes.CreateGameScenes \
        -logFile "$BUILD_DIR/create_scenes.log" \
        2>&1 | head -20
    
    # Удаляем временный скрипт
    rm -f "$PROJECT_DIR/create_scenes.cs"
    
    if [ ! -f "$DRONE_ASSEMBLY_SCENE" ] || [ ! -f "$FLIGHT_SIMULATOR_SCENE" ]; then
        echo "⚠️  Не удалось создать сцены автоматически."
        echo "   Пожалуйста, создайте сцены вручную в Unity Editor."
    fi
fi

# Собираем игру
echo ""
echo "🔨 Начинаю сборку игры..."
echo "   Это может занять несколько минут..."
echo ""

mkdir -p "$BUILD_DIR"

"$UNITY_FOUND" \
    -quit \
    -batchmode \
    -projectPath "$PROJECT_DIR" \
    -buildTarget StandaloneOSX \
    -buildPath "$APP_PATH" \
    -executeMethod BuildScript.BuildGame \
    -logFile "$BUILD_DIR/build.log" \
    2>&1 | tee "$BUILD_DIR/build_output.log" | grep -E "(Building|Error|Warning|Finished)" || true

# Проверяем результат
if [ -d "$APP_PATH" ]; then
    echo ""
    echo "✅ Сборка завершена успешно!"
    echo "📦 Игра находится в: $APP_PATH"
    echo ""
    echo "🚀 Запускаю игру..."
    open "$APP_PATH"
    echo "✅ Игра запущена!"
    echo ""
    echo "💡 Теперь вы можете запускать игру без Unity:"
    echo "   open $APP_PATH"
    echo "   или просто двойной клик по DroneGame.app в Finder"
else
    echo ""
    echo "❌ Ошибка при сборке."
    echo ""
    echo "📋 Проверьте логи:"
    echo "   - $BUILD_DIR/build.log"
    echo "   - $BUILD_DIR/build_output.log"
    echo ""
    echo "💡 Попробуйте:"
    echo "   1. Открыть проект в Unity Editor"
    echo "   2. Создать сцены вручную (File > New Scene, сохранить как DroneAssembly и FlightSimulator)"
    echo "   3. File > Build Settings > Build"
    exit 1
fi


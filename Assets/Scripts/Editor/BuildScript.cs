using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Editor
{
    /// <summary>
    /// Скрипт для автоматической сборки игры
    /// </summary>
    public class BuildScript
    {
        [MenuItem("Build/Build Game")]
        public static void BuildGame()
        {
            Debug.Log("Начинаю сборку игры...");

            // Получаем все сцены проекта
            string[] scenes = {
                "Assets/Scenes/DroneAssembly.unity",
                "Assets/Scenes/FlightSimulator.unity"
            };

            // Проверяем существование сцен
            System.Collections.Generic.List<string> validScenes = new System.Collections.Generic.List<string>();
            foreach (string scene in scenes)
            {
                if (System.IO.File.Exists(scene))
                {
                    validScenes.Add(scene);
                    Debug.Log($"Найдена сцена: {scene}");
                }
                else
                {
                    Debug.LogWarning($"Сцена не найдена: {scene}");
                }
            }

            // Если сцен нет, создаем их автоматически
            if (validScenes.Count == 0)
            {
                Debug.Log("Создаю сцены автоматически...");
                CreateScenesIfNeeded();
                validScenes.Add("Assets/Scenes/DroneAssembly.unity");
                validScenes.Add("Assets/Scenes/FlightSimulator.unity");
            }

            // Путь для сборки
            string buildPath = "Build/DroneGame";

            #if UNITY_STANDALONE_OSX
                buildPath += ".app";
            #elif UNITY_STANDALONE_WIN
                buildPath += ".exe";
            #elif UNITY_STANDALONE_LINUX
                buildPath += ".x86_64";
            #endif

            // Собираем игру
            BuildPipeline.BuildPlayer(
                validScenes.ToArray(),
                buildPath,
                EditorUserBuildSettings.activeBuildTarget,
                BuildOptions.None
            );

            Debug.Log($"Сборка завершена: {buildPath}");
        }

        private static void CreateScenesIfNeeded()
        {
            // Создаем папку для сцен, если её нет
            if (!System.IO.Directory.Exists("Assets/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/Scenes");
                AssetDatabase.Refresh();
            }

            // Создаем сцену сборки
            if (!System.IO.File.Exists("Assets/Scenes/DroneAssembly.unity"))
            {
                var assemblyScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                EditorSceneManager.SaveScene(assemblyScene, "Assets/Scenes/DroneAssembly.unity");
                Debug.Log("Создана сцена: DroneAssembly");
            }

            // Создаем сцену симулятора
            if (!System.IO.File.Exists("Assets/Scenes/FlightSimulator.unity"))
            {
                var flightScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
                EditorSceneManager.SaveScene(flightScene, "Assets/Scenes/FlightSimulator.unity");
                Debug.Log("Создана сцена: FlightSimulator");
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("Build/Build And Run")]
        public static void BuildAndRun()
        {
            BuildGame();
            
            #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                System.Diagnostics.Process.Start("Build/DroneGame" + 
                    #if UNITY_STANDALONE_OSX
                        ".app"
                    #elif UNITY_STANDALONE_WIN
                        ".exe"
                    #endif
                );
            #endif
        }
    }
}


using UnityEngine;
using AutoSetup;

/// <summary>
/// Главный скрипт для автоматической инициализации игры
/// Добавьте этот компонент на пустой GameObject в каждой сцене
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private bool autoSetup = true;
    [SerializeField] private bool showInstructions = true;

    private void Start()
    {
        if (autoSetup)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            Debug.Log($"=== Запуск игры в сцене: {sceneName} ===");

            if (sceneName == "DroneAssembly" || sceneName.Contains("Assembly"))
            {
                SetupAssemblyScene();
            }
            else if (sceneName == "FlightSimulator" || sceneName.Contains("Flight"))
            {
                SetupFlightScene();
            }
            else
            {
                // Пытаемся определить тип сцены автоматически
                if (FindObjectOfType<DroneAssembly.DroneAssemblyManager>() != null)
                {
                    SetupAssemblyScene();
                }
                else if (FindObjectOfType<FlightSimulator.DroneController>() != null)
                {
                    SetupFlightScene();
                }
                else
                {
                    Debug.LogWarning("Не удалось определить тип сцены. Создаю сцену сборки по умолчанию.");
                    SetupAssemblyScene();
                }
            }

            if (showInstructions)
            {
                ShowInstructions();
            }
        }
    }

    private void SetupAssemblyScene()
    {
        AutoAssemblySetup setup = FindObjectOfType<AutoAssemblySetup>();
        if (setup == null)
        {
            GameObject setupObj = new GameObject("AutoAssemblySetup");
            setup = setupObj.AddComponent<AutoAssemblySetup>();
        }
        setup.SetupAssemblyScene();
    }

    private void SetupFlightScene()
    {
        AutoFlightSetup setup = FindObjectOfType<AutoFlightSetup>();
        if (setup == null)
        {
            GameObject setupObj = new GameObject("AutoFlightSetup");
            setup = setupObj.AddComponent<AutoFlightSetup>();
        }
        setup.SetupFlightScene();
    }

    private void ShowInstructions()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        if (sceneName == "DroneAssembly" || sceneName.Contains("Assembly"))
        {
            Debug.Log("=== ИНСТРУКЦИЯ: СЦЕНА СБОРКИ ===");
            Debug.Log("1. Перетащите детали на правильные места на раме");
            Debug.Log("2. Когда все детали установлены, нажмите кнопку 'Запустить полет'");
            Debug.Log("3. Используйте мышь для перетаскивания деталей");
        }
        else
        {
            Debug.Log("=== ИНСТРУКЦИЯ: СИМУЛЯТОР ПОЛЕТА ===");
            Debug.Log("Управление:");
            Debug.Log("  Space - Увеличить газ");
            Debug.Log("  Shift - Уменьшить газ");
            Debug.Log("  W/S - Наклон вперед/назад");
            Debug.Log("  A/D - Наклон влево/вправо");
            Debug.Log("  Q/E - Поворот влево/вправо");
            Debug.Log("  R - Сбросить дрон");
            Debug.Log("  Esc - Вернуться к сборке");
            Debug.Log("  Tab - Показать/скрыть панель управления");
        }
    }
}


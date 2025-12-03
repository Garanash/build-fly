using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер для переключения между сценами
/// </summary>
public class GameSceneManager : MonoBehaviour
{
    /// <summary>
    /// Загружает сцену сборки
    /// </summary>
    public void LoadAssemblyScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DroneAssembly");
    }

    /// <summary>
    /// Загружает сцену симулятора полета
    /// </summary>
    public void LoadFlightSimulatorScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("FlightSimulator");
    }

    /// <summary>
    /// Перезагружает текущую сцену
    /// </summary>
    public void ReloadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    /// <summary>
    /// Выходит из игры
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}


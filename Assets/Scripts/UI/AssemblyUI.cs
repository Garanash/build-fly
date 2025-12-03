using UnityEngine;
using UnityEngine.UI;
using DroneAssembly;

/// <summary>
/// UI контроллер для сцены сборки
/// </summary>
public class AssemblyUI : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private Button startFlightButton;
    [SerializeField] private Text progressText;
    [SerializeField] private GameObject instructionPanel;

    private DroneAssemblyManager assemblyManager;

    private void Start()
    {
        assemblyManager = DroneAssemblyManager.Instance;

        if (startFlightButton != null)
        {
            startFlightButton.onClick.AddListener(OnStartFlightClicked);
            startFlightButton.interactable = false;
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    /// <summary>
    /// Обновляет UI
    /// </summary>
    private void UpdateUI()
    {
        if (assemblyManager == null) return;

        var (installed, total, complete) = assemblyManager.GetAssemblyProgress();

        if (progressText != null)
        {
            progressText.text = $"Прогресс сборки: {installed} / {total}";
        }

        if (startFlightButton != null)
        {
            startFlightButton.interactable = complete;
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки запуска полета
    /// </summary>
    private void OnStartFlightClicked()
    {
        if (assemblyManager != null)
        {
            assemblyManager.StartFlightSimulator();
        }
    }

    /// <summary>
    /// Показывает/скрывает панель инструкций
    /// </summary>
    public void ToggleInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(!instructionPanel.activeSelf);
        }
    }
}


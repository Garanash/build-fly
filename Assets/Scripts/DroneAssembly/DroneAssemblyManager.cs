using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DroneAssembly
{
    /// <summary>
    /// Менеджер управления процессом сборки квадрокоптера
    /// </summary>
    public class DroneAssemblyManager : MonoBehaviour
    {
        public static DroneAssemblyManager Instance { get; private set; }

        [Header("Настройки сборки")]
        [SerializeField] private List<PartSlot> allSlots = new List<PartSlot>();
        [SerializeField] private List<PartType> requiredParts = new List<PartType>();

        [Header("UI")]
        [SerializeField] private GameObject assemblyCompleteUI;
        [SerializeField] private GameObject assemblyIncompleteUI;
        [SerializeField] private UnityEngine.UI.Text statusText;

        private Dictionary<PartType, bool> installedParts = new Dictionary<PartType, bool>();
        private bool isAssemblyComplete = false;

        private void OnEnable()
        {
            InitializeRequiredParts();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeRequiredParts();
        }

        private void Start()
        {
            // Находим все слоты автоматически, если не назначены вручную
            if (allSlots.Count == 0)
            {
                allSlots.AddRange(FindObjectsOfType<PartSlot>());
            }

            UpdateUI();
        }

        /// <summary>
        /// Инициализирует список необходимых деталей
        /// </summary>
        private void InitializeRequiredParts()
        {
            requiredParts = new List<PartType>
            {
                PartType.Frame,
                PartType.Motor1,
                PartType.Motor2,
                PartType.Motor3,
                PartType.Motor4,
                PartType.Propeller1,
                PartType.Propeller2,
                PartType.Propeller3,
                PartType.Propeller4,
                PartType.Battery,
                PartType.FlightController
            };

            foreach (var partType in requiredParts)
            {
                installedParts[partType] = false;
            }
        }

        /// <summary>
        /// Вызывается при установке детали
        /// </summary>
        public void OnPartInstalled(PartSlot slot)
        {
            if (slot.IsOccupied && slot.InstalledPart != null)
            {
                installedParts[slot.RequiredPartType] = true;
                CheckAssemblyComplete();
                UpdateUI();
            }
        }

        /// <summary>
        /// Вызывается при удалении детали
        /// </summary>
        public void OnPartRemoved(PartSlot slot)
        {
            if (slot.RequiredPartType != PartType.None)
            {
                installedParts[slot.RequiredPartType] = false;
                isAssemblyComplete = false;
                CheckAssemblyComplete();
                UpdateUI();
            }
        }

        /// <summary>
        /// Проверяет, завершена ли сборка
        /// </summary>
        private void CheckAssemblyComplete()
        {
            isAssemblyComplete = true;

            foreach (var partType in requiredParts)
            {
                if (!installedParts.ContainsKey(partType) || !installedParts[partType])
                {
                    isAssemblyComplete = false;
                    break;
                }
            }

            if (isAssemblyComplete)
            {
                Debug.Log("Сборка квадрокоптера завершена!");
            }
        }

        /// <summary>
        /// Обновляет UI
        /// </summary>
        private void UpdateUI()
        {
            if (assemblyCompleteUI != null)
            {
                assemblyCompleteUI.SetActive(isAssemblyComplete);
            }

            if (assemblyIncompleteUI != null)
            {
                assemblyIncompleteUI.SetActive(!isAssemblyComplete);
            }

            if (statusText != null)
            {
                int installedCount = 0;
                foreach (var installed in installedParts.Values)
                {
                    if (installed) installedCount++;
                }

                statusText.text = $"Установлено деталей: {installedCount} / {requiredParts.Count}";
            }
        }

        /// <summary>
        /// Запускает симулятор полета, если сборка завершена
        /// </summary>
        public void StartFlightSimulator()
        {
            if (isAssemblyComplete)
            {
                Debug.Log("Запуск симулятора полета...");
                SceneManager.LoadScene("FlightSimulator");
            }
            else
            {
                Debug.LogWarning("Сборка не завершена! Установите все необходимые детали.");
            }
        }

        /// <summary>
        /// Получает информацию о прогрессе сборки
        /// </summary>
        public (int installed, int total, bool complete) GetAssemblyProgress()
        {
            int installed = 0;
            foreach (var installedPart in installedParts.Values)
            {
                if (installedPart) installed++;
            }

            return (installed, requiredParts.Count, isAssemblyComplete);
        }
    }
}


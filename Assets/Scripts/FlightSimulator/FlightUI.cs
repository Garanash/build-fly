using UnityEngine;
using UnityEngine.UI;

namespace FlightSimulator
{
    /// <summary>
    /// UI для симулятора полета
    /// </summary>
    public class FlightUI : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private DroneController droneController;
        [SerializeField] private Text throttleText;
        [SerializeField] private Text altitudeText;
        [SerializeField] private Text speedText;
        [SerializeField] private GameObject controlsPanel;

        private Transform droneTransform;
        private Rigidbody droneRigidbody;

        private void Start()
        {
            if (droneController != null)
            {
                droneTransform = droneController.transform;
                droneRigidbody = droneController.GetComponent<Rigidbody>();
            }
        }

        private void Update()
        {
            UpdateUI();
            HandleInput();
        }

        /// <summary>
        /// Обновляет UI элементы
        /// </summary>
        private void UpdateUI()
        {
            if (droneController != null && throttleText != null)
            {
                float throttle = droneController.GetThrottle();
                throttleText.text = $"Газ: {(throttle * 100f):F0}%";
            }

            if (droneTransform != null && altitudeText != null)
            {
                float altitude = droneTransform.position.y;
                altitudeText.text = $"Высота: {altitude:F1} м";
            }

            if (droneRigidbody != null && speedText != null)
            {
                float speed = droneRigidbody.velocity.magnitude;
                speedText.text = $"Скорость: {speed:F1} м/с";
            }
        }

        /// <summary>
        /// Обрабатывает ввод для UI
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (controlsPanel != null)
                {
                    controlsPanel.SetActive(!controlsPanel.activeSelf);
                }
            }
        }
    }
}


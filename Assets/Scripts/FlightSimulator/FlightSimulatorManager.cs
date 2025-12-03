using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlightSimulator
{
    /// <summary>
    /// Менеджер симулятора полета
    /// </summary>
    public class FlightSimulatorManager : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private bool autoStart = false;
        [SerializeField] private float resetHeight = -10f; // Высота, при которой сбрасывается дрон

        [Header("Ссылки")]
        [SerializeField] private DroneController droneController;
        [SerializeField] private Transform spawnPoint;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Rigidbody droneRigidbody;

        private void Start()
        {
            if (droneController != null)
            {
                droneRigidbody = droneController.GetComponent<Rigidbody>();
                initialPosition = spawnPoint != null ? spawnPoint.position : droneController.transform.position;
                initialRotation = spawnPoint != null ? spawnPoint.rotation : droneController.transform.rotation;
            }

            if (autoStart)
            {
                ResetDrone();
            }
        }

        private void Update()
        {
            // Проверка на сброс дрона
            if (droneController != null && droneController.transform.position.y < resetHeight)
            {
                ResetDrone();
            }

            // Возврат в меню сборки
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToAssembly();
            }

            // Сброс дрона
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetDrone();
            }
        }

        /// <summary>
        /// Сбрасывает дрон в начальную позицию
        /// </summary>
        public void ResetDrone()
        {
            if (droneController != null)
            {
                droneController.transform.position = initialPosition;
                droneController.transform.rotation = initialRotation;

                if (droneRigidbody != null)
                {
                    droneRigidbody.velocity = Vector3.zero;
                    droneRigidbody.angularVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Возвращается к сцене сборки
        /// </summary>
        public void ReturnToAssembly()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("DroneAssembly");
        }
    }
}


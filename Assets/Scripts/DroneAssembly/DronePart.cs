using UnityEngine;

namespace DroneAssembly
{
    /// <summary>
    /// Типы деталей квадрокоптера
    /// </summary>
    public enum PartType
    {
        Frame,      // Рама
        Motor1,     // Мотор 1
        Motor2,     // Мотор 2
        Motor3,     // Мотор 3
        Motor4,     // Мотор 4
        Propeller1, // Пропеллер 1
        Propeller2, // Пропеллер 2
        Propeller3, // Пропеллер 3
        Propeller4, // Пропеллер 4
        Battery,    // Батарея
        FlightController, // Контроллер полета
        Camera      // Камера (опционально)
    }

    /// <summary>
    /// Компонент детали квадрокоптера, которую можно перетаскивать и устанавливать
    /// </summary>
    public class DronePart : MonoBehaviour
    {
        [Header("Настройки детали")]
        [SerializeField] private PartType partType;
        [SerializeField] private bool isInstalled = false;
        [SerializeField] private Vector3 originalPosition;
        [SerializeField] private Quaternion originalRotation;

        private Camera mainCamera;
        private bool isDragging = false;
        private Vector3 offset;
        private Collider partCollider;

        public PartType PartType => partType;
        public bool IsInstalled => isInstalled;

        private void Awake()
        {
            mainCamera = Camera.main;
            partCollider = GetComponent<Collider>();
            if (partCollider == null)
            {
                partCollider = gameObject.AddComponent<BoxCollider>();
            }
        }

        private void Start()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        }

        private void OnMouseDown()
        {
            if (isInstalled) return;

            isDragging = true;
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Vector3.Distance(mainCamera.transform.position, transform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            offset = transform.position - worldPos;
        }

        private void OnMouseDrag()
        {
            if (!isDragging || isInstalled) return;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Vector3.Distance(mainCamera.transform.position, transform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            transform.position = worldPos + offset;
        }

        private void OnMouseUp()
        {
            isDragging = false;
        }

        /// <summary>
        /// Устанавливает деталь в слот
        /// </summary>
        public void InstallToSlot(Transform slotTransform)
        {
            isInstalled = true;
            transform.position = slotTransform.position;
            transform.rotation = slotTransform.rotation;
            transform.SetParent(slotTransform);
            
            if (partCollider != null)
            {
                partCollider.enabled = false;
            }
        }

        /// <summary>
        /// Возвращает деталь на исходную позицию
        /// </summary>
        public void ResetPosition()
        {
            isInstalled = false;
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            transform.SetParent(null);
            
            if (partCollider != null)
            {
                partCollider.enabled = true;
            }
        }

        /// <summary>
        /// Проверяет, находится ли деталь рядом со слотом
        /// </summary>
        public bool IsNearSlot(Transform slotTransform, float threshold = 0.5f)
        {
            float distance = Vector3.Distance(transform.position, slotTransform.position);
            return distance < threshold;
        }
    }
}


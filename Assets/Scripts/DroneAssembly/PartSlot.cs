using UnityEngine;

namespace DroneAssembly
{
    /// <summary>
    /// Слот для установки детали квадрокоптера
    /// </summary>
    public class PartSlot : MonoBehaviour
    {
        [Header("Настройки слота")]
        [SerializeField] private PartType requiredPartType;
        [SerializeField] private bool isOccupied = false;
        [SerializeField] private DronePart installedPart;
        [SerializeField] private float snapDistance = 0.5f;

        private void OnEnable()
        {
            isOccupied = false;
            installedPart = null;
        }

        [Header("Визуализация")]
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Color highlightColor = Color.green;
        [SerializeField] private Color errorColor = Color.red;

        private Renderer slotRenderer;
        private Material originalMaterial;
        private Material highlightMaterial;

        public PartType RequiredPartType => requiredPartType;
        public bool IsOccupied => isOccupied;
        public DronePart InstalledPart => installedPart;

        private void Awake()
        {
            slotRenderer = GetComponent<Renderer>();
            if (slotRenderer != null)
            {
                originalMaterial = slotRenderer.material;
            }

            // Создаем материал для подсветки
            if (slotRenderer != null)
            {
                highlightMaterial = new Material(originalMaterial);
                highlightMaterial.color = highlightColor;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            DronePart part = other.GetComponent<DronePart>();
            if (part != null && !isOccupied)
            {
                if (part.PartType == requiredPartType)
                {
                    HighlightSlot(true);
                }
                else
                {
                    HighlightSlot(false);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            DronePart part = other.GetComponent<DronePart>();
            if (part != null && !isOccupied && !part.IsInstalled)
            {
                if (part.PartType == requiredPartType && part.IsNearSlot(transform, snapDistance))
                {
                    InstallPart(part);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            DronePart part = other.GetComponent<DronePart>();
            if (part != null && !isOccupied)
            {
                RemoveHighlight();
            }
        }

        /// <summary>
        /// Устанавливает деталь в слот
        /// </summary>
        public void InstallPart(DronePart part)
        {
            if (isOccupied || part.PartType != requiredPartType)
            {
                return;
            }

            installedPart = part;
            isOccupied = true;
            part.InstallToSlot(transform);
            RemoveHighlight();

            // Уведомляем менеджер сборки
            DroneAssemblyManager.Instance?.OnPartInstalled(this);
        }

        /// <summary>
        /// Удаляет деталь из слота
        /// </summary>
        public void RemovePart()
        {
            if (installedPart != null)
            {
                installedPart.ResetPosition();
                installedPart = null;
            }
            isOccupied = false;
            DroneAssemblyManager.Instance?.OnPartRemoved(this);
        }

        /// <summary>
        /// Подсвечивает слот
        /// </summary>
        private void HighlightSlot(bool isValid)
        {
            if (slotRenderer != null && highlightMaterial != null)
            {
                highlightMaterial.color = isValid ? highlightColor : errorColor;
                slotRenderer.material = highlightMaterial;
            }

            if (highlightObject != null)
            {
                highlightObject.SetActive(true);
                Renderer highlightRenderer = highlightObject.GetComponent<Renderer>();
                if (highlightRenderer != null)
                {
                    Material mat = highlightRenderer.material;
                    if (mat != null)
                    {
                        mat.color = isValid ? highlightColor : errorColor;
                    }
                }
            }
        }

        /// <summary>
        /// Убирает подсветку
        /// </summary>
        private void RemoveHighlight()
        {
            if (slotRenderer != null && originalMaterial != null)
            {
                slotRenderer.material = originalMaterial;
            }

            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (highlightMaterial != null)
            {
                Destroy(highlightMaterial);
            }
        }
    }
}


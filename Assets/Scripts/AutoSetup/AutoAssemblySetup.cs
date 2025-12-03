using UnityEngine;
using DroneAssembly;

namespace AutoSetup
{
    /// <summary>
    /// Автоматически создает и настраивает сцену сборки при запуске
    /// </summary>
    public class AutoAssemblySetup : MonoBehaviour
    {
        [Header("Автоматическая настройка")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createIfNotExists = true;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupAssemblyScene();
            }
        }

        /// <summary>
        /// Настраивает сцену сборки
        /// </summary>
        [ContextMenu("Setup Assembly Scene")]
        public void SetupAssemblyScene()
        {
            Debug.Log("Начинаю автоматическую настройку сцены сборки...");

            // Создаем менеджер сборки
            DroneAssemblyManager manager = FindObjectOfType<DroneAssemblyManager>();
            if (manager == null && createIfNotExists)
            {
                GameObject managerObj = new GameObject("AssemblyManager");
                manager = managerObj.AddComponent<DroneAssemblyManager>();
                Debug.Log("Создан AssemblyManager");
            }

            // Создаем раму и слоты
            CreateDroneFrame();
            CreateAllSlots();
            CreateAllParts();

            // Создаем UI
            CreateAssemblyUI();

            // Назначаем слоты в менеджер
            if (manager != null)
            {
                AssignSlotsToManager(manager);
            }

            Debug.Log("Настройка сцены сборки завершена!");
        }

        private void CreateDroneFrame()
        {
            if (GameObject.Find("DroneFrame") != null && !createIfNotExists) return;

            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "DroneFrame";
            frame.transform.position = new Vector3(0, 0, 0);
            frame.transform.localScale = new Vector3(2f, 0.2f, 2f);
            
            // Создаем материал для рамы
            Material frameMat = new Material(Shader.Find("Standard"));
            frameMat.color = new Color(0.3f, 0.3f, 0.3f);
            frame.GetComponent<Renderer>().material = frameMat;

            // Добавляем слот для рамы
            PartSlot frameSlot = frame.AddComponent<PartSlot>();
            SetPrivateField(frameSlot, "requiredPartType", PartType.Frame);

            // Добавляем триггер
            BoxCollider trigger = frame.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(2.2f, 0.3f, 2.2f);
        }

        private void CreateAllSlots()
        {
            float offset = 0.8f;
            
            // Слоты для моторов
            CreateSlot("MotorSlot_1", new Vector3(-offset, 0.2f, offset), PartType.Motor1, 0.3f);
            CreateSlot("MotorSlot_2", new Vector3(offset, 0.2f, offset), PartType.Motor2, 0.3f);
            CreateSlot("MotorSlot_3", new Vector3(offset, 0.2f, -offset), PartType.Motor3, 0.3f);
            CreateSlot("MotorSlot_4", new Vector3(-offset, 0.2f, -offset), PartType.Motor4, 0.3f);

            // Слоты для пропеллеров
            CreateSlot("PropellerSlot_1", new Vector3(-offset, 0.4f, offset), PartType.Propeller1, 0.25f);
            CreateSlot("PropellerSlot_2", new Vector3(offset, 0.4f, offset), PartType.Propeller2, 0.25f);
            CreateSlot("PropellerSlot_3", new Vector3(offset, 0.4f, -offset), PartType.Propeller3, 0.25f);
            CreateSlot("PropellerSlot_4", new Vector3(-offset, 0.4f, -offset), PartType.Propeller4, 0.25f);

            // Слот для батареи
            CreateSlot("BatterySlot", new Vector3(0f, -0.1f, 0f), PartType.Battery, 0.3f);

            // Слот для контроллера полета
            CreateSlot("FlightControllerSlot", new Vector3(0f, 0.1f, 0f), PartType.FlightController, 0.2f);
        }

        private void CreateSlot(string name, Vector3 position, PartType partType, float triggerRadius)
        {
            if (GameObject.Find(name) != null && !createIfNotExists) return;

            GameObject slot = new GameObject(name);
            slot.transform.position = position;

            PartSlot partSlot = slot.AddComponent<PartSlot>();
            SetPrivateField(partSlot, "requiredPartType", partType);

            SphereCollider trigger = slot.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = triggerRadius;

            // Визуальный маркер слота
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "SlotMarker";
            marker.transform.SetParent(slot.transform);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.2f;
            marker.GetComponent<Collider>().enabled = false;
            
            Material markerMat = new Material(Shader.Find("Standard"));
            markerMat.color = new Color(0f, 1f, 0f, 0.5f);
            markerMat.SetFloat("_Mode", 3); // Transparent mode
            markerMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            markerMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            markerMat.SetInt("_ZWrite", 0);
            markerMat.DisableKeyword("_ALPHATEST_ON");
            markerMat.EnableKeyword("_ALPHABLEND_ON");
            markerMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            markerMat.renderQueue = 3000;
            marker.GetComponent<Renderer>().material = markerMat;
        }

        private void CreateAllParts()
        {
            float spacing = 3f;
            float startX = -6f;
            float currentX = startX;
            float currentZ = 0f;
            int itemsPerRow = 4;
            int index = 0;

            // Создаем все детали
            PartType[] allParts = {
                PartType.Frame,
                PartType.Motor1, PartType.Motor2, PartType.Motor3, PartType.Motor4,
                PartType.Propeller1, PartType.Propeller2, PartType.Propeller3, PartType.Propeller4,
                PartType.Battery,
                PartType.FlightController
            };

            foreach (PartType partType in allParts)
            {
                if (GameObject.Find(partType.ToString()) != null && !createIfNotExists) continue;

                GameObject part = CreatePart(partType);
                if (part != null)
                {
                    part.transform.position = new Vector3(currentX, 1f, currentZ);

                    index++;
                    if (index % itemsPerRow == 0)
                    {
                        currentX = startX;
                        currentZ -= spacing;
                    }
                    else
                    {
                        currentX += spacing;
                    }
                }
            }
        }

        private GameObject CreatePart(PartType partType)
        {
            GameObject part = null;
            string partName = partType.ToString();

            switch (partType)
            {
                case PartType.Frame:
                    part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    part.transform.localScale = new Vector3(2f, 0.2f, 2f);
                    break;

                case PartType.Motor1:
                case PartType.Motor2:
                case PartType.Motor3:
                case PartType.Motor4:
                    part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    part.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
                    part.transform.Rotate(90f, 0f, 0f);
                    break;

                case PartType.Propeller1:
                case PartType.Propeller2:
                case PartType.Propeller3:
                case PartType.Propeller4:
                    part = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    part.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
                    part.transform.Rotate(90f, 0f, 0f);
                    break;

                case PartType.Battery:
                    part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    part.transform.localScale = new Vector3(0.5f, 0.3f, 0.8f);
                    break;

                case PartType.FlightController:
                    part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    part.transform.localScale = new Vector3(0.4f, 0.1f, 0.6f);
                    break;
            }

            if (part != null)
            {
                part.name = partName;
                DronePart dronePart = part.AddComponent<DronePart>();
                SetPrivateField(dronePart, "partType", partType);

                // Добавляем коллайдер
                if (part.GetComponent<Collider>() == null)
                {
                    part.AddComponent<BoxCollider>();
                }

                // Создаем материал
                Material partMat = new Material(Shader.Find("Standard"));
                partMat.color = GetPartColor(partType);
                part.GetComponent<Renderer>().material = partMat;
            }

            return part;
        }

        private Color GetPartColor(PartType partType)
        {
            switch (partType)
            {
                case PartType.Frame: return new Color(0.5f, 0.5f, 0.5f);
                case PartType.Motor1:
                case PartType.Motor2:
                case PartType.Motor3:
                case PartType.Motor4: return new Color(0.8f, 0.2f, 0.2f);
                case PartType.Propeller1:
                case PartType.Propeller2:
                case PartType.Propeller3:
                case PartType.Propeller4: return new Color(0.9f, 0.9f, 0.9f);
                case PartType.Battery: return new Color(0.2f, 0.6f, 0.2f);
                case PartType.FlightController: return new Color(0.2f, 0.2f, 0.8f);
                default: return Color.white;
            }
        }

        private void CreateAssemblyUI()
        {
            if (GameObject.Find("AssemblyCanvas") != null && !createIfNotExists) return;

            // Создаем Canvas
            GameObject canvasObj = new GameObject("AssemblyCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Создаем кнопку запуска
            GameObject buttonObj = new GameObject("StartFlightButton");
            buttonObj.transform.SetParent(canvasObj.transform);
            UnityEngine.UI.Button button = buttonObj.AddComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image buttonImage = buttonObj.AddComponent<UnityEngine.UI.Image>();
            buttonImage.color = new Color(0.2f, 0.8f, 0.2f);

            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.1f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(200, 50);
            buttonRect.anchoredPosition = Vector2.zero;

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform);
            UnityEngine.UI.Text buttonText = buttonTextObj.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = "Запустить полет";
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 18;
            buttonText.color = Color.white;
            buttonText.alignment = TextAnchor.MiddleCenter;

            RectTransform textRect = buttonTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            // Создаем текст прогресса
            GameObject progressObj = new GameObject("ProgressText");
            progressObj.transform.SetParent(canvasObj.transform);
            UnityEngine.UI.Text progressText = progressObj.AddComponent<UnityEngine.UI.Text>();
            progressText.text = "Прогресс сборки: 0 / 11";
            progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            progressText.fontSize = 24;
            progressText.color = Color.white;
            progressText.alignment = TextAnchor.UpperCenter;

            RectTransform progressRect = progressObj.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.5f, 0.9f);
            progressRect.anchorMax = new Vector2(0.5f, 0.9f);
            progressRect.pivot = new Vector2(0.5f, 0.5f);
            progressRect.sizeDelta = new Vector2(300, 50);
            progressRect.anchoredPosition = Vector2.zero;

            // Добавляем AssemblyUI компонент
            AssemblyUI assemblyUI = canvasObj.AddComponent<AssemblyUI>();
            SetPrivateField(assemblyUI, "startFlightButton", button);
            SetPrivateField(assemblyUI, "progressText", progressText);

            // Настраиваем менеджер для связи с UI
            DroneAssemblyManager manager = FindObjectOfType<DroneAssemblyManager>();
            if (manager != null)
            {
                // Находим UI элементы через рефлексию
                GameObject assemblyCompleteUI = new GameObject("AssemblyCompleteUI");
                assemblyCompleteUI.transform.SetParent(canvasObj.transform);
                assemblyCompleteUI.SetActive(false);

                GameObject assemblyIncompleteUI = new GameObject("AssemblyIncompleteUI");
                assemblyIncompleteUI.transform.SetParent(canvasObj.transform);
                assemblyIncompleteUI.SetActive(true);

                SetPrivateField(manager, "assemblyCompleteUI", assemblyCompleteUI);
                SetPrivateField(manager, "assemblyIncompleteUI", assemblyIncompleteUI);
                SetPrivateField(manager, "statusText", progressText);
            }
        }

        private void AssignSlotsToManager(DroneAssemblyManager manager)
        {
            PartSlot[] allSlots = FindObjectsOfType<PartSlot>();
            SetPrivateField(manager, "allSlots", new System.Collections.Generic.List<PartSlot>(allSlots));
            Debug.Log($"Назначено {allSlots.Length} слотов в менеджер");
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(obj, value);
            }
        }
    }
}


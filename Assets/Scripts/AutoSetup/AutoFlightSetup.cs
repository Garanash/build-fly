using UnityEngine;
using FlightSimulator;

namespace AutoSetup
{
    /// <summary>
    /// Автоматически создает и настраивает сцену симулятора полета при запуске
    /// </summary>
    public class AutoFlightSetup : MonoBehaviour
    {
        [Header("Автоматическая настройка")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool createIfNotExists = true;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupFlightScene();
            }
        }

        /// <summary>
        /// Настраивает сцену симулятора полета
        /// </summary>
        [ContextMenu("Setup Flight Scene")]
        public void SetupFlightScene()
        {
            Debug.Log("Начинаю автоматическую настройку сцены симулятора полета...");

            // Создаем окружение
            CreateEnvironment();

            // Создаем квадрокоптер
            CreateDrone();

            // Настраиваем камеру
            SetupCamera();

            // Создаем UI
            CreateFlightUI();

            // Создаем менеджер
            CreateFlightManager();

            Debug.Log("Настройка сцены симулятора полета завершена!");
        }

        private void CreateEnvironment()
        {
            // Создаем землю
            if (GameObject.Find("Ground") == null || createIfNotExists)
            {
                GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = Vector3.one * 10f;

                Material groundMat = new Material(Shader.Find("Standard"));
                groundMat.color = new Color(0.4f, 0.6f, 0.3f);
                ground.GetComponent<Renderer>().material = groundMat;
            }

            // Создаем освещение
            if (GameObject.Find("Directional Light") == null || createIfNotExists)
            {
                GameObject light = new GameObject("Directional Light");
                Light dirLight = light.AddComponent<Light>();
                dirLight.type = LightType.Directional;
                dirLight.color = new Color(1f, 0.96f, 0.84f);
                dirLight.intensity = 1f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        private void CreateDrone()
        {
            if (GameObject.Find("Drone") != null && !createIfNotExists) return;

            // Создаем основной объект дрона
            GameObject drone = new GameObject("Drone");
            drone.transform.position = new Vector3(0, 2, 0);

            // Добавляем Rigidbody
            Rigidbody rb = drone.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 5f;
            rb.angularDrag = 5f;
            rb.useGravity = true;

            // Добавляем контроллер
            DroneController controller = drone.AddComponent<DroneController>();
            SetPrivateField(controller, "motorPower", 1000f);
            SetPrivateField(controller, "maxMotorPower", 2000f);
            SetPrivateField(controller, "drag", 5f);
            SetPrivateField(controller, "angularDrag", 5f);
            SetPrivateField(controller, "stability", 0.5f);
            SetPrivateField(controller, "pitchSpeed", 2f);
            SetPrivateField(controller, "rollSpeed", 2f);
            SetPrivateField(controller, "yawSpeed", 2f);
            SetPrivateField(controller, "throttleSpeed", 10f);

            // Создаем раму
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(drone.transform);
            frame.transform.localPosition = Vector3.zero;
            frame.transform.localScale = new Vector3(2f, 0.2f, 2f);
            frame.GetComponent<Collider>().isTrigger = false;

            Material frameMat = new Material(Shader.Find("Standard"));
            frameMat.color = new Color(0.3f, 0.3f, 0.3f);
            frame.GetComponent<Renderer>().material = frameMat;

            // Создаем 4 мотора
            float motorOffset = 0.8f;
            Vector3[] motorPositions = {
                new Vector3(-motorOffset, 0.2f, motorOffset),
                new Vector3(motorOffset, 0.2f, motorOffset),
                new Vector3(motorOffset, 0.2f, -motorOffset),
                new Vector3(-motorOffset, 0.2f, -motorOffset)
            };

            Transform[] motors = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject motor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                motor.name = $"Motor_{i + 1}";
                motor.transform.SetParent(drone.transform);
                motor.transform.localPosition = motorPositions[i];
                motor.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
                motor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                motor.GetComponent<Collider>().isTrigger = false;

                Material motorMat = new Material(Shader.Find("Standard"));
                motorMat.color = new Color(0.8f, 0.2f, 0.2f);
                motor.GetComponent<Renderer>().material = motorMat;

                motors[i] = motor.transform;
            }

            // Создаем 4 пропеллера
            Transform[] propellers = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject propeller = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                propeller.name = $"Propeller_{i + 1}";
                propeller.transform.SetParent(drone.transform);
                propeller.transform.localPosition = motorPositions[i] + Vector3.up * 0.3f;
                propeller.transform.localScale = new Vector3(0.4f, 0.05f, 0.4f);
                propeller.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                propeller.GetComponent<Collider>().isTrigger = false;

                Material propMat = new Material(Shader.Find("Standard"));
                propMat.color = new Color(0.9f, 0.9f, 0.9f);
                propeller.GetComponent<Renderer>().material = propMat;

                propellers[i] = propeller.transform;
            }

            // Назначаем пропеллеры в контроллер
            SetPrivateField(controller, "propeller1", propellers[0]);
            SetPrivateField(controller, "propeller2", propellers[1]);
            SetPrivateField(controller, "propeller3", propellers[2]);
            SetPrivateField(controller, "propeller4", propellers[3]);
        }

        private void SetupCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
            }

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            GameObject drone = GameObject.Find("Drone");
            if (drone != null)
            {
                SetPrivateField(cameraFollow, "target", drone.transform);
                SetPrivateField(cameraFollow, "offset", new Vector3(0f, 5f, -10f));
                SetPrivateField(cameraFollow, "smoothSpeed", 0.125f);
                SetPrivateField(cameraFollow, "lookAtTarget", true);
            }

            mainCamera.transform.position = new Vector3(0, 5, -10);
            mainCamera.transform.LookAt(Vector3.zero);
        }

        private void CreateFlightUI()
        {
            if (GameObject.Find("FlightCanvas") != null && !createIfNotExists) return;

            // Создаем Canvas
            GameObject canvasObj = new GameObject("FlightCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Создаем текстовые элементы
            CreateUIText(canvasObj.transform, "ThrottleText", "Газ: 0%", new Vector2(0.05f, 0.95f));
            CreateUIText(canvasObj.transform, "AltitudeText", "Высота: 0.0 м", new Vector2(0.05f, 0.90f));
            CreateUIText(canvasObj.transform, "SpeedText", "Скорость: 0.0 м/с", new Vector2(0.05f, 0.85f));

            // Создаем панель управления
            GameObject controlsPanel = new GameObject("ControlsPanel");
            controlsPanel.transform.SetParent(canvasObj.transform);
            RectTransform panelRect = controlsPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.7f, 0.1f);
            panelRect.anchorMax = new Vector2(0.95f, 0.6f);
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;

            UnityEngine.UI.Image panelImage = controlsPanel.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = new Color(0, 0, 0, 0.7f);

            string controlsText = "Управление:\n\n" +
                                 "Space - Газ +\n" +
                                 "Shift - Газ -\n" +
                                 "W/S - Вперед/Назад\n" +
                                 "A/D - Влево/Вправо\n" +
                                 "Q/E - Поворот\n" +
                                 "R - Сброс\n" +
                                 "Esc - Меню";

            CreateUIText(controlsPanel.transform, "ControlsText", controlsText, new Vector2(0.5f, 0.5f), 16);
            controlsPanel.SetActive(false);

            // Добавляем FlightUI компонент
            FlightUI flightUI = canvasObj.AddComponent<FlightUI>();
            GameObject drone = GameObject.Find("Drone");
            if (drone != null)
            {
                DroneController controller = drone.GetComponent<DroneController>();
                SetPrivateField(flightUI, "droneController", controller);
            }

            UnityEngine.UI.Text throttleText = GameObject.Find("ThrottleText").GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text altitudeText = GameObject.Find("AltitudeText").GetComponent<UnityEngine.UI.Text>();
            UnityEngine.UI.Text speedText = GameObject.Find("SpeedText").GetComponent<UnityEngine.UI.Text>();

            SetPrivateField(flightUI, "throttleText", throttleText);
            SetPrivateField(flightUI, "altitudeText", altitudeText);
            SetPrivateField(flightUI, "speedText", speedText);
            SetPrivateField(flightUI, "controlsPanel", controlsPanel);
        }

        private void CreateUIText(Transform parent, string name, string text, Vector2 anchor, int fontSize = 24)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            UnityEngine.UI.Text textComponent = textObj.AddComponent<UnityEngine.UI.Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.UpperLeft;

            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(300, 50);
            rect.anchoredPosition = Vector2.zero;
        }

        private void CreateFlightManager()
        {
            if (GameObject.Find("FlightSimulatorManager") != null && !createIfNotExists) return;

            GameObject managerObj = new GameObject("FlightSimulatorManager");
            FlightSimulatorManager manager = managerObj.AddComponent<FlightSimulatorManager>();
            SetPrivateField(manager, "autoStart", false);
            SetPrivateField(manager, "resetHeight", -10f);

            GameObject drone = GameObject.Find("Drone");
            if (drone != null)
            {
                DroneController controller = drone.GetComponent<DroneController>();
                SetPrivateField(manager, "droneController", controller);

                GameObject spawnPoint = new GameObject("SpawnPoint");
                spawnPoint.transform.position = new Vector3(0, 2, 0);
                SetPrivateField(manager, "spawnPoint", spawnPoint.transform);
            }
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


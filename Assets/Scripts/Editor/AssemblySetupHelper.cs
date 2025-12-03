using UnityEngine;
using UnityEditor;
using DroneAssembly;

namespace Editor
{
    /// <summary>
    /// Вспомогательный редакторский скрипт для быстрой настройки сцены сборки
    /// </summary>
    public class AssemblySetupHelper : EditorWindow
    {
        [MenuItem("Tools/Drone Assembly Setup Helper")]
        public static void ShowWindow()
        {
            GetWindow<AssemblySetupHelper>("Assembly Setup Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("Помощник настройки сборки квадрокоптера", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Создать базовую структуру сборки"))
            {
                CreateBasicAssemblyStructure();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Найти все слоты и добавить в менеджер"))
            {
                FindAndAssignSlots();
            }
        }

        private void CreateBasicAssemblyStructure()
        {
            // Создаем корневой объект для сборки
            GameObject assemblyRoot = new GameObject("DroneAssembly");
            
            // Создаем раму
            GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "Frame";
            frame.transform.SetParent(assemblyRoot.transform);
            frame.transform.localScale = new Vector3(2f, 0.2f, 2f);
            frame.transform.position = Vector3.zero;
            
            PartSlot frameSlot = frame.AddComponent<PartSlot>();
            frameSlot.GetType().GetField("requiredPartType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(frameSlot, PartType.Frame);
            
            // Создаем слоты для моторов
            CreateMotorSlots(assemblyRoot.transform);
            
            // Создаем слоты для пропеллеров
            CreatePropellerSlots(assemblyRoot.transform);
            
            // Создаем слоты для других деталей
            CreateOtherSlots(assemblyRoot.transform);
            
            // Создаем менеджер
            GameObject manager = new GameObject("AssemblyManager");
            manager.AddComponent<DroneAssemblyManager>();
            
            Debug.Log("Базовая структура сборки создана!");
        }

        private void CreateMotorSlots(Transform parent)
        {
            float offset = 0.8f;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-offset, 0.2f, offset),   // Motor 1
                new Vector3(offset, 0.2f, offset),    // Motor 2
                new Vector3(offset, 0.2f, -offset),   // Motor 3
                new Vector3(-offset, 0.2f, -offset)   // Motor 4
            };

            PartType[] motorTypes = new PartType[]
            {
                PartType.Motor1,
                PartType.Motor2,
                PartType.Motor3,
                PartType.Motor4
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject slot = new GameObject($"MotorSlot_{i + 1}");
                slot.transform.SetParent(parent);
                slot.transform.position = positions[i];
                
                PartSlot partSlot = slot.AddComponent<PartSlot>();
                partSlot.GetType().GetField("requiredPartType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(partSlot, motorTypes[i]);
                
                // Добавляем триггер
                SphereCollider trigger = slot.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.3f;
            }
        }

        private void CreatePropellerSlots(Transform parent)
        {
            float offset = 0.8f;
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-offset, 0.4f, offset),   // Propeller 1
                new Vector3(offset, 0.4f, offset),     // Propeller 2
                new Vector3(offset, 0.4f, -offset),     // Propeller 3
                new Vector3(-offset, 0.4f, -offset)    // Propeller 4
            };

            PartType[] propellerTypes = new PartType[]
            {
                PartType.Propeller1,
                PartType.Propeller2,
                PartType.Propeller3,
                PartType.Propeller4
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject slot = new GameObject($"PropellerSlot_{i + 1}");
                slot.transform.SetParent(parent);
                slot.transform.position = positions[i];
                
                PartSlot partSlot = slot.AddComponent<PartSlot>();
                partSlot.GetType().GetField("requiredPartType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(partSlot, propellerTypes[i]);
                
                // Добавляем триггер
                SphereCollider trigger = slot.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.2f;
            }
        }

        private void CreateOtherSlots(Transform parent)
        {
            // Слот для батареи
            GameObject batterySlot = new GameObject("BatterySlot");
            batterySlot.transform.SetParent(parent);
            batterySlot.transform.position = new Vector3(0f, -0.1f, 0f);
            PartSlot batteryPartSlot = batterySlot.AddComponent<PartSlot>();
            batteryPartSlot.GetType().GetField("requiredPartType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(batteryPartSlot, PartType.Battery);
            SphereCollider batteryTrigger = batterySlot.AddComponent<SphereCollider>();
            batteryTrigger.isTrigger = true;
            batteryTrigger.radius = 0.3f;

            // Слот для контроллера полета
            GameObject fcSlot = new GameObject("FlightControllerSlot");
            fcSlot.transform.SetParent(parent);
            fcSlot.transform.position = new Vector3(0f, 0.1f, 0f);
            PartSlot fcPartSlot = fcSlot.AddComponent<PartSlot>();
            fcPartSlot.GetType().GetField("requiredPartType",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(fcPartSlot, PartType.FlightController);
            SphereCollider fcTrigger = fcSlot.AddComponent<SphereCollider>();
            fcTrigger.isTrigger = true;
            fcTrigger.radius = 0.2f;
        }

        private void FindAndAssignSlots()
        {
            PartSlot[] allSlots = FindObjectsOfType<PartSlot>();
            DroneAssemblyManager manager = FindObjectOfType<DroneAssemblyManager>();
            
            if (manager == null)
            {
                Debug.LogError("DroneAssemblyManager не найден в сцене!");
                return;
            }

            SerializedObject so = new SerializedObject(manager);
            SerializedProperty slotsProperty = so.FindProperty("allSlots");
            
            if (slotsProperty != null)
            {
                slotsProperty.ClearArray();
                slotsProperty.arraySize = allSlots.Length;
                
                for (int i = 0; i < allSlots.Length; i++)
                {
                    slotsProperty.GetArrayElementAtIndex(i).objectReferenceValue = allSlots[i];
                }
                
                so.ApplyModifiedProperties();
                Debug.Log($"Найдено и назначено {allSlots.Length} слотов!");
            }
        }
    }
}


using UnityEngine;
using UnityEditor;
using DroneAssembly;

namespace Editor
{
    /// <summary>
    /// Редакторский скрипт для создания деталей квадрокоптера
    /// </summary>
    public class PartCreator : EditorWindow
    {
        private PartType selectedPartType = PartType.Frame;
        private Vector3 partPosition = Vector3.zero;
        private Vector3 partScale = Vector3.one;
        private Color partColor = Color.white;

        [MenuItem("Tools/Create Drone Part")]
        public static void ShowWindow()
        {
            GetWindow<PartCreator>("Create Drone Part");
        }

        private void OnGUI()
        {
            GUILayout.Label("Создание детали квадрокоптера", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            selectedPartType = (PartType)EditorGUILayout.EnumPopup("Тип детали:", selectedPartType);
            
            EditorGUILayout.Space();
            
            partPosition = EditorGUILayout.Vector3Field("Позиция:", partPosition);
            partScale = EditorGUILayout.Vector3Field("Масштаб:", partScale);
            partColor = EditorGUILayout.ColorField("Цвет:", partColor);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Создать деталь"))
            {
                CreatePart();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Создать все детали"))
            {
                CreateAllParts();
            }
        }

        private void CreatePart()
        {
            GameObject part = CreatePartGameObject(selectedPartType);
            if (part != null)
            {
                part.transform.position = partPosition;
                part.transform.localScale = partScale;
                
                Renderer renderer = part.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = partColor;
                    renderer.material = mat;
                }
                
                Selection.activeGameObject = part;
                Undo.RegisterCreatedObjectUndo(part, "Create Drone Part");
            }
        }

        private void CreateAllParts()
        {
            float spacing = 3f;
            float startX = -spacing * 5f;
            float currentX = startX;
            float currentZ = 0f;
            int itemsPerRow = 4;

            int index = 0;
            foreach (PartType partType in System.Enum.GetValues(typeof(PartType)))
            {
                GameObject part = CreatePartGameObject(partType);
                if (part != null)
                {
                    part.transform.position = new Vector3(currentX, 0f, currentZ);
                    
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

        private GameObject CreatePartGameObject(PartType partType)
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

                case PartType.Camera:
                    part = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    part.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                    break;
            }

            if (part != null)
            {
                part.name = partName;
                DronePart dronePart = part.AddComponent<DronePart>();
                
                // Устанавливаем тип детали через рефлексию
                var field = typeof(DronePart).GetField("partType",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(dronePart, partType);

                // Добавляем коллайдер, если его нет
                if (part.GetComponent<Collider>() == null)
                {
                    part.AddComponent<BoxCollider>();
                }
            }

            return part;
        }
    }
}


using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Вспомогательный класс для обработки ввода
    /// </summary>
    public static class InputHelper
    {
        /// <summary>
        /// Проверяет, был ли клик по объекту с коллайдером
        /// </summary>
        public static bool IsClickOnObject(Camera camera, LayerMask layerMask)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask);
            }
            return false;
        }

        /// <summary>
        /// Получает объект, по которому был клик
        /// </summary>
        public static GameObject GetClickedObject(Camera camera, LayerMask layerMask)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                {
                    return hit.collider.gameObject;
                }
            }
            return null;
        }

        /// <summary>
        /// Получает позицию мыши в мировых координатах
        /// </summary>
        public static Vector3 GetMouseWorldPosition(Camera camera, float distance = 10f)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = distance;
            return camera.ScreenToWorldPoint(mousePos);
        }
    }
}


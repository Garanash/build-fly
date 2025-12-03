using UnityEngine;

namespace FlightSimulator
{
    /// <summary>
    /// Камера, следующая за квадрокоптером
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Настройки камеры")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private float rotationSpeed = 2f;

        [Header("Режимы камеры")]
        [SerializeField] private bool followRotation = false;
        [SerializeField] private bool lookAtTarget = true;

        private void LateUpdate()
        {
            if (target == null) return;

            // Плавное перемещение камеры
            Vector3 desiredPosition = target.position + target.TransformDirection(offset);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Поворот камеры
            if (lookAtTarget)
            {
                Vector3 direction = target.position - transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
            }
            else if (followRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}


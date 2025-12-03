using UnityEngine;

namespace FlightSimulator
{
    /// <summary>
    /// Контроллер полета квадрокоптера
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DroneController : MonoBehaviour
    {
        [Header("Настройки двигателей")]
        [SerializeField] private float motorPower = 1000f;
        [SerializeField] private float maxMotorPower = 2000f;
        [SerializeField] private float motorResponseSpeed = 5f;

        [Header("Настройки физики")]
        [SerializeField] private float drag = 5f;
        [SerializeField] private float angularDrag = 5f;
        [SerializeField] private float stability = 0.5f;

        [Header("Настройки управления")]
        [SerializeField] private float pitchSpeed = 2f;
        [SerializeField] private float rollSpeed = 2f;
        [SerializeField] private float yawSpeed = 2f;
        [SerializeField] private float throttleSpeed = 10f;

        [Header("Ссылки на пропеллеры")]
        [SerializeField] private Transform propeller1;
        [SerializeField] private Transform propeller2;
        [SerializeField] private Transform propeller3;
        [SerializeField] private Transform propeller4;

        private Rigidbody rb;
        private float currentThrottle = 0f;
        private float targetThrottle = 0f;
        private Vector3 targetRotation = Vector3.zero;

        // Текущие значения двигателей
        private float motor1Power = 0f;
        private float motor2Power = 0f;
        private float motor3Power = 0f;
        private float motor4Power = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.drag = drag;
            rb.angularDrag = angularDrag;
        }

        private void Update()
        {
            HandleInput();
            UpdatePropellerRotation();
        }

        private void FixedUpdate()
        {
            ApplyMotorForces();
            ApplyStability();
        }

        /// <summary>
        /// Обрабатывает ввод пользователя
        /// </summary>
        private void HandleInput()
        {
            // Газ (Throttle)
            if (Input.GetKey(KeyCode.Space))
            {
                targetThrottle += throttleSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                targetThrottle -= throttleSpeed * Time.deltaTime;
            }

            targetThrottle = Mathf.Clamp01(targetThrottle);

            // Питч (Pitch) - наклон вперед/назад
            float pitch = 0f;
            if (Input.GetKey(KeyCode.W))
            {
                pitch = -pitchSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                pitch = pitchSpeed;
            }

            // Ролл (Roll) - наклон влево/вправо
            float roll = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                roll = -rollSpeed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                roll = rollSpeed;
            }

            // Рыскание (Yaw) - поворот влево/вправо
            float yaw = 0f;
            if (Input.GetKey(KeyCode.Q))
            {
                yaw = -yawSpeed;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                yaw = yawSpeed;
            }

            targetRotation = new Vector3(pitch, yaw, roll);
        }

        /// <summary>
        /// Применяет силы двигателей
        /// </summary>
        private void ApplyMotorForces()
        {
            // Плавное изменение газа
            currentThrottle = Mathf.Lerp(currentThrottle, targetThrottle, Time.fixedDeltaTime * motorResponseSpeed);

            // Базовая мощность двигателей
            float basePower = currentThrottle * motorPower;

            // Распределение мощности с учетом управления
            // Мотор 1: передний левый
            // Мотор 2: передний правый
            // Мотор 3: задний правый
            // Мотор 4: задний левый

            motor1Power = basePower;
            motor2Power = basePower;
            motor3Power = basePower;
            motor4Power = basePower;

            // Питч (наклон вперед/назад)
            motor1Power += targetRotation.x * motorPower * 0.3f;
            motor2Power += targetRotation.x * motorPower * 0.3f;
            motor3Power -= targetRotation.x * motorPower * 0.3f;
            motor4Power -= targetRotation.x * motorPower * 0.3f;

            // Ролл (наклон влево/вправо)
            motor1Power += targetRotation.z * motorPower * 0.3f;
            motor2Power -= targetRotation.z * motorPower * 0.3f;
            motor3Power -= targetRotation.z * motorPower * 0.3f;
            motor4Power += targetRotation.z * motorPower * 0.3f;

            // Рыскание (поворот)
            motor1Power += targetRotation.y * motorPower * 0.2f;
            motor2Power -= targetRotation.y * motorPower * 0.2f;
            motor3Power += targetRotation.y * motorPower * 0.2f;
            motor4Power -= targetRotation.y * motorPower * 0.2f;

            // Ограничиваем мощность
            motor1Power = Mathf.Clamp(motor1Power, 0f, maxMotorPower);
            motor2Power = Mathf.Clamp(motor2Power, 0f, maxMotorPower);
            motor3Power = Mathf.Clamp(motor3Power, 0f, maxMotorPower);
            motor4Power = Mathf.Clamp(motor4Power, 0f, maxMotorPower);

            // Применяем силы вверх
            rb.AddForceAtPosition(transform.up * motor1Power, GetMotorPosition(1), ForceMode.Force);
            rb.AddForceAtPosition(transform.up * motor2Power, GetMotorPosition(2), ForceMode.Force);
            rb.AddForceAtPosition(transform.up * motor3Power, GetMotorPosition(3), ForceMode.Force);
            rb.AddForceAtPosition(transform.up * motor4Power, GetMotorPosition(4), ForceMode.Force);
        }

        /// <summary>
        /// Применяет стабилизацию
        /// </summary>
        private void ApplyStability()
        {
            // Стабилизация по углу наклона
            Vector3 predictedUp = Quaternion.AngleAxis(
                rb.angularVelocity.magnitude * Mathf.Rad2Deg * stability / motorPower,
                rb.angularVelocity
            ) * transform.up;

            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torqueVector * motorPower * stability);
        }

        /// <summary>
        /// Получает позицию двигателя
        /// </summary>
        private Vector3 GetMotorPosition(int motorIndex)
        {
            float offset = 0.5f; // Расстояние от центра до двигателя
            switch (motorIndex)
            {
                case 1: return transform.position + transform.forward * offset + transform.left * offset;
                case 2: return transform.position + transform.forward * offset + transform.right * offset;
                case 3: return transform.position - transform.forward * offset + transform.right * offset;
                case 4: return transform.position - transform.forward * offset + transform.left * offset;
                default: return transform.position;
            }
        }

        /// <summary>
        /// Обновляет вращение пропеллеров
        /// </summary>
        private void UpdatePropellerRotation()
        {
            float rotationSpeed = 500f * currentThrottle;

            if (propeller1 != null)
            {
                propeller1.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
            if (propeller2 != null)
            {
                propeller2.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);
            }
            if (propeller3 != null)
            {
                propeller3.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
            }
            if (propeller4 != null)
            {
                propeller4.Rotate(Vector3.back, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Получает текущий газ
        /// </summary>
        public float GetThrottle()
        {
            return currentThrottle;
        }
    }
}


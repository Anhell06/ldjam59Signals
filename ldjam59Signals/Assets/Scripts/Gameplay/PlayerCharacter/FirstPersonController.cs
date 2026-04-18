using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private Transform playerCamera;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isCrouching = false;
    private bool isGrounded;
    
    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 0.15f; // Увеличьте это значение
    [SerializeField] private float groundCheckRadius = 0.28f;   // Чуть меньше радиуса капсулы
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float skinWidth = 0.08f; // Добавьте небольшой отступ

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Если камера не назначена в инспекторе, попробуем найти её как дочернюю
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>()?.transform;
    }

    private void Start()
    {
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleCrouch();
        HandleJump();
        
        ApplyGravity();
        ApplyFinalMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Вращение тела (персонажа) по горизонтали
        transform.Rotate(Vector3.up * mouseX);

        // Вращение камеры по вертикали с ограничением угла
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        // Получаем ввод с клавиатуры
        float horizontal = Input.GetAxis("Horizontal"); // A/D или стрелки
        float vertical = Input.GetAxis("Vertical");     // W/S или стрелки

        // Направление движения относительно поворота персонажа
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        // Выбор скорости в зависимости от режима приседания
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        
        // Применяем движение (без учета Y, так как гравитация считается отдельно)
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        // Проверка нажатия клавиши Ctrl (удержание)
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!isCrouching)
            {
                isCrouching = true;
            }
        }
        else
        {
            // Проверяем, можно ли встать (нет препятствия сверху)
            if (isCrouching && CanStandUp())
            {
                isCrouching = false;
            }
        }

        // Плавное изменение высоты контроллера и позиции камеры
        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        float targetCamY = isCrouching ? crouchHeight / 2f : standingHeight / 2f;

        // Меняем высоту CharacterController
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        
        // Смещаем центр контроллера
        Vector3 newCenter = controller.center;
        newCenter.y = controller.height / 2f;
        controller.center = newCenter;

        // Двигаем камеру вверх/вниз относительно позиции персонажа
        Vector3 camPos = playerCamera.localPosition;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.localPosition = camPos;
    }

    private bool CanStandUp()
    {
        // Проверяем лучом вверх, нет ли препятствия над головой
        float checkDistance = standingHeight - crouchHeight + 0.1f;
        Vector3 origin = transform.position + Vector3.up * (crouchHeight - 0.1f);
        
        if (Physics.Raycast(origin, Vector3.up, checkDistance))
        {
            return false;
        }
        return true;
    }

    private void HandleJump()
    {
        // Проверяем, стоит ли персонаж на земле
        isGrounded = CustomGroundCheck();

        // Если на земле и есть отрицательная вертикальная скорость, сбрасываем её
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое прижимание к земле для стабильности
        }

        // Прыжок только если не в приседе и нажата клавиша Space
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            // Формула прыжка: v = sqrt(h * -2 * g)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        // Применяем гравитацию к вертикальной скорости
        velocity.y += gravity * Time.deltaTime;
    }

    private void ApplyFinalMovement()
    {
        // Двигаем персонажа с учетом вертикальной скорости (прыжок/гравитация)
        controller.Move(velocity * Time.deltaTime);
    }

    // Опционально: разблокировка курсора при нажатии Escape (для удобства отладки)
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    private bool CustomGroundCheck()
    {
        // Позиция для проверки: от низа капсулы + небольшой отступ
        Vector3 capsuleBottom = transform.position + controller.center;// + Vector3.down * (controller.height / 2 - skinWidth);
    
        // Вариант 1: SphereCast (самый надежный)
        if (Physics.SphereCast(capsuleBottom, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask))
        {
            // Проверяем угол наклона поверхности (чтобы не прилипать к стенам)
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle <= controller.slopeLimit;
        }
    
        // Вариант 2: CheckSphere (проще, но тоже работает)
        // Vector3 spherePosition = capsuleBottom + Vector3.down * groundCheckRadius;
        // return Physics.CheckSphere(spherePosition, groundCheckRadius, groundMask);
    
        return false;
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Sprint Settings")]
    [SerializeField] private bool holdToSprint = true; // true = удержание Shift, false = переключение
    [SerializeField] private float sprintFOV = 70f; // Увеличение FOV при беге
    [SerializeField] private float fovTransitionSpeed = 8f; // Скорость изменения FOV
    
    [Header("Stamina Settings (опционально)")]
    [SerializeField] private bool useStamina = false;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 25f; // Расход в секунду
    [SerializeField] private float staminaRegenRate = 20f; // Восстановление в секунду
    [SerializeField] private float staminaRegenDelay = 1f; // Задержка перед восстановлением
    [SerializeField] private float minStaminaToSprint = 10f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxLookAngle = 80f;
    [SerializeField] private Transform playerCamera;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckDistance = 0.15f; // Увеличьте это значение
    [SerializeField] private float groundCheckRadius = 0.28f;   // Чуть меньше радиуса капсулы
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private float skinWidth = 0.08f; // Добавьте небольшой отступ

    private CharacterController controller;
    private Camera playerCameraComponent;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isCrouching = false;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float coyoteTime = 0.15f;
    private bool jumpPressed;
    
    // Переменные для бега
    private bool isSprinting = false;
    private float currentStamina;
    private float staminaRegenTimer;
    private float defaultFOV;
    
    // Свойство для получения состояния бега из других скриптов
    public bool IsSprinting => isSprinting;
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>()?.transform;
        
        if (playerCamera != null)
            playerCameraComponent = playerCamera.GetComponent<Camera>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentStamina = maxStamina;
        
        if (playerCameraComponent != null)
            defaultFOV = playerCameraComponent.fieldOfView;
    }

    private void Update()
    {
        HandleMouseLook();
        isGrounded = CustomGroundCheck();
        
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;

        HandleSprint();
        HandleStamina();
        HandleSprintFOV();
        HandleMovement();
        HandleCrouch();
        HandleJump();
        ApplyGravity();
        ApplyFinalMovement();
        StickToGround();
    }

    private void HandleSprint()
    {
        if (isCrouching)
        {
            isSprinting = false;
            return;
        }

        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        bool hasEnoughStamina = !useStamina || currentStamina > minStaminaToSprint;
        bool isMoving = Input.GetAxis("Vertical") > 0 || Input.GetAxis("Horizontal") != 0;

        if (holdToSprint)
        {
            // Режим удержания Shift
            isSprinting = sprintInput && hasEnoughStamina && isMoving;
        }
        else
        {
            // Режим переключения
            if (Input.GetKeyDown(KeyCode.LeftShift) && hasEnoughStamina && isMoving)
            {
                isSprinting = !isSprinting;
            }
            
            // Автоматически выключаем бег если остановились или недостаточно стамины
            if (!isMoving || !hasEnoughStamina)
            {
                isSprinting = false;
            }
        }
    }

    private void HandleStamina()
    {
        if (!useStamina) return;

        if (isSprinting && isGrounded)
        {
            // Расход стамины при беге
            currentStamina = Mathf.Max(0, currentStamina - staminaDrainRate * Time.deltaTime);
            staminaRegenTimer = staminaRegenDelay;
            
            // Автоматически выключаем бег если стамина кончилась
            if (currentStamina <= 0)
            {
                isSprinting = false;
            }
        }
        else
        {
            // Восстановление стамины
            if (staminaRegenTimer > 0)
            {
                staminaRegenTimer -= Time.deltaTime;
            }
            else if (currentStamina < maxStamina)
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
            }
        }
    }

    private void HandleSprintFOV()
    {
        if (playerCameraComponent == null) return;
        
        float targetFOV = (isSprinting && !isCrouching) ? sprintFOV : defaultFOV;
        playerCameraComponent.fieldOfView = Mathf.Lerp(
            playerCameraComponent.fieldOfView, 
            targetFOV, 
            Time.deltaTime * fovTransitionSpeed
        );
    }

    private bool CustomGroundCheck()
    {
        Vector3 capsuleBottom = transform.position + controller.center;
        
        if (Physics.SphereCast(capsuleBottom, groundCheckRadius, Vector3.down, 
                              out RaycastHit hit, groundCheckDistance, groundMask))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle <= controller.slopeLimit;
        }
        
        return false;
    }

    private void StickToGround()
    {
        if (isGrounded && velocity.y <= 0)
        {
            Vector3 capsuleBottom = transform.position + controller.center + 
                                   Vector3.down * (controller.height / 2);
            
            if (Physics.Raycast(capsuleBottom, Vector3.down, out RaycastHit hit, 
                               groundCheckDistance * 2, groundMask))
            {
                Vector3 stickVelocity = Vector3.down * (hit.distance - skinWidth);
                controller.Move(stickVelocity);
            }
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();
        
        // Выбор скорости
        float currentSpeed;
        if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else if (isSprinting && vertical > 0) // Бег только вперед (как в большинстве шутеров)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
        
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            isCrouching = true;
            isSprinting = false; // Нельзя бежать в приседе
        }
        else if (isCrouching && CanStandUp())
        {
            isCrouching = false;
        }

        float targetHeight = isCrouching ? crouchHeight : standingHeight;
        
        if (Mathf.Abs(controller.height - targetHeight) > 0.01f)
        {
            controller.height = Mathf.Lerp(controller.height, targetHeight, 
                                          Time.deltaTime * crouchTransitionSpeed);
            
            Vector3 newCenter = controller.center;
            newCenter.y = controller.height / 2f;
            controller.center = newCenter;

            Vector3 camPos = playerCamera.localPosition;
            camPos.y = targetHeight / 2f;
            playerCamera.localPosition = camPos;
        }
    }

    private bool CanStandUp()
    {
        float checkDistance = standingHeight - crouchHeight + 0.1f;
        Vector3 origin = transform.position + Vector3.up * (crouchHeight - 0.1f);
        return !Physics.Raycast(origin, Vector3.up, checkDistance);
    }

    private void HandleJump()
    {
        if (jumpPressed && (isGrounded || coyoteTimeCounter > 0) && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimeCounter = 0;
            jumpPressed = false;
        }
        else if (jumpPressed)
        {
            jumpPressed = false;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void ApplyFinalMovement()
    {
        controller.Move(velocity * Time.deltaTime);
    }

    // Публичные методы для UI или других систем
    public float GetStaminaPercentage()
    {
        return currentStamina / maxStamina;
    }

    public void RefillStamina(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    // Визуализация для отладки
    private void OnDrawGizmos()
    {
        if (controller == null) return;
        
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 capsuleBottom = transform.position + controller.center + 
                               Vector3.down * (controller.height / 2 - skinWidth);
        
        Gizmos.DrawLine(capsuleBottom, capsuleBottom + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(capsuleBottom + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
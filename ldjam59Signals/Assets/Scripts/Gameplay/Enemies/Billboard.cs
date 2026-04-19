using UnityEngine;
using UnityEngine.Serialization;

public class Billboard : MonoBehaviour
{
    [FormerlySerializedAs("left")] [SerializeField] private GameObject _backward;
    [FormerlySerializedAs("right")] [SerializeField] private GameObject _forward;
    [FormerlySerializedAs("forward")] [SerializeField] private GameObject _left;
    [FormerlySerializedAs("backward")] [SerializeField] private GameObject _right;
    
    // Дополнительные параметры для учета позиции
    [SerializeField] private bool usePositionForBillboard = true;
    [SerializeField] private float positionInfluenceWeight = 0.5f; // Вес влияния позиции (0-1)
    
    private Vector3 lastPosition;
    private Vector3 currentDirection;
    
    private void Start()
    {
        lastPosition = transform.position;
    }
    
    public void SetMovingDirection(Vector3 direction)
    {
        currentDirection = direction;
        SetMovingDirection2D(new Vector2(direction.x, direction.z), Camera.main);
    }
    
    private void LateUpdate()
    {
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        // Получаем направление на камеру без вертикальной составляющей
        Vector3 directionToCamera = toCamera.normalized;
        directionToCamera.y = 0f;
        
        // Если направление слишком маленькое, используем forward по умолчанию
        if (directionToCamera.sqrMagnitude < 0.0001f)
            directionToCamera = transform.forward;
        else
            directionToCamera.Normalize();
        
        // Поворачиваем объект только вокруг оси Y
        transform.rotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
        
        // Обновляем билборд на основе движения и позиции
        UpdateBillboardBasedOnMovementAndPosition();
    }
    
    private void UpdateBillboardBasedOnMovementAndPosition()
    {
        // Вычисляем направление движения
        Vector3 movementDirection = transform.position - lastPosition;
        movementDirection.y = 0f;
        
        Vector3 finalDirection;
        
        if (usePositionForBillboard && movementDirection.sqrMagnitude > 0.0001f)
        {
            // Вычисляем направление от текущей позиции к точке впереди по движению
            Vector3 futurePosition = transform.position + movementDirection.normalized;
            Vector3 directionFromCurrent = (futurePosition - transform.position).normalized;
            
            // Комбинируем направление движения с учетом позиции
            finalDirection = Vector3.Lerp(movementDirection.normalized, directionFromCurrent, positionInfluenceWeight);
            finalDirection.Normalize();
        }
        else if (movementDirection.sqrMagnitude > 0.0001f)
        {
            finalDirection = movementDirection.normalized;
        }
        else
        {
            finalDirection = currentDirection;
        }
        
        // Применяем билборд с учетом комбинированного направления
        if (finalDirection.sqrMagnitude > 0.0001f)
        {
            ApplyBillboardDirection(finalDirection);
        }
        
        lastPosition = transform.position;
    }
    
    private void ApplyBillboardDirection(Vector3 direction)
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null)
            return;
            
        Vector3 worldDirection = direction.normalized;
        
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();
        
        Vector3 cameraRight = playerCamera.transform.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();
        
        float forwardDot = Vector3.Dot(worldDirection, cameraForward);
        float rightDot = Vector3.Dot(worldDirection, cameraRight);
        
        if (Mathf.Abs(forwardDot) >= Mathf.Abs(rightDot))
        {
            if (forwardDot >= 0f)
                SetFrontBillboard();
            else
                SetBackBillboard();
        }
        else
        {
            if (rightDot >= 0f)
                SetRightBillboard();
            else
                SetLeftBillboard();
        }
    }

    private void SetMovingDirection2D(Vector2 direction, Camera playerCamera)
    {
        if (playerCamera == null || direction.sqrMagnitude < 0.0001f)
            return;

        Vector3 worldDirection = new Vector3(direction.x, 0f, direction.y).normalized;
        
        // Сохраняем текущее направление
        currentDirection = worldDirection;
        
        // Применяем билборд с учетом позиции
        if (usePositionForBillboard)
        {
            UpdateBillboardBasedOnMovementAndPosition();
        }
        else
        {
            ApplyBillboardDirection(worldDirection);
        }
    }

    protected virtual void SetFrontBillboard()
    {
        _backward.gameObject.SetActive(true);
        _forward.gameObject.SetActive(false);
        _left.gameObject.SetActive(false);
        _right.gameObject.SetActive(false);
    }

    protected virtual void SetBackBillboard()
    {
        _backward.gameObject.SetActive(false);
        _forward.gameObject.SetActive(true);
        _left.gameObject.SetActive(false);
        _right.gameObject.SetActive(false);
    }

    protected virtual void SetLeftBillboard()
    {
        _backward.gameObject.SetActive(false);
        _forward.gameObject.SetActive(false);
        _left.gameObject.SetActive(true);
        _right.gameObject.SetActive(false);
    }

    protected virtual void SetRightBillboard()
    {
        _backward.gameObject.SetActive(false);
        _forward.gameObject.SetActive(false);
        _left.gameObject.SetActive(false);
        _right.gameObject.SetActive(true);
    }
}
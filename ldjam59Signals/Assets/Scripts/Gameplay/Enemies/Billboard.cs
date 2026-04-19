using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    
    public void SetMovingDirection(Vector3 direction)
    {
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
    }

    private void SetMovingDirection2D(Vector2 direction, Camera playerCamera)
    {
        if (playerCamera == null || direction.sqrMagnitude < 0.0001f)
            return;

        Vector3 worldDirection = new Vector3(direction.x, 0f, direction.y).normalized;

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

    protected virtual void SetFrontBillboard()
    {
        _animator.SetBool("Front",true);
        _animator.SetBool("Back",false);
        _animator.SetBool("Left",false);
        _animator.SetBool("Right",false);
    }

    protected virtual void SetBackBillboard()
    {
        _animator.SetBool("Front",false);
        _animator.SetBool("Back",true);
        _animator.SetBool("Left",false);
        _animator.SetBool("Right",false);
    }

    protected virtual void SetLeftBillboard()
    {
        _animator.SetBool("Front",false);
        _animator.SetBool("Back",false);
        _animator.SetBool("Left",true);
        _animator.SetBool("Right",false);
    }

    protected virtual void SetRightBillboard()
    {
        _animator.SetBool("Front",false);
        _animator.SetBool("Back",false);
        _animator.SetBool("Left",false);
        _animator.SetBool("Right",true);
    }
}
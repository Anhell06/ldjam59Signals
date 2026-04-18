using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] 
    private SpriteRenderer _spritreRend;
    
    public void SetMovingDirection(Vector3 direction)
    {
        SetMovingDirection2D(new Vector2(direction.x, direction.z), Camera.main);
    }
    
    private void LateUpdate()
    {
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
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
    }

    protected virtual void SetBackBillboard()
    {
    }

    protected virtual void SetLeftBillboard()
    {
    }

    protected virtual void SetRightBillboard()
    {
    }
}

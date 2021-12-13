using UnityEngine;

public class RotateTowardsMouse:MonoBehaviour
{
    //rotate stuff
    public float smoothTime = 0.1f;
    float turnSmoothVelocity;
    [SerializeField] Camera cam;
    [SerializeField] float distance = 1f;

    private void Awake()
    {
        if(cam==null)
            cam = Camera.main;
    }

    private void Update()
    {
        UpdateRotation();
    }

    void UpdateRotation()
    {
        //float targetAngle = GetAngleFromOriginToMouse(transform.position);
        //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothTime);
        //transform.rotation = Quaternion.Euler(0f, angle, 0f);
        Vector3 screenMP = Input.mousePosition;
        screenMP.z = distance;
        Vector3 mousePos = cam.ScreenToWorldPoint(screenMP);
        mousePos.y = transform.position.y;
        Vector3 lookDirection = (mousePos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    public Vector3 GetLookDirection(Vector3 origin)
    {
        Vector2 mousePos = (Vector2)cam.ScreenToViewportPoint(Input.mousePosition);
        Vector2 positionOnScreen = cam.WorldToViewportPoint(origin);
        Vector2 lookDirection = mousePos - positionOnScreen;        
        return lookDirection;
    }

    public float GetAngleFromOriginToMouse(Vector3 origin)
    {
        Vector3 lookDirection = GetLookDirection(origin);
        float targetAngle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;
        return targetAngle;

    }
}
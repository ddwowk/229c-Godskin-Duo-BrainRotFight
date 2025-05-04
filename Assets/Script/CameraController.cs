using TMPro;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    private Camera mainCamere;
    public Transform currentTarget;
    public float SmoothTime = 0.3f;
    public float DefaulZoom = 3f;
    public float BulletZoom = 4f;
    public float SmootZoom = 0.2f;

    private float targetZoomSize;
    public Vector3 Offset = new Vector3(0, -4, -10);
    private Vector3 cameraVelocity = Vector3.zero;
    private float zoomVelocity = 0f;
   
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            mainCamere = GetComponent<Camera>();
        }
    }
    private void Start()
    {
        targetZoomSize = DefaulZoom;
    }
    private void LateUpdate()
    {
        if(currentTarget != null) {
        
            Vector3 targetPosition = currentTarget.position + Offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref cameraVelocity, SmoothTime);
            mainCamere.orthographicSize = Mathf.SmoothDamp(mainCamere.orthographicSize, targetZoomSize, ref zoomVelocity, SmootZoom);
        }   
    }
    public void SetTarget(Transform newTarget, float zoomLevel)
    {
        if(newTarget != null)
        {
            currentTarget = newTarget;
            targetZoomSize = zoomLevel;
        }
    }
    public void SetTargetWithDefaultZoom(Transform newTarget)
    {
        SetTarget(newTarget, DefaulZoom);
    }
    public void SetTargetWithProjectileZoom(Transform newTarget)
    {
        SetTarget(newTarget, BulletZoom);
    }
}

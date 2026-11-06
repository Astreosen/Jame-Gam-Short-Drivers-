using UnityEngine;

public class Rotater : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float xAngle = -22.497f;
    [SerializeField] private float yAngle = 0f;

    private Camera cam;
    private float zAngle;
    private bool dragging;

    private void Awake()
    {
        cam = Camera.main;
        zAngle = transform.eulerAngles.z;
        transform.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
    }

    private Vector2 lastDir;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragging = hit.collider.transform.IsChildOf(transform);
                if (dragging)
                    lastDir = (Vector2)Input.mousePosition - (Vector2)cam.WorldToScreenPoint(transform.position);
            }
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (dragging)
        {
            if (zAngle >= 126f)
            {
                zAngle = 126f;
            }
            if (zAngle <= -126f)
            {
                zAngle = -126f;
            }
            Vector2 dir = (Vector2)Input.mousePosition - (Vector2)cam.WorldToScreenPoint(transform.position);
            zAngle += Vector2.SignedAngle(lastDir, dir);
            lastDir = dir;
            transform.localRotation = Quaternion.Euler(xAngle, yAngle, -zAngle);
        }
    }
}
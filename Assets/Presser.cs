using UnityEngine;

public class Presser : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float xAngle = -22.497f;
    [SerializeField] private float yAngle = 0f;

    private Camera cam;
    private float zAngle;
    public bool dragging;

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
                    
            }
        }

        if (Input.GetMouseButtonUp(0))
            dragging = false;

        if (dragging)
        {
            transform.localRotation = Quaternion.Euler(-60f, 0f, -90f);
        }
        else {
            transform.localRotation = Quaternion.Euler(-90f, 0f, -90f);
        }
    }
}
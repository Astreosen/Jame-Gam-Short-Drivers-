using UnityEngine;

public class CursorLookCamera : MonoBehaviour
{
    public float maxYaw = 60f;
    public float maxPitch = 40f;
    public float smoothing = 6f;

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float x = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
        float y = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;

        Quaternion offset = Quaternion.Euler(-y * maxPitch, x * maxYaw, 0f);
        Quaternion target = startRotation * offset;

        transform.localRotation = Quaternion.Slerp(transform.localRotation, target, smoothing * Time.deltaTime);
    }
}
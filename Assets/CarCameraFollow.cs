using UnityEngine;

public class CarCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 6f;
    public float height = 2.5f;
    public float rotationDamping = 3f;
    public float heightDamping = 2f;

    [Header("Look")]
    public bool lookAtTarget = true;

    private void LateUpdate()
    {
        if (!target) return;

        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        Vector3 pos = target.position - currentRotation * Vector3.forward * distance;
        pos.y = currentHeight;

        transform.position = pos;

        if (lookAtTarget)
            transform.LookAt(target);
    }
}

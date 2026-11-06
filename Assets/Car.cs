using UnityEngine;
using UnityEngine.VFX;


public class Car : MonoBehaviour
{
    public Rigidbody rb;

    public float SuspensionHeight = 3f;
    public float SuspensionForce = 200f;
    public float Stabilize = 2f;
    public float Damping = 5f;
    public float DampingS = 5f;

    public float Speed = 500f;
    public float BrakeForce = 10f;
    public float MaxSteerAngle = 30f;
    public float TurnSpeed = 5f;

    public float LateralFriction = 0.8f;
    public float RollingFriction = 0.98f;

    public bool isGrounded;
    public float x;
    public float y;
    public float z;

    private float inputForward;
    private bool inputBrake;
    private float inputSteer;

    public VisualEffect vfx;
    public float slideThreshold = 2f;  
    public float maxSmokeRate = 50f; 
    public float minSmokeLife = 0.1f;
    public float maxSmokeLife = 1f; 


    void Update()
    {
        inputForward = 0f;
        if (Input.GetKey(KeyCode.W)) inputForward = 1f;
        if (Input.GetKey(KeyCode.S)) inputForward = -1f;

        inputBrake = Input.GetKey(KeyCode.Space);

        inputSteer = 0f;
        if (Input.GetKey(KeyCode.A)) inputSteer = -1f;
        if (Input.GetKey(KeyCode.D)) inputSteer = 1f;

        Vector3 euler = transform.eulerAngles;
        x = euler.x > 180f ? euler.x - 360f : euler.x;
        z = euler.z > 180f ? euler.z - 360f : euler.z;
        y = euler.y;
    }

    void FixedUpdate()
    {
        Suspension();
        Move();
        Steer();
        ApplyFriction();
        UpdateVFX();
    }

    void UpdateVFX()
    {
        if (!isGrounded)
        {
            vfx.SetFloat("SmokeRate", 0f);
            return;
        }

        Vector3 localVel = transform.InverseTransformDirection(rb.linearVelocity);

        float slideAmount = Mathf.Abs(localVel.x);

        if (slideAmount > slideThreshold)
        {
            float speed = rb.linearVelocity.magnitude;

            float t = Mathf.InverseLerp(0f, 40f, speed);

            float rate = Mathf.Lerp(0f, maxSmokeRate, slideAmount / 10f);
            float life = Mathf.Lerp(maxSmokeLife, minSmokeLife, t);

            vfx.SetFloat("SmokeRate", rate);
            vfx.SetFloat("SmokeLife", life);
        }
        else
        {
            vfx.SetFloat("SmokeRate", 0f);
        }
    }


    void Suspension()
    {
        Vector3 down = transform.TransformDirection(Vector3.down);

        if (Physics.Raycast(transform.position, down, out RaycastHit hit, SuspensionHeight + 1f))
        {
            isGrounded = true;

            if (hit.distance <= SuspensionHeight)
            {
                float displacement = SuspensionHeight - hit.distance;

                Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
                float forwardAccel = localVelocity.z;
                float rightAccel = localVelocity.x;

                float springForce = displacement * SuspensionForce;
                springForce += forwardAccel * 50f;
                springForce -= rightAccel * 25f;

                float damperForce = Vector3.Dot(rb.linearVelocity, transform.up) * DampingS;
                rb.AddForce(transform.up * (springForce - damperForce), ForceMode.Force);

                rb.AddTorque(Vector3.right * (-x * Stabilize - rb.angularVelocity.x * Damping));
                rb.AddTorque(Vector3.forward * (-z * Stabilize - rb.angularVelocity.z * Damping));
            }
        }
        else
        {
            isGrounded = false;
        }

    }

    void Move()
    {
        if (isGrounded)
        {
            rb.AddForce(transform.forward * inputForward * Speed * Time.fixedDeltaTime, ForceMode.Force);

            if (inputBrake)
            {
                rb.AddForce(-rb.linearVelocity * BrakeForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    void Steer()
    {
        if (!isGrounded || inputSteer == 0f) return;

        float speedFactor = rb.linearVelocity.magnitude;
        float steerAngle = MaxSteerAngle * inputSteer;
        float turn = Mathf.Tan(Mathf.Deg2Rad * steerAngle) * speedFactor * Time.fixedDeltaTime;

        rb.MoveRotation(Quaternion.Euler(0f, transform.eulerAngles.y + turn, 0f));
    }

    void ApplyFriction()
    {
        if (!isGrounded) return;

        Vector3 lateralVelocity = Vector3.Project(rb.linearVelocity, transform.right);
        rb.AddForce(-lateralVelocity * LateralFriction, ForceMode.VelocityChange);

        Vector3 forwardVelocity = Vector3.Project(rb.linearVelocity, transform.forward);
        rb.AddForce(-forwardVelocity * (1f - RollingFriction), ForceMode.VelocityChange);
    }
}

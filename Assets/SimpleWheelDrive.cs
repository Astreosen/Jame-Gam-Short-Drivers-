using UnityEngine;
using UnityEngine.VFX;


[RequireComponent(typeof(Rigidbody))]
public class SimpleWheelDrive : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Visuals (optional)")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;
    public AudioSource engineRev;

    [Header("Drift Kick Settings")]
    public float driftKickForce = 2000f;
    public float driftKickSteerMultiplier = 1f;



    [Header("Drive & Steering")]
    public float maxMotorTorque = 1200f;
    public float maxBrakeTorque = 3000f;
    public float maxSteerAngle = 30f;

    [Header("Physics")]
    public float downforce = 100f;
    public float centerOfMassYOffset = -0.5f;

    [Header("Drift Settings")]
    public float driftGrip = 0.4f;
    public float frontGrip = 0.85f; 
    public float driftSteerThreshold = 0.6f;
    public float driftSpeedThreshold = 10f;
    public float driftLerpSpeed = 6f;

    [Header("Per-Wheel VFX")]
    public VisualEffect vfxFL;
    public VisualEffect vfxFR;
    public VisualEffect vfxRL;
    public VisualEffect vfxRR;

    public float wheelSidewaysThreshold = 2f;
    public float wheelSpinSpeedThreshold = 3f;
    public float maxWheelSmokeRate = 50f;

    public float maxWheelSmokeLife = 0.8f;
    public float minWheelSmokeLife = 0.1f;

    public float spinSmokeBoostPerWheel = 2f;




    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += new Vector3(0f, centerOfMassYOffset, 0f);
    }

    void FixedUpdate()
    {
        float motorInput = Input.GetAxis("Vertical");
        float steerInput = Input.GetAxis("Horizontal");

        engineRev.pitch = Mathf.Abs(motorInput) * 2.4f + 0.6f;

        float steering = maxSteerAngle * steerInput;
        wheelFL.steerAngle = steering;
        wheelFR.steerAngle = steering;


        float motor = maxMotorTorque * motorInput;
        wheelRL.motorTorque = motor;
        wheelRR.motorTorque = motor;

        bool handbrake = Input.GetKey(KeyCode.Space);
        bool brakeInput = Input.GetKey(KeyCode.LeftShift);


        float brake = brakeInput ? maxBrakeTorque : 0f;
        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;


        HandleDrift(steerInput, handbrake);

        if (handbrake)
        {

            Vector3 velocityDir = rb.linearVelocity.normalized;
            if (velocityDir.sqrMagnitude < 0.1f) velocityDir = transform.forward;


            Vector3 lateralDir = Vector3.Cross(Vector3.up, velocityDir).normalized;

 
            float steerDir = -Mathf.Clamp(steerInput, -1f, 1f); 


            Vector3 lateralForce = lateralDir * driftKickForce * steerDir * driftKickSteerMultiplier;


            if (Mathf.Abs(steerDir) < 0.1f)
            {
                lateralForce += lateralDir * driftKickForce * 0.2f;
            }


            Vector3 rearPosition = (wheelRL.transform.position + wheelRR.transform.position) / 2f;
            rb.AddForceAtPosition(lateralForce, rearPosition);
        }






        rb.AddForce(-transform.up * downforce * rb.linearVelocity.sqrMagnitude * 0.001f);



        UpdateWheelPose(wheelFL, meshFL);
        UpdateWheelPose(wheelFR, meshFR);
        UpdateWheelPose(wheelRL, meshRL);
        UpdateWheelPose(wheelRR, meshRR);
        UpdateWheelVFX(wheelFL, vfxFL);
        UpdateWheelVFX(wheelFR, vfxFR);
        UpdateWheelVFX(wheelRL, vfxRL);
        UpdateWheelVFX(wheelRR, vfxRR);


    }
    void UpdateWheelVFX(WheelCollider wc, VisualEffect vfx)
    {
        if (vfx == null)
            return;

        WheelHit hit;
        bool grounded = wc.GetGroundHit(out hit);

        if (!grounded)
        {
            vfx.SetFloat("SmokeRate", 0f);
            return;
        }


        float sidewaysSlip = Mathf.Abs(hit.sidewaysSlip);


        float forwardSlip = Mathf.Abs(hit.forwardSlip);

        float slideAmount = 0f;


        if (sidewaysSlip > wheelSidewaysThreshold)
            slideAmount += sidewaysSlip;


        if (forwardSlip > wheelSpinSpeedThreshold)
            slideAmount += spinSmokeBoostPerWheel;


        if (slideAmount <= 0.1f)
        {
            vfx.SetFloat("SmokeRate", 0f);
            return;
        }


        float speed = rb.linearVelocity.magnitude;
        float t = Mathf.InverseLerp(0f, 40f, speed);

        float rate = Mathf.Lerp(0f, maxWheelSmokeRate, slideAmount / 10f);
        float life = Mathf.Lerp(maxWheelSmokeLife, minWheelSmokeLife, t);

        vfx.SetFloat("SmokeRate", rate);
        vfx.SetFloat("SmokeLife", life);
    }





    void HandleDrift(float steerInput, bool handbrake)
    {
        float speed = rb.linearVelocity.magnitude;
        float steerAmount = Mathf.Abs(steerInput);

        float driftFactor = 0f;
        if (speed >= driftSpeedThreshold)
        {
            driftFactor = Mathf.Clamp01((steerAmount - driftSteerThreshold) / (1f - driftSteerThreshold));
        }

        if (handbrake)
        {
            driftFactor = 1f;
        }

        float rearGripTarget = Mathf.Lerp(1f, driftGrip, driftFactor);
        float frontGripTarget = Mathf.Lerp(1f, 0.5f, driftFactor);
        float forwardGripRear = 0.9f;
        float forwardGripFront = 0.95f;

        AdjustFriction(wheelFL, frontGripTarget, forwardGripFront);
        AdjustFriction(wheelFR, frontGripTarget, forwardGripFront);
        AdjustFriction(wheelRL, rearGripTarget, forwardGripRear);
        AdjustFriction(wheelRR, rearGripTarget, forwardGripRear);
    }




    void AdjustFriction(WheelCollider wc, float sideGrip, float forwardGrip)
    {
        WheelFrictionCurve sideways = wc.sidewaysFriction;
        WheelFrictionCurve forward = wc.forwardFriction;

        sideways.stiffness = Mathf.Lerp(sideways.stiffness, sideGrip, Time.fixedDeltaTime * driftLerpSpeed);
        forward.stiffness = Mathf.Lerp(forward.stiffness, forwardGrip, Time.fixedDeltaTime * driftLerpSpeed);

        wc.sidewaysFriction = sideways;
        wc.forwardFriction = forward;
    }

    void UpdateWheelPose(WheelCollider wc, Transform wheelMesh)
    {
        if (wheelMesh == null) return;

        Vector3 pos;
        Quaternion rot;
        wc.GetWorldPose(out pos, out rot);

        wheelMesh.position = pos;
        wheelMesh.rotation = rot;
    }
}

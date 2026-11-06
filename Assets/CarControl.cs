using UnityEngine;
using TMPro;

public class CarControl : MonoBehaviour
{
    [Header("Car Properties")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float coastBrakeTorque = 30000000000f;
    public float maxSpeed = 20f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    public GameObject SteeringWheel;
    public GameObject Gas;
    public GameObject Brake;
    public GearShifter gs;
    public TextMeshPro label;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        Vector3 centerOfMass = rigidBody.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rigidBody.centerOfMass = centerOfMass;

        wheels = GetComponentsInChildren<WheelControl>();
    }

    void FixedUpdate()
    {
        Debug.Log("Gas dragging: " + Gas.GetComponent<Presser>().dragging);
        float vInput = 0f;
        if (Gas.GetComponent<Presser>().dragging == true)
        {
            if (gs.currentGear == 5)
            {
                vInput = 80;
            }
            else if (gs.currentGear == -1)
            {
                vInput = 0;
            }
            else
            {
                vInput = -90 - (gs.currentGear * 20);
            }
        }
        else
        {
            vInput = 0f;
        }

        float hInput = SteeringWheel.transform.localRotation.z;


        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));
        label.text = ((int)forwardSpeed * -1) + "";

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        foreach (var wheel in wheels)
        {

            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (Gas.GetComponent<Presser>().dragging == true)
            {
  
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }

                wheel.WheelCollider.brakeTorque = 0f;
            }
            else
            {
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = coastBrakeTorque;
            }

            if (Brake.GetComponent<Presser>().dragging == true)
            {
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = brakeTorque;
            }
        }
    }
}
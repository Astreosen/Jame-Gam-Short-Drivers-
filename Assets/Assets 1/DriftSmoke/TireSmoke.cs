using UnityEngine;

[RequireComponent(typeof(Transform))]
public class TireSmoke : MonoBehaviour
{
    [Header("References")]
    public WheelCollider wheel;                 // Wheel collider to sample slip from
    public ParticleSystem smoke;                // Particle system used as smoke emitter

    [Header("Emission")]
    public float slipThreshold = 0.35f;         // sideways slip threshold to start emitting
    public float maxEmission = 120f;            // particles per second at full slip
    public float lerpSpeed = 10f;               // how fast emission interpolates

    [Header("Particle Collision")]
    public float collisionRadius = 0.12f;       // local radius to test for collisions
    public LayerMask collisionMask = ~0;        // layers to collide with
    public float bounceDamp = 0.45f;            // how much velocity kept after bounce

    [Header("Spin Inheritance")]
    [Range(0f,1f)] public float spinInheritance = 0.65f; // how much tangential velocity inherits from wheel

    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (smoke == null)
        {
            Debug.LogError("[TireSmoke] Smoke ParticleSystem is not assigned.", this);
            enabled = false;
            return;
        }

        emission = smoke.emission;
        mainModule = smoke.main;
        emission.rateOverTime = 0f;
    }

    void LateUpdate()
    {
        if (wheel == null) return;

        // 1) Read wheel slip and control emission
        WheelHit hit;
        float targetRate = 0f;
        if (wheel.GetGroundHit(out hit))
        {
            float sideways = Mathf.Abs(hit.sidewaysSlip);
            float forward = Mathf.Abs(hit.forwardSlip);

            // Assetto-style: combine sideways slip and wheel slip to determine smoke
            float slipValue = Mathf.Max(sideways, forward * 0.6f);
            float t = Mathf.InverseLerp(slipThreshold, 1.6f, slipValue);
            targetRate = Mathf.Lerp(0f, maxEmission, t);
        }

        // Smooth emission changes
        var current = emission.rateOverTime.constant;
        float newRate = Mathf.Lerp(current, targetRate, Time.deltaTime * lerpSpeed);
        emission.rateOverTime = newRate;

        // 2) Particle-level collision and spin inheritance
        int count = smoke.particleCount;
        if (count == 0) return;

        if (particles == null || particles.Length < count) particles = new ParticleSystem.Particle[count];
        int got = smoke.GetParticles(particles, count);

        // compute wheel tangential velocity at contact to push smoke around
        Vector3 wheelCenter = transform.position;
        Vector3 wheelWorldUp = transform.up;

        // estimate rim tangential velocity from wheel angular velocity (approx)
        // if WheelCollider attached to Rigidbody, you can sample angularVelocity from it elsewhere
        // Here we approximate from local rotation speed if available
        Vector3 rimVelocity = Vector3.zero;
        var rb = wheel.attachedRigidbody;
        if (rb != null)
        {
            // approximate tangential at wheel center using rb.angularVelocity
            rimVelocity = Vector3.Cross(rb.angularVelocity, (particles.Length>0? (particles[0].position - wheelCenter) : Vector3.zero));
        }

        for (int i = 0; i < got; i++)
        {
            // simple overlap test for collisions
            Collider[] hits = Physics.OverlapSphere(particles[i].position, collisionRadius, collisionMask);
            if (hits.Length > 0)
            {
                // use the closest hit to compute normal
                Vector3 cp = hits[0].ClosestPoint(particles[i].position);
                Vector3 normal = (particles[i].position - cp).normalized;
                // reflect velocity and damp
                particles[i].velocity = Vector3.Reflect(particles[i].velocity, normal) * bounceDamp;

                // nudge particle slightly away so it doesn't get stuck inside geometry
                particles[i].position += normal * 0.01f;
            }

            // spin inheritance: push particles tangentially around wheel axis
            Vector3 toParticle = particles[i].position - wheelCenter;
            Vector3 tangential = Vector3.Cross(wheelWorldUp, toParticle).normalized;
            // scale by wheel forward speed / approximated rim velocity magnitude
            float spinVel = rimVelocity.magnitude;
            Vector3 inherit = tangential * spinVel * spinInheritance;
            particles[i].velocity += inherit * Time.deltaTime;
        }

        smoke.SetParticles(particles, got);
    }
}

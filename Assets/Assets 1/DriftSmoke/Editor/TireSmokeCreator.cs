#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class TireSmokeCreator
{
    [MenuItem("Tools/DriftSmoke/Create Wheel Smoke Prefab")]
    public static void CreateSmokePrefab()
    {
        // create root gameobject
        GameObject root = new GameObject("WheelSmoke_Prefab");
        GameObject smokeGO = new GameObject("Smoke");
        smokeGO.transform.SetParent(root.transform, false);
        smokeGO.transform.localPosition = Vector3.zero;

        // add particle system
        var ps = smokeGO.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.duration = 5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 2000;

        var emission = ps.emission;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.08f;
        shape.position = new Vector3(0f, -0.05f, 0f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.25f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0.7f,0.7f,0.7f), 1.0f) },
                  new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
        col.color = g;

        // renderer settings
        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.lengthScale = 1f;

        // Add the runtime script
        var smokeScript = root.AddComponent(typeof(TireSmoke)) as TireSmoke;
        var tireScript = smokeGO.AddComponent(typeof(ParticleSystem)) as ParticleSystem;

        // assign references
        smokeScript.smoke = ps;

        // create prefab asset
        string path = "Assets/DriftSmoke/WheelSmoke_Prefab.prefab";
        UnityEngine.Object prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        if (prefab != null)
        {
            Debug.Log("Prefab created at " + path);
        }
        else
        {
            Debug.LogError("Failed to create prefab");
        }

        // cleanup
        GameObject.DestroyImmediate(root);
    }
}
#endif

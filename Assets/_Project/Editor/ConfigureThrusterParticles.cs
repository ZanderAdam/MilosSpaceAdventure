using UnityEngine;
using UnityEditor;

namespace MilosAdventure.Editor
{
    public static class ConfigureThrusterParticles
    {
        [MenuItem("Tools/Configure Thruster Particles")]
        public static void Configure()
        {
            GameObject miloShip = GameObject.Find("MiloShip");
            if (miloShip == null)
            {
                Debug.LogError("MiloShip not found in scene!");
                return;
            }

            Transform engineTransform = miloShip.transform.Find("engine");
            if (engineTransform == null)
            {
                Debug.LogError("engine child not found!");
                return;
            }

            ParticleSystem ps = engineTransform.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogError("ParticleSystem not found on engine!");
                return;
            }

            // Configure main module
            var main = ps.main;
            main.startColor = new Color(0.6f, 0.9f, 1f, 1f);
            main.startSize = 0.4f;
            main.startLifetime = 0.4f;
            main.startSpeed = 4f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 100;
            main.playOnAwake = false;

            // Configure emission
            var emission = ps.emission;
            emission.rateOverTime = 40f;
            emission.enabled = false; // Controlled by script

            // Configure shape to emit backwards (left, since ship points right)
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;
            shape.rotation = new Vector3(0f, 180f, 0f); // Emit backwards

            // Configure color over lifetime for blue fire effect
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.6f, 0.9f, 1f), 0f),     // Bright cyan
                    new GradientColorKey(new Color(0.2f, 0.5f, 1f), 0.5f),   // Blue
                    new GradientColorKey(new Color(0f, 0.2f, 0.8f), 1f)      // Dark blue
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f), 
                    new GradientAlphaKey(0.6f, 0.5f),
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = gradient;

            // Configure size over lifetime for taper
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(0.5f, 0.7f);
            curve.AddKey(1f, 0.3f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            // Set the material for rendering
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                Material particleMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                    "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Particles/Particles-Unlit.mat");

                if (particleMaterial == null)
                {
                    // Fallback to built-in Default-Particle
                    particleMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
                }

                if (particleMaterial != null)
                {
                    renderer.sharedMaterial = particleMaterial;
                }
                else
                {
                    Debug.LogWarning("Could not find particle material, particles may not be visible!");
                }
            }

            // Also wire up the reference in PlayerShipController
            PlayerShipController controller = miloShip.GetComponent<PlayerShipController>();
            if (controller != null)
            {
                SerializedObject so = new SerializedObject(controller);
                SerializedProperty prop = so.FindProperty("thrusterParticles");
                if (prop != null)
                {
                    prop.objectReferenceValue = ps;
                    so.ApplyModifiedProperties();
                }
            }

            EditorUtility.SetDirty(miloShip);
            Debug.Log("Thruster ParticleSystem configured successfully with material!");
        }
    }
}

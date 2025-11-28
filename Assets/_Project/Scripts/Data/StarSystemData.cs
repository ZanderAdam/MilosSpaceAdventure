using System;
using System.Collections.Generic;
using UnityEngine;

namespace MilosAdventure.Data
{
    /// <summary>
    /// Root JSON structure for a star system.
    /// </summary>
    [Serializable]
    public class StarSystemJson
    {
        public SystemInfo system;
        public List<CelestialBodyJson> rootBodies;

        [NonSerialized]
        public List<CelestialBodyJson> bodies;

        public void FlattenHierarchy()
        {
            bodies = new List<CelestialBodyJson>();
            if (rootBodies != null)
            {
                foreach (var root in rootBodies)
                {
                    AddBodyAndChildren(root);
                }
            }
        }

        private void AddBodyAndChildren(CelestialBodyJson body)
        {
            bodies.Add(body);
            if (body.children != null)
            {
                foreach (var child in body.children)
                {
                    AddBodyAndChildren(child);
                }
            }
        }
    }

    /// <summary>
    /// Metadata for a star system.
    /// </summary>
    [Serializable]
    public class SystemInfo
    {
        public string id;
        public string name;
        public Bounds bounds;
    }

    [Serializable]
    public class Bounds
    {
        public float width;
        public float height;
    }

    /// <summary>
    /// Data for a single celestial body (star, planet, or moon).
    /// </summary>
    [Serializable]
    public class CelestialBodyJson
    {
        public string id;
        public string name;
        public string description;
        public string type;
        public string sprite;
        public string parentId;

        public float orbitDistance;
        public float orbitSpeed;
        public float orbitAngle;
        public float scale;
        public float rotation;
        public float rotationSpeed;

        public float luminosity;
        public int planetNumber;
        public string moonLetter;

        public float baseSize;
        public string fallbackColor;
        public string orbitRingColor;
        public float orbitRingWidth;

        public List<CelestialBodyJson> children; // Used for hierarchical JSON structure

        public bool IsStar => type?.ToLower() == "star";
        public bool IsPlanet => type?.ToLower() == "planet";
        public bool IsMoon => type?.ToLower() == "moon";
    }
}

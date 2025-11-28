using NUnit.Framework;
using UnityEngine;
using MilosAdventure.Data;

namespace MilosAdventure.Tests.EditMode.Data
{
    /// <summary>
    /// Unit tests for JSON data classes.
    /// Tests deserialization and helper properties.
    /// </summary>
    [TestFixture]
    public class JsonSerializationTests
    {
        [Test]
        public void StarSystemJson_Deserialization_ParsesCorrectly()
        {
            string json = @"{
                ""system"": {
                    ""id"": ""test-system"",
                    ""name"": ""Test System"",
                    ""bounds"": {
                        ""width"": 1920,
                        ""height"": 1080
                    }
                },
                ""rootBodies"": []
            }";

            StarSystemJson result = JsonUtility.FromJson<StarSystemJson>(json);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.system);
            Assert.AreEqual("test-system", result.system.id);
            Assert.AreEqual("Test System", result.system.name);
            Assert.AreEqual(1920f, result.system.bounds.width);
            Assert.AreEqual(1080f, result.system.bounds.height);
            Assert.IsNotNull(result.rootBodies);
        }

        [Test]
        public void CelestialBodyJson_Deserialization_ParsesStarCorrectly()
        {
            string json = @"{
                ""id"": ""star-1"",
                ""name"": ""Sol"",
                ""type"": ""star"",
                ""sprite"": ""star_yellow"",
                ""parentId"": """",
                ""orbitDistance"": 0,
                ""orbitSpeed"": 0,
                ""orbitAngle"": 0,
                ""scale"": 2.0,
                ""fallbackColor"": ""#FFFF00""
            }";

            CelestialBodyJson result = JsonUtility.FromJson<CelestialBodyJson>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual("star-1", result.id);
            Assert.AreEqual("Sol", result.name);
            Assert.AreEqual("star", result.type);
            Assert.AreEqual("star_yellow", result.sprite);
            Assert.AreEqual(2.0f, result.scale);
            Assert.AreEqual("#FFFF00", result.fallbackColor);
        }

        [Test]
        public void CelestialBodyJson_Deserialization_ParsesPlanetCorrectly()
        {
            string json = @"{
                ""id"": ""planet-1"",
                ""name"": ""Earth"",
                ""type"": ""planet"",
                ""sprite"": ""planet_blue"",
                ""parentId"": ""star-1"",
                ""orbitDistance"": 150,
                ""orbitSpeed"": 30,
                ""orbitAngle"": 0,
                ""scale"": 1.0,
                ""fallbackColor"": ""#0000FF""
            }";

            CelestialBodyJson result = JsonUtility.FromJson<CelestialBodyJson>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual("planet-1", result.id);
            Assert.AreEqual("Earth", result.name);
            Assert.AreEqual("planet", result.type);
            Assert.AreEqual("star-1", result.parentId);
            Assert.AreEqual(150f, result.orbitDistance);
            Assert.AreEqual(30f, result.orbitSpeed);
            Assert.AreEqual(0f, result.orbitAngle);
        }

        [Test]
        public void CelestialBodyJson_IsStar_ReturnsTrueForStar()
        {
            CelestialBodyJson body = new CelestialBodyJson
            {
                type = "star"
            };

            Assert.IsTrue(body.IsStar);
            Assert.IsFalse(body.IsPlanet);
            Assert.IsFalse(body.IsMoon);
        }

        [Test]
        public void CelestialBodyJson_IsPlanet_ReturnsTrueForPlanet()
        {
            CelestialBodyJson body = new CelestialBodyJson
            {
                type = "planet"
            };

            Assert.IsFalse(body.IsStar);
            Assert.IsTrue(body.IsPlanet);
            Assert.IsFalse(body.IsMoon);
        }

        [Test]
        public void CelestialBodyJson_IsMoon_ReturnsTrueForMoon()
        {
            CelestialBodyJson body = new CelestialBodyJson
            {
                type = "moon"
            };

            Assert.IsFalse(body.IsStar);
            Assert.IsFalse(body.IsPlanet);
            Assert.IsTrue(body.IsMoon);
        }

        [Test]
        public void CelestialBodyJson_TypeCaseInsensitive_WorksCorrectly()
        {
            CelestialBodyJson starUpper = new CelestialBodyJson { type = "STAR" };
            CelestialBodyJson starMixed = new CelestialBodyJson { type = "StAr" };

            Assert.IsTrue(starUpper.IsStar);
            Assert.IsTrue(starMixed.IsStar);
        }

        [Test]
        public void CelestialBodyJson_NullType_ReturnsFalseForAllChecks()
        {
            CelestialBodyJson body = new CelestialBodyJson
            {
                type = null
            };

            Assert.IsFalse(body.IsStar);
            Assert.IsFalse(body.IsPlanet);
            Assert.IsFalse(body.IsMoon);
        }

        [Test]
        public void SystemInfo_Deserialization_ParsesAllFields()
        {
            string json = @"{
                ""id"": ""system-alpha"",
                ""name"": ""Alpha Centauri"",
                ""bounds"": {
                    ""width"": 2560,
                    ""height"": 1440
                }
            }";

            MilosAdventure.Data.SystemInfo result = JsonUtility.FromJson<MilosAdventure.Data.SystemInfo>(json);

            Assert.IsNotNull(result);
            Assert.AreEqual("system-alpha", result.id);
            Assert.AreEqual("Alpha Centauri", result.name);
            Assert.AreEqual(2560f, result.bounds.width);
            Assert.AreEqual(1440f, result.bounds.height);
        }

        [Test]
        public void StarSystemJson_WithMultipleBodies_DeserializesAll()
        {
            string json = @"{
                ""system"": {
                    ""id"": ""sys-1"",
                    ""name"": ""Solar System"",
                    ""bounds"": {
                        ""width"": 1920,
                        ""height"": 1080
                    }
                },
                ""rootBodies"": [
                    {
                        ""id"": ""star-1"",
                        ""name"": ""Sun"",
                        ""type"": ""star"",
                        ""sprite"": ""star_yellow"",
                        ""parentId"": """",
                        ""orbitDistance"": 0,
                        ""orbitSpeed"": 0,
                        ""orbitAngle"": 0,
                        ""scale"": 2.0,
                        ""fallbackColor"": ""#FFFF00"",
                        ""children"": [
                            {
                                ""id"": ""planet-1"",
                                ""name"": ""Earth"",
                                ""type"": ""planet"",
                                ""sprite"": ""planet_blue"",
                                ""parentId"": ""star-1"",
                                ""orbitDistance"": 150,
                                ""orbitSpeed"": 30,
                                ""orbitAngle"": 0,
                                ""scale"": 1.0,
                                ""fallbackColor"": ""#0000FF""
                            }
                        ]
                    }
                ]
            }";

            StarSystemJson result = JsonUtility.FromJson<StarSystemJson>(json);
            result.FlattenHierarchy();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.bodies);
            Assert.AreEqual(2, result.bodies.Count);
            Assert.IsTrue(result.bodies[0].IsStar);
            Assert.IsTrue(result.bodies[1].IsPlanet);
        }
    }
}

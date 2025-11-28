using NUnit.Framework;
using UnityEngine;
using MilosAdventure.Logic;

namespace MilosAdventure.Tests.EditMode.Logic
{
    /// <summary>
    /// Unit tests for OrbitCalculator static class.
    /// Tests orbit position calculations, angle updates, and normalization.
    /// </summary>
    [TestFixture]
    public class OrbitCalculatorTests
    {
        private const float Delta = 0.0001f;

        [Test]
        public void CalculateOrbitPosition_Angle0_ReturnsPositionToRight()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 10f;
            float angle = 0f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(10f, result.x, Delta);
            Assert.AreEqual(0f, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_Angle90_ReturnsPositionAbove()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 10f;
            float angle = 90f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(0f, result.x, Delta);
            Assert.AreEqual(10f, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_Angle180_ReturnsPositionToLeft()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 10f;
            float angle = 180f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(-10f, result.x, Delta);
            Assert.AreEqual(0f, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_Angle270_ReturnsPositionBelow()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 10f;
            float angle = 270f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(0f, result.x, Delta);
            Assert.AreEqual(-10f, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_WithParentOffset_OffsetsPosition()
        {
            Vector2 parentPos = new Vector2(5f, 5f);
            float orbitDistance = 10f;
            float angle = 0f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(15f, result.x, Delta);
            Assert.AreEqual(5f, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_ZeroDistance_ReturnsParentPosition()
        {
            Vector2 parentPos = new Vector2(3f, 7f);
            float orbitDistance = 0f;
            float angle = 45f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(parentPos.x, result.x, Delta);
            Assert.AreEqual(parentPos.y, result.y, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_Angle45_ReturnsCorrectPosition()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 10f;
            float angle = 45f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            float expected = 10f * Mathf.Cos(45f * Mathf.Deg2Rad);
            Assert.AreEqual(expected, result.x, Delta);
            Assert.AreEqual(expected, result.y, Delta);
        }

        [Test]
        public void UpdateOrbitAngle_PositiveSpeed_IncreasesAngle()
        {
            float currentAngle = 0f;
            float orbitSpeed = 30f;
            float deltaTime = 1f;

            float result = OrbitCalculator.UpdateOrbitAngle(currentAngle, orbitSpeed, deltaTime);

            Assert.AreEqual(30f, result, Delta);
        }

        [Test]
        public void UpdateOrbitAngle_NegativeSpeed_DecreasesAngle()
        {
            float currentAngle = 90f;
            float orbitSpeed = -30f;
            float deltaTime = 1f;

            float result = OrbitCalculator.UpdateOrbitAngle(currentAngle, orbitSpeed, deltaTime);

            Assert.AreEqual(60f, result, Delta);
        }

        [Test]
        public void UpdateOrbitAngle_SmallDeltaTime_ReturnsSmallChange()
        {
            float currentAngle = 0f;
            float orbitSpeed = 180f;
            float deltaTime = 0.016f;

            float result = OrbitCalculator.UpdateOrbitAngle(currentAngle, orbitSpeed, deltaTime);

            float expected = 180f * 0.016f;
            Assert.AreEqual(expected, result, Delta);
        }

        [Test]
        public void NormalizeAngle_PositiveAngle_ReturnsWithinRange()
        {
            float angle = 45f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(45f, result, Delta);
        }

        [Test]
        public void NormalizeAngle_Angle360_ReturnsZero()
        {
            float angle = 360f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(0f, result, Delta);
        }

        [Test]
        public void NormalizeAngle_AngleAbove360_WrapsAround()
        {
            float angle = 450f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(90f, result, Delta);
        }

        [Test]
        public void NormalizeAngle_NegativeAngle_ConvertsToPositive()
        {
            float angle = -45f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(315f, result, Delta);
        }

        [Test]
        public void NormalizeAngle_LargeNegativeAngle_WrapsCorrectly()
        {
            float angle = -450f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(270f, result, Delta);
        }

        [Test]
        public void NormalizeAngle_MultipleRotations_HandlesCorrectly()
        {
            float angle = 720f;

            float result = OrbitCalculator.NormalizeAngle(angle);

            Assert.AreEqual(0f, result, Delta);
        }

        [Test]
        public void CalculateOrbitPosition_LargeDistance_ScalesCorrectly()
        {
            Vector2 parentPos = Vector2.zero;
            float orbitDistance = 1000f;
            float angle = 0f;

            Vector2 result = OrbitCalculator.CalculateOrbitPosition(parentPos, orbitDistance, angle);

            Assert.AreEqual(1000f, result.x, Delta);
        }

        [Test]
        public void UpdateOrbitAngle_ZeroSpeed_ReturnsUnchangedAngle()
        {
            float currentAngle = 45f;
            float orbitSpeed = 0f;
            float deltaTime = 1f;

            float result = OrbitCalculator.UpdateOrbitAngle(currentAngle, orbitSpeed, deltaTime);

            Assert.AreEqual(45f, result, Delta);
        }
    }
}

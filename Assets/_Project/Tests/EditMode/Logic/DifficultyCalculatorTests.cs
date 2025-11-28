using NUnit.Framework;
using MilosAdventure.Logic;

namespace MilosAdventure.Tests.EditMode.Logic
{
    /// <summary>
    /// Unit tests for DifficultyCalculator class.
    /// Tests difficulty tier calculations based on distance from galaxy center.
    /// </summary>
    [TestFixture]
    public class DifficultyCalculatorTests
    {
        private DifficultyCalculator _calculator;

        [SetUp]
        public void Setup()
        {
            _calculator = new DifficultyCalculator();
        }

        [Test]
        public void CalculateDifficulty_CenterDistance_ReturnsTier1()
        {
            var result = _calculator.CalculateDifficulty(0.0f);

            Assert.AreEqual(1, result.Tier);
            Assert.AreEqual("Addition to 10", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point1_ReturnsTier1()
        {
            var result = _calculator.CalculateDifficulty(0.1f);

            Assert.AreEqual(1, result.Tier);
            Assert.AreEqual("Addition to 10", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point2_ReturnsTier2()
        {
            var result = _calculator.CalculateDifficulty(0.2f);

            Assert.AreEqual(2, result.Tier);
            Assert.AreEqual("Multiplication to 5", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point3_ReturnsTier2()
        {
            var result = _calculator.CalculateDifficulty(0.3f);

            Assert.AreEqual(2, result.Tier);
            Assert.AreEqual("Multiplication to 5", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point4_ReturnsTier3()
        {
            var result = _calculator.CalculateDifficulty(0.4f);

            Assert.AreEqual(3, result.Tier);
            Assert.AreEqual("Multiplication to 10", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point5_ReturnsTier3()
        {
            var result = _calculator.CalculateDifficulty(0.5f);

            Assert.AreEqual(3, result.Tier);
            Assert.AreEqual("Multiplication to 10", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point6_ReturnsTier4()
        {
            var result = _calculator.CalculateDifficulty(0.6f);

            Assert.AreEqual(4, result.Tier);
            Assert.AreEqual("Division", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point7_ReturnsTier4()
        {
            var result = _calculator.CalculateDifficulty(0.7f);

            Assert.AreEqual(4, result.Tier);
            Assert.AreEqual("Division", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point8_ReturnsTier5()
        {
            var result = _calculator.CalculateDifficulty(0.8f);

            Assert.AreEqual(5, result.Tier);
            Assert.AreEqual("Mixed operations", result.Description);
        }

        [Test]
        public void CalculateDifficulty_Distance0Point9_ReturnsTier5()
        {
            var result = _calculator.CalculateDifficulty(0.9f);

            Assert.AreEqual(5, result.Tier);
            Assert.AreEqual("Mixed operations", result.Description);
        }

        [Test]
        public void CalculateDifficulty_EdgeDistance_ReturnsTier5()
        {
            var result = _calculator.CalculateDifficulty(1.0f);

            Assert.AreEqual(5, result.Tier);
            Assert.AreEqual("Mixed operations", result.Description);
        }

        [Test]
        public void CalculateDifficulty_NegativeDistance_ClampedToTier1()
        {
            var result = _calculator.CalculateDifficulty(-0.5f);

            Assert.AreEqual(1, result.Tier);
            Assert.AreEqual("Addition to 10", result.Description);
        }

        [Test]
        public void CalculateDifficulty_DistanceAbove1_ClampedToTier5()
        {
            var result = _calculator.CalculateDifficulty(1.5f);

            Assert.AreEqual(5, result.Tier);
            Assert.AreEqual("Mixed operations", result.Description);
        }

        [Test]
        public void GetTierNumber_Distance0Point5_Returns3()
        {
            int tier = _calculator.GetTierNumber(0.5f);

            Assert.AreEqual(3, tier);
        }

        [Test]
        public void GetTierNumber_Distance0Point0_Returns1()
        {
            int tier = _calculator.GetTierNumber(0.0f);

            Assert.AreEqual(1, tier);
        }

        [Test]
        public void GetTierNumber_Distance1Point0_Returns5()
        {
            int tier = _calculator.GetTierNumber(1.0f);

            Assert.AreEqual(5, tier);
        }

        [Test]
        public void CalculateDifficulty_BoundaryAt0Point19_ReturnsTier1()
        {
            var result = _calculator.CalculateDifficulty(0.19f);

            Assert.AreEqual(1, result.Tier);
        }

        [Test]
        public void CalculateDifficulty_BoundaryAt0Point39_ReturnsTier2()
        {
            var result = _calculator.CalculateDifficulty(0.39f);

            Assert.AreEqual(2, result.Tier);
        }

        [Test]
        public void CalculateDifficulty_BoundaryAt0Point59_ReturnsTier3()
        {
            var result = _calculator.CalculateDifficulty(0.59f);

            Assert.AreEqual(3, result.Tier);
        }

        [Test]
        public void CalculateDifficulty_BoundaryAt0Point79_ReturnsTier4()
        {
            var result = _calculator.CalculateDifficulty(0.79f);

            Assert.AreEqual(4, result.Tier);
        }
    }
}

using UnityEngine;

namespace MilosAdventure.Logic
{
    /// <summary>
    /// Pure C# logic for calculating difficulty tiers based on distance from galaxy center.
    /// </summary>
    public class DifficultyCalculator
    {
        /// <summary>
        /// Represents a difficulty tier with its properties.
        /// </summary>
        public struct DifficultyTier
        {
            public int Tier;
            public string Description;

            public DifficultyTier(int tier, string description)
            {
                Tier = tier;
                Description = description;
            }
        }

        /// <summary>
        /// Calculates the difficulty tier based on distance from galaxy center.
        /// </summary>
        /// <param name="normalizedDistance">Distance from galaxy center (0.0 = center, 1.0 = edge)</param>
        /// <returns>Difficulty tier information</returns>
        public DifficultyTier CalculateDifficulty(float normalizedDistance)
        {
            normalizedDistance = Mathf.Clamp01(normalizedDistance);

            if (normalizedDistance < 0.2f)
                return new DifficultyTier(1, "Addition to 10");

            if (normalizedDistance < 0.4f)
                return new DifficultyTier(2, "Multiplication to 5");

            if (normalizedDistance < 0.6f)
                return new DifficultyTier(3, "Multiplication to 10");

            if (normalizedDistance < 0.8f)
                return new DifficultyTier(4, "Division");

            return new DifficultyTier(5, "Mixed operations");
        }

        /// <summary>
        /// Gets just the tier number for a given distance.
        /// </summary>
        /// <param name="normalizedDistance">Distance from galaxy center (0.0-1.0)</param>
        /// <returns>Tier number (1-5)</returns>
        public int GetTierNumber(float normalizedDistance)
        {
            return CalculateDifficulty(normalizedDistance).Tier;
        }
    }
}

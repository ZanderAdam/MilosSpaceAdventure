using UnityEngine;

namespace MilosAdventure.Logic
{
    /// <summary>
    /// Pure C# logic for orbital mechanics calculations.
    /// Provides mathematical functions for orbit positions and angle updates.
    /// </summary>
    public static class OrbitCalculator
    {
        /// <summary>
        /// Calculates the position of an orbiting body in 2D space.
        /// </summary>
        /// <param name="parentPosition">Position of the parent body</param>
        /// <param name="orbitDistance">Radius of the orbit</param>
        /// <param name="currentAngle">Current angle in degrees (0 = right, increases counter-clockwise)</param>
        /// <returns>Calculated position in 2D space</returns>
        public static Vector2 CalculateOrbitPosition(
            Vector2 parentPosition,
            float orbitDistance,
            float currentAngle)
        {
            float angleRad = currentAngle * Mathf.Deg2Rad;
            float x = parentPosition.x + Mathf.Cos(angleRad) * orbitDistance;
            float y = parentPosition.y + Mathf.Sin(angleRad) * orbitDistance;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Updates the orbital angle based on orbit speed and time.
        /// </summary>
        /// <param name="currentAngle">Current angle in degrees</param>
        /// <param name="orbitSpeed">Angular velocity in degrees per second</param>
        /// <param name="deltaTime">Time step</param>
        /// <returns>New angle in degrees</returns>
        public static float UpdateOrbitAngle(
            float currentAngle,
            float orbitSpeed,
            float deltaTime)
        {
            return currentAngle + orbitSpeed * deltaTime;
        }

        /// <summary>
        /// Normalizes an angle to the range [0, 360).
        /// </summary>
        /// <param name="angle">Angle in degrees</param>
        /// <returns>Normalized angle in degrees</returns>
        public static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }
    }
}

using UnityEngine;

namespace MilosAdventure.UI.Utils
{
    /// <summary>
    /// Utility class for converting world coordinates to UI Toolkit coordinates.
    /// Handles coordinate system differences between world space and UI Toolkit.
    /// </summary>
    public static class CoordinateConverter
    {
        /// <summary>
        /// Converts a world space position to UI Toolkit screen coordinates.
        /// World space: origin (0,0) at center, Y-up
        /// UI Toolkit: origin (0,0) at top-left, Y-down
        /// </summary>
        /// <param name="worldPos">Position in world space (center origin)</param>
        /// <param name="worldSize">Size of the world bounds</param>
        /// <param name="uiSize">Size of the UI container in pixels</param>
        /// <returns>Position in UI Toolkit coordinates (top-left origin)</returns>
        public static Vector2 WorldToUIPosition(Vector3 worldPos, Vector2 worldSize, Vector2 uiSize)
        {
            Vector2 normalized = (new Vector2(worldPos.x, worldPos.y) + worldSize / 2f) / worldSize;

            normalized.y = 1f - normalized.y;

            return normalized * uiSize;
        }

        /// <summary>
        /// Converts a UI Toolkit screen position back to world space coordinates.
        /// </summary>
        /// <param name="uiPos">Position in UI Toolkit coordinates (top-left origin)</param>
        /// <param name="worldSize">Size of the world bounds</param>
        /// <param name="uiSize">Size of the UI container in pixels</param>
        /// <returns>Position in world space (center origin)</returns>
        public static Vector2 UIToWorldPosition(Vector2 uiPos, Vector2 worldSize, Vector2 uiSize)
        {
            Vector2 normalized = uiPos / uiSize;

            normalized.y = 1f - normalized.y;

            return (normalized * worldSize) - (worldSize / 2f);
        }

        /// <summary>
        /// Calculates the UI scale factor for a given world object based on its size.
        /// </summary>
        /// <param name="worldObjectSize">Size of the object in world units</param>
        /// <param name="worldSize">Total size of the world bounds</param>
        /// <param name="uiSize">Size of the UI container in pixels</param>
        /// <returns>Pixel size for the UI element</returns>
        public static float WorldToUIScale(float worldObjectSize, Vector2 worldSize, Vector2 uiSize)
        {
            float averageWorldSize = (worldSize.x + worldSize.y) / 2f;
            float averageUISize = (uiSize.x + uiSize.y) / 2f;

            return (worldObjectSize / averageWorldSize) * averageUISize;
        }
    }
}

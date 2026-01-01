using UnityEngine;

namespace MilosAdventure.UI.Utils
{
    /// <summary>
    /// Utility class for calculating DPI-adaptive touch target sizes.
    /// Ensures touch targets meet minimum physical size requirements across devices.
    /// </summary>
    public static class TouchTargetCalculator
    {
        private const float REFERENCE_DPI = 160f; // Standard Android mdpi
        private const float MIN_PHYSICAL_POINTS = 44f; // Apple HIG minimum

        /// <summary>
        /// Calculates the pixel size for a touch target based on physical points and device DPI.
        /// </summary>
        /// <param name="physicalPoints">Physical size in points (default: 44pt per Apple HIG)</param>
        /// <returns>Touch target size in pixels for the current device</returns>
        public static float GetTouchTargetPixels(float physicalPoints = MIN_PHYSICAL_POINTS)
        {
            float deviceDpi = Screen.dpi > 0 ? Screen.dpi : REFERENCE_DPI;
            return (physicalPoints / REFERENCE_DPI) * deviceDpi;
        }

        /// <summary>
        /// Gets an adaptive touch target size based on UI element density.
        /// Useful for planetary icons that may be closer together.
        /// </summary>
        /// <param name="densityFactor">Density multiplier (1.0 = normal, 0.8 = denser layout)</param>
        /// <returns>Touch target size in pixels adjusted for density</returns>
        public static float GetAdaptiveTouchTarget(float densityFactor = 1f)
        {
            float baseSize = GetTouchTargetPixels();
            return baseSize * Mathf.Clamp(densityFactor, 0.7f, 1.5f);
        }

        /// <summary>
        /// Gets the touch target size as a Vector2 for use with UI Toolkit style properties.
        /// </summary>
        /// <param name="physicalPoints">Physical size in points</param>
        /// <returns>Square touch target dimensions in pixels</returns>
        public static Vector2 GetTouchTargetVector(float physicalPoints = MIN_PHYSICAL_POINTS)
        {
            float size = GetTouchTargetPixels(physicalPoints);
            return new Vector2(size, size);
        }
    }
}

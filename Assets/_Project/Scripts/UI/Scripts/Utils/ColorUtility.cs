using UnityEngine;

namespace MilosAdventure.UI.Utils
{
    /// <summary>
    /// Utility class for parsing color values from various formats.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// Parses a hex color string to a Unity Color.
        /// Supports formats: "#RRGGBB", "#RRGGBBAA", "RRGGBB", "RRGGBBAA"
        /// </summary>
        /// <param name="hex">Hex color string (e.g., "#FF5733" or "FF5733")</param>
        /// <returns>Parsed Color, or white if parsing fails</returns>
        public static Color ParseHex(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return Color.white;

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (UnityEngine.ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }

            Debug.LogWarning($"Failed to parse hex color: {hex}. Returning white.");
            return Color.white;
        }

        /// <summary>
        /// Tries to parse a hex color string to a Unity Color.
        /// </summary>
        /// <param name="hex">Hex color string</param>
        /// <param name="color">Parsed color output</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParseHex(string hex, out Color color)
        {
            color = Color.white;

            if (string.IsNullOrEmpty(hex))
                return false;

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            return UnityEngine.ColorUtility.TryParseHtmlString(hex, out color);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MilosAdventure.UI.Utils
{
    /// <summary>
    /// Utility class for loading and caching sprites for UI Toolkit.
    /// Provides extension methods for setting sprite backgrounds on VisualElements.
    /// </summary>
    public static class SpriteLoader
    {
        private static readonly Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// Loads a sprite from Resources with caching to avoid redundant loads.
        /// </summary>
        /// <param name="resourcePath">Path relative to Resources folder (without extension)</param>
        /// <returns>Cached Sprite or null if not found</returns>
        public static Sprite LoadSprite(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
                return null;

            if (_spriteCache.TryGetValue(resourcePath, out Sprite cachedSprite))
                return cachedSprite;

            Sprite sprite = Resources.Load<Sprite>(resourcePath);
            if (sprite != null)
            {
                _spriteCache[resourcePath] = sprite;
            }

            return sprite;
        }

        /// <summary>
        /// Sets a sprite as the background image for a VisualElement.
        /// </summary>
        /// <param name="element">VisualElement to apply sprite to</param>
        /// <param name="sprite">Sprite to set as background</param>
        public static void SetBackgroundSprite(this VisualElement element, Sprite sprite)
        {
            if (sprite != null)
            {
                element.style.backgroundImage = new StyleBackground(sprite);
                element.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            }
        }

        /// <summary>
        /// Clears the sprite cache to free memory.
        /// </summary>
        public static void ClearCache()
        {
            _spriteCache.Clear();
        }

        /// <summary>
        /// Gets the current cache size for debugging.
        /// </summary>
        public static int GetCacheSize()
        {
            return _spriteCache.Count;
        }
    }
}

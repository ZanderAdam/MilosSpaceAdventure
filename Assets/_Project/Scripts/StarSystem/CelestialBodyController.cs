using UnityEngine;
using MilosAdventure.Data;
using MilosAdventure.Logic;

/// <summary>
/// Controls a single celestial body (star, planet, or moon).
/// Handles sprite rendering, orbital motion, and initialization from JSON.
/// </summary>
[DefaultExecutionOrder(-100)]
public class CelestialBodyController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CelestialBodyJson _data;
    private StarSystemRenderer _system;
    private float _currentAngle;

    public string Id => _data?.id;
    public string BodyName => _data?.name;
    public string Description => _data?.description ?? "";
    public bool IsStar => _data?.IsStar ?? false;

    public float VisualRadius
    {
        get
        {
            if (spriteRenderer == null) return 0f;

            return Mathf.Max(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y) * 0.5f;
        }
    }

    public Vector2 GetOrbitalVelocity()
    {
        if (_data == null || _data.IsStar)
            return Vector2.zero;

        float angleRad = _currentAngle * Mathf.Deg2Rad;
        float tangentAngle = angleRad + Mathf.PI * 0.5f;

        float adjustedDistance = _data.orbitDistance * _system.OrbitDistanceMultiplier;
        float linearSpeed = _data.orbitSpeed * Mathf.Deg2Rad * adjustedDistance;

        return new Vector2(
            Mathf.Cos(tangentAngle),
            Mathf.Sin(tangentAngle)
        ) * linearSpeed;
    }

    /// <summary>
    /// Initialize this celestial body from JSON data.
    /// </summary>
    public void Initialize(CelestialBodyJson data, StarSystemRenderer system)
    {
        _data = data;
        _system = system;
        _currentAngle = data.orbitAngle;

        var sprite = StarSystemLoader.Instance.GetSprite(data.sprite);
        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        else
        {
            // Create a simple circle sprite as fallback
            spriteRenderer.sprite = CreateCircleSprite(64);

            // Apply fallback color if specified
            if (!string.IsNullOrEmpty(data.fallbackColor))
            {
                if (ColorUtility.TryParseHtmlString(data.fallbackColor, out Color c))
                    spriteRenderer.color = c;
            }
        }

        transform.localScale = Vector3.one * data.scale * system.ScaleMultiplier;

        if (!data.IsStar)
        {
            SetupGravityLockTrigger();
        }

        UpdatePosition();
    }

    private void SetupGravityLockTrigger()
    {
        var collider = gameObject.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
        collider.radius = VisualRadius * 2f / transform.localScale.x;
    }

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                // Create smooth circle with anti-aliasing
                float alpha = 1f - Mathf.Clamp01((distance - radius + 1) / 2f);
                pixels[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void FixedUpdate()
    {
        if (_data == null || _data.IsStar) return;

        _currentAngle = OrbitCalculator.UpdateOrbitAngle(_currentAngle, _data.orbitSpeed, Time.fixedDeltaTime);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (_data.IsStar)
        {
            transform.position = Vector3.zero;
            return;
        }

        Vector2 parentPos = _system.GetParentPosition(_data.parentId);
        float adjustedDistance = _data.orbitDistance * _system.OrbitDistanceMultiplier;
        Vector2 position = OrbitCalculator.CalculateOrbitPosition(parentPos, adjustedDistance, _currentAngle);
        transform.position = new Vector3(position.x, position.y, 0);
    }
}

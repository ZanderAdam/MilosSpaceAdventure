using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Debug HUD for displaying ship information and testing save/load functionality.
/// </summary>
public class DebugHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerShipController _playerShip;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private TextMeshProUGUI _positionText;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _loadButton;

    [Header("Settings")]
    [SerializeField] private bool _showDebugInfo = true;
    [SerializeField] private int _updateFrequency = 10;

    private int _frameCount;

    private void Start()
    {
        SetupButtons();
        UpdateVisibility();
    }

    private void Update()
    {
        if (!_showDebugInfo || _playerShip == null) return;

        _frameCount++;
        if (_frameCount >= _updateFrequency)
        {
            _frameCount = 0;
            UpdateDisplayInfo();
        }
    }

    private void SetupButtons()
    {
        if (_saveButton != null)
        {
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        if (_loadButton != null)
        {
            _loadButton.onClick.AddListener(OnLoadButtonClicked);
        }
    }

    private void UpdateDisplayInfo()
    {
        if (_speedText != null)
        {
            float speed = _playerShip.CurrentSpeed;
            _speedText.text = $"Speed: {speed:F2}";
        }

        if (_positionText != null)
        {
            Vector3 pos = _playerShip.transform.position;
            _positionText.text = $"Position: ({pos.x:F1}, {pos.y:F1})";
        }
    }

    private void UpdateVisibility()
    {
        bool shouldShow = _showDebugInfo;

        if (_speedText != null)
            _speedText.gameObject.SetActive(shouldShow);

        if (_positionText != null)
            _positionText.gameObject.SetActive(shouldShow);
    }

    private void OnSaveButtonClicked()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager instance not found!");
            return;
        }

        if (_playerShip == null)
        {
            Debug.LogWarning("PlayerShip reference not set!");
            return;
        }

        SaveData saveData = new SaveData
        {
            currentSystemId = "test_system",
            shipX = _playerShip.transform.position.x,
            shipY = _playerShip.transform.position.y,
            shipRotation = _playerShip.transform.rotation.eulerAngles.z
        };

        SaveManager.Instance.Save(saveData);
        Debug.Log("Game saved successfully");
    }

    private void OnLoadButtonClicked()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("SaveManager instance not found!");
            return;
        }

        if (_playerShip == null)
        {
            Debug.LogWarning("PlayerShip reference not set!");
            return;
        }

        SaveData saveData = SaveManager.Instance.Load();

        if (saveData == null)
        {
            Debug.LogWarning("No save data found!");
            return;
        }

        Vector3 loadedPosition = new Vector3(saveData.shipX, saveData.shipY, 0f);
        Quaternion loadedRotation = Quaternion.Euler(0f, 0f, saveData.shipRotation);

        _playerShip.transform.position = loadedPosition;
        _playerShip.transform.rotation = loadedRotation;

        Debug.Log($"Game loaded successfully - Position: {loadedPosition}, Rotation: {saveData.shipRotation}");
    }

    private void OnValidate()
    {
        _updateFrequency = Mathf.Max(1, _updateFrequency);

        if (Application.isPlaying)
        {
            UpdateVisibility();
        }
    }

    /// <summary>
    /// Toggle debug info visibility at runtime.
    /// </summary>
    public void ToggleDebugInfo()
    {
        _showDebugInfo = !_showDebugInfo;
        UpdateVisibility();
    }
}

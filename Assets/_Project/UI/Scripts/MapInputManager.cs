using UnityEngine;

namespace MilosAdventure.UI
{
    /// <summary>
    /// Singleton manager for interactive system map input and state.
    /// Handles M key toggle and player control suspension when map is open.
    /// </summary>
    public class MapInputManager : MonoBehaviour
    {
        public static MapInputManager Instance { get; private set; }

        [Header("References")]
        [SerializeField]
        [Tooltip("Reference to the interactive map controller")]
        private InteractiveMapController mapController;

        [SerializeField]
        [Tooltip("Reference to the player ship controller to disable during map view")]
        private PlayerShipController playerShip;

        [SerializeField]
        [Tooltip("Reference to mobile controls to disable during map view")]
        private MobileControls mobileControls;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            // TODO: Migrate to Unity Input System (new input system) per project guidelines
            // Currently using legacy Input.GetKeyDown for consistency with PlayerShipController
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMap();
            }
        }

        /// <summary>
        /// Toggles the interactive map on/off, disabling player controls when open.
        /// Called by M key press (PC) or minimap tap (mobile).
        /// </summary>
        public void ToggleMap()
        {
            if (mapController == null)
            {
                Debug.LogWarning("[MapInputManager] No map controller assigned");
                return;
            }

            if (mapController.IsVisible)
            {
                CloseMap();
            }
            else
            {
                OpenMap();
            }
        }

        private void OnDisable()
        {
            // Future-proofing: Clean up any event subscriptions here
            // Ensures controls re-enabled if component disabled while map is open
            if (mapController != null && mapController.IsVisible)
            {
                CloseMap();
            }
        }

        private void OpenMap()
        {
            if (mapController == null) return;

            mapController.ShowMap();

            if (playerShip != null)
            {
                playerShip.enabled = false;
            }

            if (mobileControls != null)
            {
                // Reset input state before disabling to prevent stuck inputs
                if (mobileControls.TryGetComponent<MobileControls>(out var controls))
                {
                    controls.ResetInputState();
                }
                mobileControls.enabled = false;
            }
        }

        private void CloseMap()
        {
            if (mapController == null) return;

            try
            {
                mapController.HideMap();
            }
            finally
            {
                // ALWAYS re-enable controls, even if HideMap() throws exception
                if (playerShip != null)
                {
                    playerShip.enabled = true;
                }

                if (mobileControls != null)
                {
                    mobileControls.enabled = true;
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}

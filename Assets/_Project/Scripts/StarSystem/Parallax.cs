using UnityEngine;

namespace MilosAdventure
{
    public class ParallaxEffect : MonoBehaviour
    {
        public float parallaxSpeed = 0.5f; // Adjust this value for different speeds
        public Transform cameraTransform;
        private Vector3 lastCameraPosition;

        void LateUpdate()
        {
            // Calculate how much the camera has moved since the last frame
            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

            // Move the background layer based on the camera's movement and parallax speed
            transform.position += new Vector3(deltaMovement.x * parallaxSpeed, deltaMovement.y * parallaxSpeed, 0);

            // Update the last camera position for the next frame
            lastCameraPosition = cameraTransform.position;
        }
    }
}

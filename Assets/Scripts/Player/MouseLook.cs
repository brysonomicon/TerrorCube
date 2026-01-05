using UnityEngine;
using UnityEngine.InputSystem;

namespace TerrorCube.Player
{
    public class MouseLook : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform playerBody;
        
        [Header("Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private float topClamp = -90f;
        [SerializeField] private float bottomClamp = 90f;

        /// <summary>
        /// The raw X/Y data from the mouse
        /// </summary>
        private Vector2 _lookInput;

        /// <summary>
        /// Current vertical angle of mouse input
        /// </summary>
        private float _xRotation = 0f;

        /// <summary>
        /// Locks the cursor to the center of the screen and hides it.
        /// </summary>
        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        /// <summary>
        /// Callback function triggered by the Input System when the "Look" action is performed.
        /// Reads the mouse delta values.
        /// </summary>
        /// <param name="context">The input context containing the value.</param>
        public void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Processes the look rotation every frame.
        /// Rotates the camera vertically (local X axis) and the player body horizontally (global Y axis).
        /// </summary>
        private void Update()
        {
            float mouseX = _lookInput.x * mouseSensitivity * Time.deltaTime;
            float mouseY = _lookInput.y * mouseSensitivity * Time.deltaTime;

            _xRotation += mouseY;
            _xRotation = Mathf.Clamp(_xRotation, topClamp, bottomClamp);

            transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

            if (playerBody != null)
            {
                playerBody.Rotate(Vector3.up * mouseX);
            }
        }
    }
}

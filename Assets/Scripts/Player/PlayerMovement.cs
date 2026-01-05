using UnityEngine;
using UnityEngine.InputSystem;

namespace TerrorCube.Player
{
    /// <summary>
    /// Handles character movement logic including walking and gravity using the CharacterController component.
    /// Handles standard FPS movement. Quake-physics not yet implemented.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float gravity = -9.81f;

        /// <summary>
        /// Reference to the Unity component that handles collision and movement
        /// </summary>
        private CharacterController _controller;
        
        /// <summary>
        /// The raw WASD input from the player
        /// </summary>
        private Vector2 _moveInput;
        
        /// <summary>
        /// The current vertical velocity.
        /// </summary>
        private Vector3 _velocity;

        /// <summary>
        /// Initializes the controller reference.
        /// </summary>
        private void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        /// <summary>
        /// Callback for the Input System "Move" action
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Processes movement physics every frame.
        /// Resets vertical velocity if grounded, calculates local movement direction based on input,
        /// applies movement velocity, and applies gravity over time.
        /// </summary>
        private void Update()
        {
            if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            Vector3 move = transform.right * _moveInput.x + transform.forward * _moveInput.y;

            _controller.Move(move * moveSpeed * Time.deltaTime);

            _velocity.y += gravity * Time.deltaTime;

            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}

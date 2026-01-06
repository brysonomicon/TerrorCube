using UnityEngine;
using UnityEngine.InputSystem;

namespace TerrorCube.Player
{
    /// <summary>
    /// Handles character movement logic using Quake III Arena style kinematic physics.
    /// Ported from the specific setup to match "tight and responsive" strafe jumping.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Quake Settings")]
        [SerializeField] private float gravityForce = 20.0f;
        [SerializeField] private float friction = 6.0f; // Ground friction

        [Header("Movement Stats")]
        [SerializeField] private float moveSpeed = 7.0f;                // Ground move speed
        [SerializeField] private float runAcceleration = 14.0f;         // Ground accel
        [SerializeField] private float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
        [SerializeField] private float airAcceleration = 2.0f;          // Air accel
        [SerializeField] private float airDeacceleration = 2.0f;        // Deacceleration experienced when opposite strafing
        [SerializeField] private float airControl = 0.3f;               // How precise air control is
        [SerializeField] private float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when side strafing
        [SerializeField] private float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
        [SerializeField] private float jumpSpeed = 8.0f;                // The speed at which the character's up axis gains when hitting jump
        [SerializeField] private bool holdJumpToBhop = false;           // When enabled allows player to just hold jump button to keep on bhopping perfectly. Beware: smells like casual.

        [Header("Sliding Settings")]
        [SerializeField] private float slideFriction = 0f;
        [SerializeField] private float slideHeight = 1.0f;
        [SerializeField] private float normalHeight = 2.0f;
        [SerializeField] private float slideTransitionSpeed = 10f;

        private CharacterController _controller;
        
        // Inputs
        private Vector2 _moveInput;
        private bool _wishJump;
        private bool _wishCrouch;

        // Physics State
        private Vector3 _velocity;
        
        // Debugging
        private float _playerTopVelocity = 0.0f;
        
        // Helper to emulate the "Cmd" class from snippet
        private float _cmdForwardMove;
        private float _cmdRightMove;

        // Styles
        private GUIStyle startStyle;

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (holdJumpToBhop)
            {
                _wishJump = context.ReadValueAsButton();
            }
            else
            {
                if (context.performed) _wishJump = true;
                if (context.canceled) _wishJump = false;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            _wishCrouch = context.ReadValueAsButton();
        }

        private void Update()
        {
            // Handle Crouch Height
            float targetHeight = _wishCrouch ? slideHeight : normalHeight;
            _controller.height = Mathf.Lerp(_controller.height, targetHeight, slideTransitionSpeed * Time.deltaTime);
            // Re-center center to avoid sinking into ground
            _controller.center = new Vector3(0, _controller.height / 2f, 0);

            // Update input values for the frame
            _cmdForwardMove = _moveInput.y;
            _cmdRightMove = _moveInput.x;

            if (_controller.isGrounded)
                GroundMove();
            else
                AirMove();

            _controller.Move(_velocity * Time.deltaTime);

            // Calculate top velocity for debug
            Vector3 udp = _velocity;
            udp.y = 0.0f;
            if (udp.magnitude > _playerTopVelocity)
                _playerTopVelocity = udp.magnitude;
        }

        private void SetMovementDir(out Vector3 wishDir)
        {
            // Transform input to world space
            wishDir = transform.right * _cmdRightMove + transform.forward * _cmdForwardMove;
        }

        private void AirMove()
        {
            Vector3 wishDir;
            float accel;

            SetMovementDir(out wishDir);

            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            wishDir.Normalize();

            // CPM: Aircontrol
            float wishSpeed2 = wishSpeed;
            if (Vector3.Dot(_velocity, wishDir) < 0)
                accel = airDeacceleration;
            else
                accel = airAcceleration;

            // If the player is ONLY strafing left or right
            if (_cmdForwardMove == 0 && _cmdRightMove != 0)
            {
                if (wishSpeed > sideStrafeSpeed)
                    wishSpeed = sideStrafeSpeed;
                accel = sideStrafeAcceleration;
            }

            Accelerate(wishDir, wishSpeed, accel);
            
            if (airControl > 0)
                ApplyAirControl(wishDir, wishSpeed2);
            // !CPM

            // Apply gravity
            _velocity.y -= gravityForce * Time.deltaTime;
        }

        // CPM Air Control Logic
        private void ApplyAirControl(Vector3 wishDir, float wishSpeed)
        {
            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(_cmdForwardMove) < 0.001f || Mathf.Abs(wishSpeed) < 0.001f)
                return;

            float zSpeed = _velocity.y;
            _velocity.y = 0;
            
            float speed = _velocity.magnitude;
            _velocity.Normalize();

            float dot = Vector3.Dot(_velocity, wishDir);
            float k = 32f;
            k *= airControl * dot * dot * Time.deltaTime;

            // Change direction while slowing down
            if (dot > 0)
            {
                _velocity.x = _velocity.x * speed + wishDir.x * k;
                _velocity.y = _velocity.y * speed + wishDir.y * k;
                _velocity.z = _velocity.z * speed + wishDir.z * k;

                _velocity.Normalize();
            }

            _velocity.x *= speed;
            _velocity.z *= speed; 
            
            // Restore vertical velocity
            _velocity.y = zSpeed;
        }

        private void GroundMove()
        {
            Vector3 wishDir;

            // Crouch Slide Logic: No friction if crouching
            bool isSliding = _wishCrouch;

            if (isSliding)
            {
                ApplyFriction(slideFriction);
            }
            else if (!_wishJump)
            {
                ApplyFriction(1.0f);
            }
            else
            {
                ApplyFriction(0);
            }

            SetMovementDir(out wishDir);
            wishDir.Normalize();
            
            float wishSpeed = wishDir.magnitude;
            wishSpeed *= moveSpeed;

            Accelerate(wishDir, wishSpeed, runAcceleration);

            // Secret Sauce: Apply Air Control logic while sliding to allow sharp turns without losing speed
            if (isSliding)
            {
                ApplyAirControl(wishDir, wishSpeed);
            }

            // Reset the gravity velocity
            _velocity.y = -gravityForce * Time.deltaTime; 
            
            if (_wishJump)
            {
                _velocity.y = jumpSpeed; 
                if (!holdJumpToBhop) _wishJump = false; 
            }
        }

        private void ApplyFriction(float t)
        {
            Vector3 vec = _velocity;
            vec.y = 0;
            float speed = vec.magnitude;
            float drop = 0;

            if (_controller.isGrounded)
            {
                float control = speed < runDeacceleration ? runDeacceleration : speed;
                drop = control * friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0) newSpeed = 0;
            if (speed > 0) newSpeed /= speed;

            _velocity.x *= newSpeed;
            _velocity.z *= newSpeed;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel)
        {
            float currentSpeed = Vector3.Dot(_velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            
            if (addSpeed <= 0)
                return;
            
            float accelSpeed = accel * Time.deltaTime * wishSpeed;
            if (accelSpeed > addSpeed)
                accelSpeed = addSpeed;

            _velocity.x += accelSpeed * wishDir.x;
            _velocity.z += accelSpeed * wishDir.z;
        }

        private void OnGUI()
        {
            if (startStyle == null)
            {
                startStyle = new GUIStyle(GUI.skin.label);
                startStyle.fontSize = 24;
                startStyle.fontStyle = FontStyle.Bold;
                startStyle.normal.textColor = Color.white;
            }

            GUI.backgroundColor = new Color(0, 0, 0, 0.5f);
            GUI.Box(new Rect(5, 5, 300, 150), "Debug Info");

            float speed = new Vector2(_velocity.x, _velocity.z).magnitude;
            GUI.Label(new Rect(10, 30, 290, 30), $"Speed: {speed:F2} (ups)", startStyle);
            GUI.Label(new Rect(10, 60, 290, 30), $"Top Speed: {_playerTopVelocity:F2}", startStyle);
            GUI.Label(new Rect(10, 90, 290, 30), $"Grounded: {_controller.isGrounded}", startStyle);
            
            string inputState = $"Cmd: Fwd {_cmdForwardMove:F1} | Right {_cmdRightMove:F1}";
            GUI.Label(new Rect(10, 120, 290, 30), inputState, startStyle);
        }
    }
}
